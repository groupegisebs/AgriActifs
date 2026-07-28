using AgriActifs.Web.Data;
using AgriActifs.Web.Models.Ferme;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        ViewBag.ExploitationName = exploitation.Name;
        ViewBag.ActifsTotal = await db.ActifsAgricoles.CountAsync(a => a.ExploitationId == exploitationId && a.IsActive, cancellationToken);
        ViewBag.ActifsMaintenance = await db.ActifsAgricoles.CountAsync(a => a.ExploitationId == exploitationId && a.Statut == ActifStatut.EnMaintenance, cancellationToken);
        ViewBag.ValeurParc = await db.ActifsAgricoles.Where(a => a.ExploitationId == exploitationId && a.IsActive).SumAsync(a => a.AcquisitionValue, cancellationToken);
        ViewBag.StocksBas = await db.StockArticles.CountAsync(s => s.ExploitationId == exploitationId && s.IsActive && s.QuantityOnHand <= s.ReorderLevel, cancellationToken);
        ViewBag.InterventionsOuvertes = await db.Interventions.CountAsync(i => i.ExploitationId == exploitationId && i.Statut != InterventionStatut.Cloturee && i.Statut != InterventionStatut.Annulee, cancellationToken);
        ViewBag.Parcelles = await db.Parcelles.CountAsync(p => p.ExploitationId == exploitationId && p.IsActive, cancellationToken);
        ViewBag.Accessible = await exploitationContext.GetAccessibleExploitationsAsync(cancellationToken);
        ViewBag.CurrentId = exploitationId;
        return View();
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
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.Parcelles.AsNoTracking()
            .Include(p => p.Assolements)
            .Where(p => p.ExploitationId == exploitationId)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);
        return View(items);
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
        return RedirectToAction(nameof(Index));
    }
}

[Area("Ferme")]
public class ActifsController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(ActifCategorie? categorie, ActifStatut? statut, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var query = db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive);
        if (categorie is not null) query = query.Where(a => a.Categorie == categorie);
        if (statut is not null) query = query.Where(a => a.Statut == statut);
        ViewBag.Categorie = categorie;
        ViewBag.Statut = statut;
        return View(await query.OrderBy(a => a.InternalCode).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.ActifsAgricoles.AsNoTracking()
            .Include(a => a.Parcelle)
            .FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadParcellesAsync(cancellationToken);
        return View(new ActifAgricole());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActifAgricole model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadParcellesAsync(cancellationToken);
            return View(model);
        }
        db.ActifsAgricoles.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Actif créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.ActifsAgricoles.FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadParcellesAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActifAgricole model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadParcellesAsync(cancellationToken);
            return View(model);
        }
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Actif mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive)
            .OrderBy(a => a.InternalCode)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Code;Nom;Categorie;Statut;Marque;Modele;Valeur");
        foreach (var a in items)
            sb.AppendLine($"{a.InternalCode};{a.Name};{a.Categorie};{a.Statut};{a.Brand};{a.Model};{a.AcquisitionValue}");

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "inventaire-actifs.csv");
    }

    private async Task LoadParcellesAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Parcelles = new SelectList(
            await db.Parcelles.AsNoTracking()
                .Where(p => p.ExploitationId == exploitationId && p.IsActive)
                .OrderBy(p => p.Code)
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
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.Interventions.AsNoTracking()
            .Include(i => i.Actif)
            .Where(i => i.ExploitationId == exploitationId)
            .OrderByDescending(i => i.PlannedDate)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadActifsAsync(cancellationToken);
        return View(new InterventionMaintenance { PlannedDate = DateTime.UtcNow.Date });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InterventionMaintenance model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadActifsAsync(cancellationToken);
            return View(model);
        }
        db.Interventions.Add(model);
        if (model.ActifAgricoleId is int actifId && model.Type == InterventionType.Correctif)
        {
            var actif = await db.ActifsAgricoles.FirstOrDefaultAsync(a => a.Id == actifId && a.ExploitationId == model.ExploitationId, cancellationToken);
            if (actif is not null) actif.Statut = ActifStatut.EnMaintenance;
        }
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Intervention créée.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Interventions.FirstOrDefaultAsync(i => i.Id == id && i.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadActifsAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InterventionMaintenance model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadActifsAsync(cancellationToken);
            return View(model);
        }
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Intervention mise à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id, string? report, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Interventions.FirstOrDefaultAsync(i => i.Id == id && i.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        item.Statut = InterventionStatut.Cloturee;
        item.CompletedDate = DateTime.UtcNow;
        item.Report = report;
        if (item.ActifAgricoleId is int actifId)
        {
            var actif = await db.ActifsAgricoles.FirstOrDefaultAsync(a => a.Id == actifId && a.ExploitationId == exploitationId, cancellationToken);
            if (actif is not null) actif.Statut = ActifStatut.EnService;
        }
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Intervention clôturée.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadActifsAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Actifs = new SelectList(
            await db.ActifsAgricoles.AsNoTracking()
                .Where(a => a.ExploitationId == exploitationId && a.IsActive)
                .OrderBy(a => a.InternalCode)
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
