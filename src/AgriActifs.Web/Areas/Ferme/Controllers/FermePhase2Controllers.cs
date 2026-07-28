using AgriActifs.Web.Data;
using AgriActifs.Web.Models.Ferme;
using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Areas.Ferme.Controllers;

[Area("Ferme")]
public class ActivitesController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(string? view, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.ActivitesAgricoles.AsNoTracking()
            .Include(a => a.Parcelle)
            .Include(a => a.Actif)
            .Where(a => a.ExploitationId == exploitationId)
            .OrderBy(a => a.PlannedDate)
            .ToListAsync(cancellationToken);
        ViewBag.ViewMode = view ?? "list";
        ViewBag.CalendarMonth = DateTime.UtcNow.Date;
        return View(items);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);
        return View(new ActiviteAgricole { PlannedDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActiviteAgricole model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }
        db.ActivitesAgricoles.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Activité créée.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.ActivitesAgricoles.FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadLookupsAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActiviteAgricole model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Activité mise à jour.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Parcelles = new SelectList(
            await db.Parcelles.AsNoTracking().Where(p => p.ExploitationId == exploitationId && p.IsActive).OrderBy(p => p.Code).ToListAsync(cancellationToken),
            "Id", "Name");
        ViewBag.Actifs = new SelectList(
            await db.ActifsAgricoles.AsNoTracking().Where(a => a.ExploitationId == exploitationId && a.IsActive).OrderBy(a => a.InternalCode)
                .Select(a => new { a.Id, Name = a.InternalCode + " — " + a.Name }).ToListAsync(cancellationToken),
            "Id", "Name");
    }
}

[Area("Ferme")]
public class DocumentsController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(DocumentCategorie? categorie, string? q, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var query = db.DocumentsFerme.AsNoTracking()
            .Include(d => d.Actif)
            .Include(d => d.Parcelle)
            .Where(d => d.ExploitationId == exploitationId);
        if (categorie is not null) query = query.Where(d => d.Categorie == categorie);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(d => d.Title.Contains(term) || (d.Tags != null && d.Tags.Contains(term)));
        }
        ViewBag.Categorie = categorie;
        ViewBag.Q = q;
        return View(await query.OrderByDescending(d => d.DocumentDate).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Create(int? actifId, CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);
        return View(new DocumentFerme
        {
            DocumentDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
            ActifAgricoleId = actifId
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentFerme model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        model.UploadedByUserId = CurrentUserId;
        model.UploadedAt = DateTime.UtcNow;
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }
        db.DocumentsFerme.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Document enregistré.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.DocumentsFerme.FirstOrDefaultAsync(d => d.Id == id && d.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadLookupsAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DocumentFerme model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Document mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Actifs = new SelectList(
            await db.ActifsAgricoles.AsNoTracking().Where(a => a.ExploitationId == exploitationId && a.IsActive).OrderBy(a => a.InternalCode)
                .Select(a => new { a.Id, Name = a.InternalCode + " — " + a.Name }).ToListAsync(cancellationToken),
            "Id", "Name");
        ViewBag.Parcelles = new SelectList(
            await db.Parcelles.AsNoTracking().Where(p => p.ExploitationId == exploitationId && p.IsActive).OrderBy(p => p.Code).ToListAsync(cancellationToken),
            "Id", "Name");
        ViewBag.Fournisseurs = new SelectList(
            await db.Fournisseurs.AsNoTracking().Where(f => f.ExploitationId == exploitationId && f.IsActive).OrderBy(f => f.Name).ToListAsync(cancellationToken),
            "Id", "Name");
    }
}

[Area("Ferme")]
public class NotificationsController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db,
    IFermeNotificationService notifications) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        await notifications.SyncAlertsAsync(exploitationId, cancellationToken);
        var items = await db.FermeNotifications.AsNoTracking()
            .Where(n => n.ExploitationId == exploitationId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);
        ViewBag.Unread = items.Count(n => !n.IsRead);
        return View(items);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.FermeNotifications.FirstOrDefaultAsync(n => n.Id == id && n.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        item.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.FermeNotifications.Where(n => n.ExploitationId == exploitationId && !n.IsRead).ToListAsync(cancellationToken);
        foreach (var n in items) n.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Toutes les notifications marquées lues.";
        return RedirectToAction(nameof(Index));
    }
}

