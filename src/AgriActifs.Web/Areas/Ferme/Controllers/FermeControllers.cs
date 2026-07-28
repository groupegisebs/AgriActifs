using AgriActifs.Web.Data;
using AgriActifs.Web.Models.Ferme;
using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgriActifs.Web.Areas.Ferme.Controllers;

[Area("Ferme")]
[Authorize]
public abstract class FermeControllerBase(IExploitationContextService exploitationContext) : Controller
{
    protected Task<int> GetExploitationIdAsync(CancellationToken cancellationToken = default) =>
        exploitationContext.GetCurrentExploitationIdAsync(cancellationToken);

    protected string? CurrentUserId =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
}

[Area("Ferme")]
public class ContextController(IExploitationContextService exploitationContext) : FermeControllerBase(exploitationContext)
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetExploitation(int exploitationId, string? returnUrl, CancellationToken cancellationToken)
    {
        await exploitationContext.EnsureAccessAsync(exploitationId, cancellationToken);
        await exploitationContext.SetCurrentExploitationIdAsync(exploitationId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Dashboard");
    }
}

[Area("Ferme")]
public class DashboardController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var exploitation = await db.Exploitations.AsNoTracking()
            .FirstAsync(e => e.Id == exploitationId, cancellationToken);

        var actifs = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive)
            .ToListAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var warrantyLimit = today.AddDays(30);

        var coutMois = await db.Interventions.AsNoTracking()
            .Where(i => i.ExploitationId == exploitationId
                        && i.CompletedDate != null
                        && i.CompletedDate >= monthStart)
            .SumAsync(i => i.LaborCost + i.PartsCost, cancellationToken);

        var upcoming = await db.Interventions.AsNoTracking()
            .Include(i => i.Actif)
            .Where(i => i.ExploitationId == exploitationId
                        && i.Statut != InterventionStatut.Cloturee
                        && i.Statut != InterventionStatut.Annulee
                        && i.PlannedDate >= today)
            .OrderBy(i => i.PlannedDate)
            .Take(6)
            .Select(i => new UpcomingMaintenanceItem(
                i.Id,
                i.Title,
                i.Actif != null ? i.Actif.Name : null,
                i.PlannedDate,
                i.Type.ToString()))
            .ToListAsync(cancellationToken);

        var critiques = actifs
            .Where(a => a.Statut is ActifStatut.EnMaintenance or ActifStatut.HorsService or ActifStatut.EnPanne)
            .OrderBy(a => a.Statut)
            .ThenBy(a => a.InternalCode)
            .Take(6)
            .Select(a => new CriticalActifItem(
                a.Id,
                a.InternalCode,
                a.Name,
                a.Statut.ToString(),
                a.Statut switch
                {
                    ActifStatut.EnPanne or ActifStatut.HorsService => "danger",
                    _ => "warning"
                }))
            .ToList();

        var stocksBas = await db.StockArticles.AsNoTracking()
            .Where(s => s.ExploitationId == exploitationId && s.IsActive && s.QuantityOnHand <= s.ReorderLevel)
            .OrderBy(s => s.QuantityOnHand)
            .Take(5)
            .Select(s => new StockAlertItem(s.Id, s.Sku, s.Name, s.QuantityOnHand, s.ReorderLevel))
            .ToListAsync(cancellationToken);

        var recent = await db.Interventions.AsNoTracking()
            .Include(i => i.Actif)
            .Where(i => i.ExploitationId == exploitationId && i.CompletedDate != null)
            .OrderByDescending(i => i.CompletedDate)
            .Take(5)
            .Select(i => new RecentActivityItem(
                i.Title,
                i.Actif != null ? $"{i.Actif.InternalCode} · {i.Actif.Name}" : "Sans actif",
                i.CompletedDate!.Value))
            .ToListAsync(cancellationToken);

        var byCat = actifs
            .GroupBy(a => a.Categorie)
            .Select(g => new CategorySlice(
                g.Key.GetDisplayName(),
                g.Count(),
                CategoryColor(g.Key)))
            .OrderByDescending(x => x.Count)
            .ToList();

        var alertes = new List<DashboardAlertItem>();
        foreach (var a in actifs.Where(x => x.Statut == ActifStatut.EnPanne).Take(5))
            alertes.Add(new DashboardAlertItem("danger", $"{a.InternalCode} — en panne", "Actifs", "Details", a.Id));
        foreach (var a in actifs.Where(IsServiceDue).Take(5))
            alertes.Add(new DashboardAlertItem("warning", $"{a.InternalCode} — entretien dû", "Actifs", "Details", a.Id));
        foreach (var a in actifs.Where(x => x.WarrantyEndDate is not null && x.WarrantyEndDate <= warrantyLimit && x.WarrantyEndDate >= today).Take(3))
            alertes.Add(new DashboardAlertItem("info", $"{a.InternalCode} — garantie expire bientôt", "Actifs", "Details", a.Id));
        foreach (var s in stocksBas)
            alertes.Add(new DashboardAlertItem("warning", $"Stock bas : {s.Name}", "Stocks", "Index", null));

        var ops = actifs.Count(a => a.Statut == ActifStatut.EnService);
        var totalFleet = actifs.Count(a => a.Statut is not (ActifStatut.Reforme or ActifStatut.Vendu or ActifStatut.Loue));
        var disponibilite = totalFleet == 0 ? 100m : Math.Round(100m * ops / totalFleet, 1);

        var accessible = await exploitationContext.GetAccessibleExploitationsAsync(cancellationToken);
        var model = new FermeDashboardViewModel
        {
            ExploitationName = exploitation.Name,
            UserDisplayName = User.Identity?.Name,
            CurrentExploitationId = exploitationId,
            Accessible = accessible.Select(e => new ExploitationOptionItem(e.Id, e.Name)).ToList(),
            Parcelles = await db.Parcelles.CountAsync(p => p.ExploitationId == exploitationId && p.IsActive, cancellationToken),
            ActifsTotal = actifs.Count,
            ActifsOperationnels = ops,
            ActifsMaintenance = actifs.Count(a => a.Statut == ActifStatut.EnMaintenance),
            ActifsEnPanne = actifs.Count(a => a.Statut == ActifStatut.EnPanne),
            ActifsHorsService = actifs.Count(a => a.Statut == ActifStatut.HorsService),
            ActifsReforme = actifs.Count(a => a.Statut is ActifStatut.Reforme or ActifStatut.Loue or ActifStatut.Vendu),
            ValeurParc = actifs.Sum(a => a.AcquisitionValue),
            StocksBas = stocksBas.Count,
            InterventionsOuvertes = await db.Interventions.CountAsync(
                i => i.ExploitationId == exploitationId
                     && i.Statut != InterventionStatut.Cloturee
                     && i.Statut != InterventionStatut.Annulee,
                cancellationToken),
            AlertesTotal = alertes.Count,
            CoutMaintenanceMois = coutMois,
            TauxDisponibilite = disponibilite,
            ActifsParCategorie = byCat,
            MaintenancesAVenir = upcoming,
            ActifsCritiques = critiques,
            AlertesStock = stocksBas,
            Alertes = alertes.Take(10).ToList(),
            ActiviteRecente = recent
        };

        return View(model);
    }

    private static bool IsServiceDue(ActifAgricole a)
    {
        var today = DateTime.UtcNow.Date;
        if (a.NextServiceDate is not null && a.NextServiceDate.Value.Date <= today)
            return true;
        if (a.EngineHours is not null && a.NextServiceHours is not null && a.EngineHours >= a.NextServiceHours)
            return true;
        return false;
    }

    private static string CategoryColor(ActifCategorie c) => c switch
    {
        ActifCategorie.Machinerie => "#1b4d3e",
        ActifCategorie.Vehicules => "#2d6a4f",
        ActifCategorie.Outillage => "#c4a35a",
        ActifCategorie.Irrigation => "#2a7d9b",
        ActifCategorie.Installations => "#5c7268",
        ActifCategorie.Energie => "#d97706",
        ActifCategorie.EquipementElevage => "#7c3aed",
        ActifCategorie.Production => "#3d6b5a",
        ActifCategorie.IoT => "#0ea5e9",
        _ => "#94a3b8"
    };
}

internal static class EnumDisplayExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .OfType<DisplayAttribute>()
            .FirstOrDefault();
        return display?.Name ?? value.ToString();
    }
}

[Area("Ferme")]
public class ExploitationsController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await exploitationContext.GetAccessibleExploitationsAsync(cancellationToken);
        var ids = list.Select(x => x.Id).ToList();
        var items = await db.Exploitations.AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    public IActionResult Create() => View(new Exploitation());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Exploitation model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        db.Exploitations.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        if (!string.IsNullOrEmpty(CurrentUserId))
        {
            db.ExploitationUsers.Add(new ExploitationUser
            {
                ExploitationId = model.Id,
                UserId = CurrentUserId,
                Role = ExploitationUserRole.Proprietaire
            });
            await db.SaveChangesAsync(cancellationToken);
        }
        await exploitationContext.SetCurrentExploitationIdAsync(model.Id, cancellationToken);
        TempData["Success"] = "Exploitation créée.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        await exploitationContext.EnsureAccessAsync(id, cancellationToken);
        var item = await db.Exploitations.FindAsync([id], cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Exploitation model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();
        await exploitationContext.EnsureAccessAsync(id, cancellationToken);
        if (!ModelState.IsValid) return View(model);
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Exploitation mise à jour.";
        return RedirectToAction(nameof(Index));
    }
}