[Area("Ferme")]
public class IrrigationController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var secteurs = await db.IrrigationSecteurs.AsNoTracking()
            .Include(s => s.Parcelle)
            .Include(s => s.Pompe)
            .Where(s => s.ExploitationId == exploitationId)
            .OrderBy(s => s.Code)
            .ToListAsync(cancellationToken);
        var pompes = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive && a.Categorie == ActifCategorie.Irrigation)
            .OrderBy(a => a.InternalCode)
            .ToListAsync(cancellationToken);
        return View(new IrrigationIndexViewModel { Secteurs = secteurs, Pompes = pompes });
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);
        return View(new IrrigationSecteur());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IrrigationSecteur model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }
        db.IrrigationSecteurs.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Secteur d'irrigation créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.IrrigationSecteurs.FirstOrDefaultAsync(s => s.Id == id && s.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadLookupsAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, IrrigationSecteur model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Secteur mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Parcelles = new SelectList(
            await db.Parcelles.AsNoTracking().Where(p => p.ExploitationId == exploitationId && p.IsActive).OrderBy(p => p.Code).ToListAsync(cancellationToken),
            "Id", "Name");
        ViewBag.Pompes = new SelectList(
            await db.ActifsAgricoles.AsNoTracking()
                .Where(a => a.ExploitationId == exploitationId && a.IsActive && a.Categorie == ActifCategorie.Irrigation)
                .OrderBy(a => a.InternalCode)
                .Select(a => new { a.Id, Name = a.InternalCode + " — " + a.Name })
                .ToListAsync(cancellationToken),
            "Id", "Name");
    }
}

[Area("Ferme")]
public class EnergieController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var assets = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive && a.Categorie == ActifCategorie.Energie)
            .OrderBy(a => a.InternalCode)
            .ToListAsync(cancellationToken);
        var releves = await db.EnergieReleves.AsNoTracking()
            .Include(r => r.Actif)
            .Where(r => r.ExploitationId == exploitationId)
            .OrderByDescending(r => r.ReadingDate)
            .Take(50)
            .ToListAsync(cancellationToken);
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return View(new EnergieIndexViewModel
        {
            Actifs = assets,
            Releves = releves,
            KwhMois = releves.Where(r => r.ReadingDate >= monthStart).Sum(r => r.Kwh),
            CoutMois = releves.Where(r => r.ReadingDate >= monthStart).Sum(r => r.Cost)
        });
    }

    public async Task<IActionResult> CreateReleve(CancellationToken cancellationToken)
    {
        await LoadActifsAsync(cancellationToken);
        return View(new EnergieReleve { ReadingDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReleve(EnergieReleve model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadActifsAsync(cancellationToken);
            return View(model);
        }
        db.EnergieReleves.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Relevé énergétique enregistré.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadActifsAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Actifs = new SelectList(
            await db.ActifsAgricoles.AsNoTracking()
                .Where(a => a.ExploitationId == exploitationId && a.IsActive && a.Categorie == ActifCategorie.Energie)
                .OrderBy(a => a.InternalCode)
                .Select(a => new { a.Id, Name = a.InternalCode + " — " + a.Name })
                .ToListAsync(cancellationToken),
            "Id", "Name");
    }
}

[Area("Ferme")]
public class RapportsController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var actifs = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive)
            .ToListAsync(cancellationToken);
        var interventions = await db.Interventions.AsNoTracking()
            .Where(i => i.ExploitationId == exploitationId)
            .ToListAsync(cancellationToken);

        var yearStart = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var maintYear = interventions
            .Where(i => i.CompletedDate >= yearStart || (i.CompletedDate is null && i.PlannedDate >= yearStart))
            .Sum(i => i.LaborCost + i.PartsCost);

        var tcoRows = actifs
            .OrderByDescending(a => a.AcquisitionValue)
            .Take(20)
            .Select(a =>
            {
                var maint = interventions.Where(i => i.ActifAgricoleId == a.Id).Sum(i => i.LaborCost + i.PartsCost);
                var years = a.UsefulLifeYears is > 0
                    ? a.UsefulLifeYears.Value
                    : Math.Max(1, (int)((DateTime.UtcNow - (a.AcquisitionDate ?? a.CreatedAt)).TotalDays / 365) + 1);
                var tco = a.AcquisitionValue + maint - a.ResidualValue;
                return new TcoRow(
                    a.Id,
                    a.InternalCode,
                    a.Name,
                    a.AcquisitionValue,
                    maint,
                    a.ResidualValue,
                    tco,
                    Math.Round(tco / years, 0));
            })
            .ToList();

        return View(new RapportsFermeViewModel
        {
            ValeurParc = actifs.Sum(a => a.AcquisitionValue),
            CoutMaintenanceAnnee = maintYear,
            Pannes = actifs.Count(a => a.Statut == ActifStatut.EnPanne),
            InterventionsCloturees = interventions.Count(i => i.Statut == InterventionStatut.Cloturee),
            TcoRows = tcoRows
        });
    }

    public async Task<IActionResult> ExportTco(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var actifs = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive)
            .OrderBy(a => a.InternalCode)
            .ToListAsync(cancellationToken);
        var interventions = await db.Interventions.AsNoTracking()
            .Where(i => i.ExploitationId == exploitationId)
            .ToListAsync(cancellationToken);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Code;Nom;Acquisition;Maintenance;Residuel;TCO");
        foreach (var a in actifs)
        {
            var maint = interventions.Where(i => i.ActifAgricoleId == a.Id).Sum(i => i.LaborCost + i.PartsCost);
            var tco = a.AcquisitionValue + maint - a.ResidualValue;
            sb.AppendLine($"{a.InternalCode};{a.Name};{a.AcquisitionValue};{maint};{a.ResidualValue};{tco}");
        }
        return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "rapport-tco.csv");
    }
}