[Area("Ferme")]
public class ParcellesController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var query = db.Parcelles.AsNoTracking()
            .Include(p => p.Assolements)
            .Where(p => p.ExploitationId == exploitationId);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Code.Contains(term)
                || (p.CurrentCulture != null && p.CurrentCulture.Contains(term)));
        }
        ViewBag.Q = q;
        return View(await query.OrderBy(p => p.Code).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Parcelles.AsNoTracking()
            .Include(p => p.Assolements)
            .FirstOrDefaultAsync(p => p.Id == id && p.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();

        var actifs = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ParcelleId == id && a.ExploitationId == exploitationId && a.IsActive)
            .OrderBy(a => a.InternalCode)
            .ToListAsync(cancellationToken);

        var actifIds = actifs.Select(a => a.Id).ToList();
        var interventions = await db.Interventions.AsNoTracking()
            .Include(i => i.Actif)
            .Where(i => i.ExploitationId == exploitationId
                        && i.ActifAgricoleId != null
                        && actifIds.Contains(i.ActifAgricoleId.Value))
            .OrderByDescending(i => i.PlannedDate)
            .Take(20)
            .ToListAsync(cancellationToken);

        return View(new ParcelleDetailsViewModel
        {
            Parcelle = item,
            Actifs = actifs,
            Interventions = interventions
        });
    }

    public IActionResult Create() => View(new Parcelle());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Parcelle model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid) return View(model);
        db.Parcelles.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Parcelle créée.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Parcelles.FirstOrDefaultAsync(p => p.Id == id && p.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Parcelle model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Parcelle mise à jour.";
        return RedirectToAction(nameof(Details), new { id });
    }
}

[Area("Ferme")]
public class ActifsController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(ActifCategorie? categorie, ActifStatut? statut, string? q, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var query = db.ActifsAgricoles.AsNoTracking()
            .Include(a => a.Parcelle)
            .Where(a => a.ExploitationId == exploitationId && a.IsActive);
        if (categorie is not null) query = query.Where(a => a.Categorie == categorie);
        if (statut is not null) query = query.Where(a => a.Statut == statut);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(a => a.Name.Contains(term) || a.InternalCode.Contains(term)
                || (a.Brand != null && a.Brand.Contains(term))
                || (a.Model != null && a.Model.Contains(term)));
        }
        ViewBag.Categorie = categorie;
        ViewBag.Statut = statut;
        ViewBag.Q = q;
        return View(await query.OrderBy(a => a.InternalCode).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.ActifsAgricoles.AsNoTracking()
            .Include(a => a.Parcelle)
            .Include(a => a.Fournisseur)
            .FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();

        var interventions = await db.Interventions.AsNoTracking()
            .Include(i => i.Pieces).ThenInclude(p => p.StockArticle)
            .Where(i => i.ActifAgricoleId == id && i.ExploitationId == exploitationId)
            .OrderByDescending(i => i.PlannedDate)
            .ToListAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        return View(new ActifDetailsViewModel
        {
            Actif = item,
            Interventions = interventions,
            CoutMaintenanceTotal = interventions.Sum(i => i.LaborCost + i.PartsCost),
            ServiceDueByHours = item.EngineHours is not null && item.NextServiceHours is not null
                                && item.EngineHours >= item.NextServiceHours,
            ServiceDueByDate = item.NextServiceDate is not null && item.NextServiceDate.Value.Date <= today,
            WarrantyExpiring = item.WarrantyEndDate is not null
                               && item.WarrantyEndDate.Value.Date <= today.AddDays(30)
                               && item.WarrantyEndDate.Value.Date >= today
        });
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);
        return View("Edit", new ActifAgricole());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActifAgricole actif, CancellationToken cancellationToken)
    {
        actif.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(actif.QrPayload))
            actif.QrPayload = actif.InternalCode;
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View("Edit", actif);
        }
        db.ActifsAgricoles.Add(actif);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Actif créé.";
        return RedirectToAction(nameof(Details), new { id = actif.Id });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.ActifsAgricoles.FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadLookupsAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActifAgricole actif, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != actif.Id || actif.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(actif);
        }
        db.Update(actif);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Actif mis à jour.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive)
            .OrderBy(a => a.InternalCode)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Code;Nom;Categorie;Statut;Marque;Modele;Heures;Valeur");
        foreach (var a in items)
            sb.AppendLine($"{a.InternalCode};{a.Name};{a.Categorie};{a.Statut};{a.Brand};{a.Model};{a.EngineHours};{a.AcquisitionValue}");

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "inventaire-actifs.csv");
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Parcelles = new SelectList(
            await db.Parcelles.AsNoTracking()
                .Where(p => p.ExploitationId == exploitationId && p.IsActive)
                .OrderBy(p => p.Code)
                .ToListAsync(cancellationToken),
            "Id", "Name");
        ViewBag.Fournisseurs = new SelectList(
            await db.Fournisseurs.AsNoTracking()
                .Where(f => f.ExploitationId == exploitationId && f.IsActive)
                .OrderBy(f => f.Name)
                .ToListAsync(cancellationToken),
            "Id", "Name");
    }
}

[Area("Ferme")]
public class StocksController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.StockArticles.AsNoTracking()
            .Where(s => s.ExploitationId == exploitationId)
            .OrderBy(s => s.Sku)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    public IActionResult Create() => View(new StockArticle());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockArticle model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid) return View(model);
        db.StockArticles.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Article créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.StockArticles.FirstOrDefaultAsync(s => s.Id == id && s.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StockArticle model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Article mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Adjust(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.StockArticles.FirstOrDefaultAsync(s => s.Id == id && s.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(int id, StockMouvementType type, decimal quantity, string? notes, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.StockArticles.FirstOrDefaultAsync(s => s.Id == id && s.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();

        var delta = type switch
        {
            StockMouvementType.Entree => Math.Abs(quantity),
            StockMouvementType.Sortie => -Math.Abs(quantity),
            _ => quantity
        };
        item.QuantityOnHand += delta;
        db.StockMouvements.Add(new StockMouvement
        {
            StockArticleId = item.Id,
            Type = type,
            Quantity = quantity,
            Notes = notes,
            CreatedByUserId = CurrentUserId
        });
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Stock ajusté.";
        return RedirectToAction(nameof(Index));
    }
}

[Area("Ferme")]
public class MaintenanceController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(string? filter, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        var weekEnd = today.AddDays(7);
        filter ??= "all";

        var query = db.Interventions.AsNoTracking()
            .Include(i => i.Actif)
            .Where(i => i.ExploitationId == exploitationId);

        query = filter switch
        {
            "today" => query.Where(i => i.PlannedDate.Date == today
                && i.Statut != InterventionStatut.Cloturee
                && i.Statut != InterventionStatut.Annulee),
            "week" => query.Where(i => i.PlannedDate.Date >= today && i.PlannedDate.Date <= weekEnd
                && i.Statut != InterventionStatut.Cloturee
                && i.Statut != InterventionStatut.Annulee),
            "late" => query.Where(i => i.PlannedDate.Date < today
                && i.Statut != InterventionStatut.Cloturee
                && i.Statut != InterventionStatut.Annulee),
            "done" => query.Where(i => i.Statut == InterventionStatut.Cloturee),
            _ => query
        };

        ViewBag.Filter = filter;
        return View(await query.OrderByDescending(i => i.PlannedDate).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Create(int? actifId, CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);
        var model = new InterventionFormViewModel
        {
            Intervention = new InterventionMaintenance
            {
                PlannedDate = DateTime.UtcNow.Date,
                ActifAgricoleId = actifId
            },
            PieceLines = [new InterventionPieceLine(), new InterventionPieceLine(), new InterventionPieceLine()]
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InterventionFormViewModel model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        model.Intervention.ExploitationId = exploitationId;
        if (!TryValidateModel(model.Intervention, nameof(model.Intervention)))
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }

        db.Interventions.Add(model.Intervention);
        await db.SaveChangesAsync(cancellationToken);
        await SyncPiecesAsync(model.Intervention.Id, model.PieceLines, exploitationId, deduct: false, cancellationToken);

        if (model.Intervention.ActifAgricoleId is int actifId)
        {
            var actif = await db.ActifsAgricoles.FirstOrDefaultAsync(a => a.Id == actifId && a.ExploitationId == exploitationId, cancellationToken);
            if (actif is not null)
            {
                if (model.Intervention.Type == InterventionType.Correctif)
                    actif.Statut = model.Intervention.Statut == InterventionStatut.Ouverte
                        ? ActifStatut.EnPanne
                        : ActifStatut.EnMaintenance;
                else if (model.Intervention.Type == InterventionType.Preventif)
                    actif.Statut = ActifStatut.EnMaintenance;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Intervention créée.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Interventions
            .Include(i => i.Pieces)
            .FirstOrDefaultAsync(i => i.Id == id && i.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadLookupsAsync(cancellationToken);
        var lines = item.Pieces.Select(p => new InterventionPieceLine
        {
            StockArticleId = p.StockArticleId,
            Quantity = p.Quantity
        }).ToList();
        while (lines.Count < 3) lines.Add(new InterventionPieceLine());
        return View(new InterventionFormViewModel { Intervention = item, PieceLines = lines });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InterventionFormViewModel model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Intervention.Id || model.Intervention.ExploitationId != exploitationId) return BadRequest();
        if (!TryValidateModel(model.Intervention, nameof(model.Intervention)))
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }

        db.Update(model.Intervention);
        await SyncPiecesAsync(id, model.PieceLines, exploitationId, deduct: false, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Intervention mise à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id, string? report, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Interventions
            .Include(i => i.Pieces)
            .FirstOrDefaultAsync(i => i.Id == id && i.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();

        item.Statut = InterventionStatut.Cloturee;
        item.CompletedDate = DateTime.UtcNow;
        item.Report = report;

        await DeductPiecesAsync(item, cancellationToken);

        if (item.ActifAgricoleId is int actifId)
        {
            var actif = await db.ActifsAgricoles.FirstOrDefaultAsync(a => a.Id == actifId && a.ExploitationId == exploitationId, cancellationToken);
            if (actif is not null)
            {
                actif.Statut = ActifStatut.EnService;
                if (item.Type == InterventionType.Preventif)
                {
                    if (actif.EngineHours is not null)
                        actif.NextServiceHours = actif.EngineHours + 250;
                    actif.NextServiceDate = DateTime.UtcNow.Date.AddMonths(6);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Intervention clôturée (pièces déduites si configurées).";
        return RedirectToAction(nameof(Index));
    }

    private async Task SyncPiecesAsync(
        int interventionId,
        IEnumerable<InterventionPieceLine>? lines,
        int exploitationId,
        bool deduct,
        CancellationToken cancellationToken)
    {
        var existing = await db.InterventionPieces
            .Where(p => p.InterventionMaintenanceId == interventionId)
            .ToListAsync(cancellationToken);
        db.InterventionPieces.RemoveRange(existing);

        if (lines is null) return;
        foreach (var line in lines.Where(l => l.StockArticleId is > 0 && l.Quantity > 0))
        {
            var articleOk = await db.StockArticles.AnyAsync(
                s => s.Id == line.StockArticleId && s.ExploitationId == exploitationId, cancellationToken);
            if (!articleOk) continue;
            db.InterventionPieces.Add(new InterventionPiece
            {
                InterventionMaintenanceId = interventionId,
                StockArticleId = line.StockArticleId!.Value,
                Quantity = line.Quantity,
                Deducted = deduct
            });
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task DeductPiecesAsync(InterventionMaintenance item, CancellationToken cancellationToken)
    {
        decimal partsCost = 0;
        foreach (var piece in item.Pieces.Where(p => !p.Deducted))
        {
            var article = await db.StockArticles.FirstOrDefaultAsync(s => s.Id == piece.StockArticleId, cancellationToken);
            if (article is null) continue;
            article.QuantityOnHand -= piece.Quantity;
            partsCost += piece.Quantity * article.UnitCost;
            piece.Deducted = true;
            db.StockMouvements.Add(new StockMouvement
            {
                StockArticleId = article.Id,
                Type = StockMouvementType.Sortie,
                Quantity = piece.Quantity,
                Notes = $"Intervention #{item.Id} — {item.Title}",
                CreatedByUserId = CurrentUserId,
                InterventionMaintenanceId = item.Id
            });
        }
        if (partsCost > 0 && item.PartsCost == 0)
            item.PartsCost = partsCost;
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Actifs = new SelectList(
            await db.ActifsAgricoles.AsNoTracking()
                .Where(a => a.ExploitationId == exploitationId && a.IsActive)
                .OrderBy(a => a.InternalCode)
                .Select(a => new { a.Id, Name = a.InternalCode + " — " + a.Name })
                .ToListAsync(cancellationToken),
            "Id", "Name");
        ViewBag.Pieces = new SelectList(
            await db.StockArticles.AsNoTracking()
                .Where(s => s.ExploitationId == exploitationId && s.IsActive
                            && (s.Categorie == StockCategorie.Pieces || s.Categorie == StockCategorie.Autre))
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, Name = s.Sku + " — " + s.Name + $" ({s.QuantityOnHand} {s.Unit})" })
                .ToListAsync(cancellationToken),
            "Id", "Name");
    }
}

[Area("Ferme")]
public class FournisseursController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        return View(await db.Fournisseurs.AsNoTracking()
            .Where(f => f.ExploitationId == exploitationId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken));
    }

    public IActionResult Create() => View(new Fournisseur());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Fournisseur model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid) return View(model);
        db.Fournisseurs.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Fournisseur créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Fournisseurs.FirstOrDefaultAsync(f => f.Id == id && f.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Fournisseur model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Fournisseur mis à jour.";
        return RedirectToAction(nameof(Index));
    }
}
