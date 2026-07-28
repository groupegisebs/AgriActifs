using AgriActifs.Web.Data;
using AgriActifs.Web.Models.Ferme;
using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Areas.Ferme.Controllers;

[Area("Ferme")]
public class CarteController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var exploitation = await db.Exploitations.AsNoTracking()
            .FirstAsync(e => e.Id == exploitationId, cancellationToken);

        var actifs = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive && a.GpsLat != null && a.GpsLng != null)
            .ToListAsync(cancellationToken);

        var parcelles = await db.Parcelles.AsNoTracking()
            .Where(p => p.ExploitationId == exploitationId && p.IsActive && p.GpsLat != null && p.GpsLng != null)
            .ToListAsync(cancellationToken);

        var capteurs = await db.CapteursIoT.AsNoTracking()
            .Where(c => c.ExploitationId == exploitationId && c.IsActive && c.GpsLat != null && c.GpsLng != null)
            .ToListAsync(cancellationToken);

        var secteurs = await db.IrrigationSecteurs.AsNoTracking()
            .Include(s => s.Pompe)
            .Where(s => s.ExploitationId == exploitationId && s.IsActive)
            .ToListAsync(cancellationToken);

        var centerLat = exploitation.MapCenterLat
            ?? actifs.Select(a => a.GpsLat!.Value).DefaultIfEmpty(45.6308).Average();
        var centerLng = exploitation.MapCenterLng
            ?? actifs.Select(a => a.GpsLng!.Value).DefaultIfEmpty(-72.9569).Average();

        return View(new CarteFermeViewModel
        {
            ExploitationName = exploitation.Name,
            CenterLat = centerLat,
            CenterLng = centerLng,
            Actifs = actifs.Select(a => new MapMarker(
                a.Id, "actif", a.Name,
                $"{a.InternalCode} · {a.Statut} · {(a.EngineHours is not null ? $"{a.EngineHours:N0} h" : "")}",
                a.GpsLat!.Value, a.GpsLng!.Value,
                a.Statut switch
                {
                    ActifStatut.EnPanne => "danger",
                    ActifStatut.EnMaintenance => "warning",
                    _ => "ok"
                },
                Url.Action("Details", "Actifs", new { area = "Ferme", id = a.Id }))).ToList(),
            Parcelles = parcelles.Select(p => new MapMarker(
                p.Id, "parcelle", p.Name,
                $"{p.AreaHa} ha · {p.CurrentCulture ?? "—"} · {p.Etat}",
                p.GpsLat!.Value, p.GpsLng!.Value, "parcelle",
                Url.Action("Details", "Parcelles", new { area = "Ferme", id = p.Id }))).ToList(),
            Capteurs = capteurs.Select(c => new MapMarker(
                c.Id, "capteur", c.Name,
                $"{c.Type} · {c.LastValue?.ToString("0.##") ?? "—"} {c.Unit}",
                c.GpsLat!.Value, c.GpsLng!.Value,
                c.Statut == CapteurStatut.Alerte ? "danger" : "iot",
                Url.Action("Details", "Capteurs", new { area = "Ferme", id = c.Id }))).ToList(),
            Irrigation = secteurs
                .Where(s => s.Pompe?.GpsLat != null)
                .Select(s => new MapMarker(
                    s.Id, "irrigation", s.Name,
                    $"Pompe {s.Pompe!.Name} · {s.DebitM3H} m³/h",
                    s.Pompe.GpsLat!.Value, s.Pompe.GpsLng!.Value, "water",
                    Url.Action("Index", "Irrigation", new { area = "Ferme" }))).ToList()
        });
    }
}

[Area("Ferme")]
public class CapteursController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.CapteursIoT.AsNoTracking()
            .Include(c => c.Parcelle)
            .Include(c => c.Actif)
            .Where(c => c.ExploitationId == exploitationId)
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.CapteursIoT.AsNoTracking()
            .Include(c => c.Parcelle)
            .Include(c => c.Actif)
            .FirstOrDefaultAsync(c => c.Id == id && c.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        var lectures = await db.CapteurLectures.AsNoTracking()
            .Where(l => l.CapteurIoTId == id)
            .OrderByDescending(l => l.RecordedAt)
            .Take(50)
            .ToListAsync(cancellationToken);
        return View(new CapteurDetailsViewModel { Capteur = item, Lectures = lectures });
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);
        return View(new CapteurIoT());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CapteurIoT model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }
        db.CapteursIoT.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Capteur créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.CapteursIoT.FirstOrDefaultAsync(c => c.Id == id && c.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadLookupsAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CapteurIoT model, CancellationToken cancellationToken)
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
        TempData["Success"] = "Capteur mis à jour.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLecture(int id, decimal value, string? notes, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var capteur = await db.CapteursIoT.FirstOrDefaultAsync(c => c.Id == id && c.ExploitationId == exploitationId, cancellationToken);
        if (capteur is null) return NotFound();

        db.CapteurLectures.Add(new CapteurLecture
        {
            CapteurIoTId = id,
            Value = value,
            Notes = notes,
            RecordedAt = DateTime.UtcNow
        });
        capteur.LastValue = value;
        capteur.LastReadingAt = DateTime.UtcNow;
        if ((capteur.AlertMin is not null && value < capteur.AlertMin)
            || (capteur.AlertMax is not null && value > capteur.AlertMax))
            capteur.Statut = CapteurStatut.Alerte;
        else
            capteur.Statut = CapteurStatut.EnLigne;

        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Lecture enregistrée.";
        return RedirectToAction(nameof(Details), new { id });
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
public class ReadinessController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        var horizon = today.AddDays(21);

        var activites = await db.ActivitesAgricoles.AsNoTracking()
            .Include(a => a.Parcelle)
            .Include(a => a.Actif)
            .Where(a => a.ExploitationId == exploitationId
                        && a.Statut != ActiviteStatut.Annulee
                        && a.Statut != ActiviteStatut.Terminee
                        && a.PlannedDate >= today
                        && a.PlannedDate <= horizon)
            .OrderBy(a => a.PlannedDate)
            .ToListAsync(cancellationToken);

        var actifs = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive)
            .ToListAsync(cancellationToken);

        var items = activites.Select(a =>
        {
            var (ready, label, reason) = Evaluate(a, actifs);
            return new ReadinessItem(
                a.Id, a.Title, a.Type.ToString(), a.PlannedDate,
                a.Parcelle?.Name, a.Actif?.Name, ready, label, reason);
        }).ToList();

        return View(new ReadinessViewModel
        {
            Items = items,
            ReadyCount = items.Count(i => i.Ready),
            BlockedCount = items.Count(i => !i.Ready)
        });
    }

    private static (bool Ready, string Label, string Reason) Evaluate(ActiviteAgricole a, List<ActifAgricole> actifs)
    {
        if (a.Type is not (ActiviteType.Semis or ActiviteType.Recolte or ActiviteType.TravailSol or ActiviteType.Traitement))
            return (true, "OK", "Pas de contrainte équipement majeure");

        ActifAgricole? machine = null;
        if (a.ActifAgricoleId is int id)
            machine = actifs.FirstOrDefault(x => x.Id == id);
        else
            machine = actifs.FirstOrDefault(x =>
                x.Categorie is ActifCategorie.Machinerie or ActifCategorie.Outillage
                && x.Statut == ActifStatut.EnService);

        if (machine is null)
            return (false, "Bloqué", "Aucun équipement opérationnel disponible");

        if (machine.Statut is ActifStatut.EnPanne or ActifStatut.HorsService)
            return (false, "Bloqué", $"{machine.InternalCode} indisponible ({machine.Statut})");

        if (machine.Statut == ActifStatut.EnMaintenance)
            return (false, "Risque", $"{machine.InternalCode} en maintenance");

        if (machine.NextServiceDate is not null && machine.NextServiceDate.Value.Date <= a.PlannedDate)
            return (false, "Risque", $"{machine.InternalCode} — entretien dû avant l'activité");

        if (machine.EngineHours is not null && machine.NextServiceHours is not null
            && machine.EngineHours >= machine.NextServiceHours)
            return (false, "Risque", $"{machine.InternalCode} — seuil heures atteint");

        return (true, "Prêt", $"{machine.InternalCode} opérationnel");
    }
}

[Area("Ferme")]
public class TerrainController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : FermeControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var exploitation = await db.Exploitations.AsNoTracking().FirstAsync(e => e.Id == exploitationId, cancellationToken);
        var today = DateTime.UtcNow.Date;

        var model = new TerrainDashboardViewModel
        {
            ExploitationName = exploitation.Name,
            Alertes = await db.FermeNotifications.CountAsync(n => n.ExploitationId == exploitationId && !n.IsRead, cancellationToken),
            InterventionsOuvertes = await db.Interventions.CountAsync(i =>
                i.ExploitationId == exploitationId
                && i.Statut != InterventionStatut.Cloturee
                && i.Statut != InterventionStatut.Annulee, cancellationToken),
            StocksBas = await db.StockArticles.CountAsync(s =>
                s.ExploitationId == exploitationId && s.IsActive && s.QuantityOnHand <= s.ReorderLevel, cancellationToken),
            EnPanne = await db.ActifsAgricoles.CountAsync(a =>
                a.ExploitationId == exploitationId && a.IsActive && a.Statut == ActifStatut.EnPanne, cancellationToken),
            Aujourdhui = await db.Interventions.AsNoTracking()
                .Include(i => i.Actif)
                .Where(i => i.ExploitationId == exploitationId
                            && i.PlannedDate.Date == today
                            && i.Statut != InterventionStatut.Cloturee
                            && i.Statut != InterventionStatut.Annulee)
                .OrderBy(i => i.Priorite)
                .Take(8)
                .Select(i => new UpcomingMaintenanceItem(i.Id, i.Title, i.Actif != null ? i.Actif.Name : null, i.PlannedDate, i.Type.ToString()))
                .ToListAsync(cancellationToken)
        };
        return View(model);
    }

    public IActionResult Scan() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Scan(string code, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        code = (code ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(code))
        {
            ModelState.AddModelError(string.Empty, "Saisissez ou scannez un code QR.");
            return View();
        }

        var actif = await db.ActifsAgricoles.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ExploitationId == exploitationId && a.IsActive
                && (a.QrPayload == code || a.InternalCode == code), cancellationToken);
        if (actif is null)
        {
            ModelState.AddModelError(string.Empty, $"Aucun actif pour « {code} ».");
            return View();
        }
        return RedirectToAction(nameof(Actif), new { id = actif.Id });
    }

    public async Task<IActionResult> Actif(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var actif = await db.ActifsAgricoles.AsNoTracking()
            .Include(a => a.Parcelle)
            .FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        return actif is null ? NotFound() : View(actif);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeclarerPanne(int id, string? description, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var actif = await db.ActifsAgricoles.FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        if (actif is null) return NotFound();

        actif.Statut = ActifStatut.EnPanne;
        var intervention = new InterventionMaintenance
        {
            ExploitationId = exploitationId,
            ActifAgricoleId = id,
            Title = $"Panne terrain — {actif.InternalCode}",
            Type = InterventionType.Correctif,
            Statut = InterventionStatut.Ouverte,
            Priorite = InterventionPriorite.Urgente,
            PlannedDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
            Description = description ?? "Déclarée depuis l'app terrain",
            AssignedToName = User.Identity?.Name
        };
        db.Interventions.Add(intervention);
        db.FermeNotifications.Add(new FermeNotification
        {
            ExploitationId = exploitationId,
            Title = "Panne déclarée",
            Message = $"{actif.InternalCode} — {actif.Name}",
            Severity = NotificationSeverity.Danger,
            LinkController = "Actifs",
            LinkAction = "Details",
            LinkId = id,
            DedupeKey = $"panne-decl:{id}:{DateTime.UtcNow:yyyyMMddHHmm}"
        });
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Panne déclarée.";
        return RedirectToAction(nameof(Actif), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AjouterPhoto(int id, string photoUrl, string? title, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var actif = await db.ActifsAgricoles.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        if (actif is null) return NotFound();
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            TempData["Success"] = "URL photo requise.";
            return RedirectToAction(nameof(Actif), new { id });
        }

        db.DocumentsFerme.Add(new DocumentFerme
        {
            ExploitationId = exploitationId,
            Title = string.IsNullOrWhiteSpace(title) ? $"Photo terrain {actif.InternalCode}" : title,
            Categorie = DocumentCategorie.Photo,
            FileUrl = photoUrl.Trim(),
            ActifAgricoleId = id,
            DocumentDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = CurrentUserId,
            Tags = "terrain,photo"
        });
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Photo ajoutée.";
        return RedirectToAction(nameof(Actif), new { id });
    }

    public async Task<IActionResult> Stocks(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.StockArticles.AsNoTracking()
            .Where(s => s.ExploitationId == exploitationId && s.IsActive)
            .OrderBy(s => s.QuantityOnHand <= s.ReorderLevel ? 0 : 1)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Maintenance(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.Interventions.AsNoTracking()
            .Include(i => i.Actif)
            .Where(i => i.ExploitationId == exploitationId
                        && i.Statut != InterventionStatut.Cloturee
                        && i.Statut != InterventionStatut.Annulee)
            .OrderBy(i => i.PlannedDate)
            .Take(30)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderMaintenance(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Interventions
            .Include(i => i.Pieces)
            .FirstOrDefaultAsync(i => i.Id == id && i.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();

        item.Statut = InterventionStatut.Cloturee;
        item.CompletedDate = DateTime.UtcNow;
        item.ValidatedAt = DateTime.UtcNow;
        item.ValidatedByUserId = CurrentUserId;

        foreach (var piece in item.Pieces.Where(p => !p.Deducted))
        {
            var article = await db.StockArticles.FirstOrDefaultAsync(s => s.Id == piece.StockArticleId, cancellationToken);
            if (article is null) continue;
            article.QuantityOnHand -= piece.Quantity;
            piece.Deducted = true;
            db.StockMouvements.Add(new StockMouvement
            {
                StockArticleId = article.Id,
                Type = StockMouvementType.Sortie,
                Quantity = piece.Quantity,
                Notes = $"Terrain validation #{item.Id}",
                CreatedByUserId = CurrentUserId,
                InterventionMaintenanceId = item.Id
            });
        }

        if (item.ActifAgricoleId is int actifId)
        {
            var actif = await db.ActifsAgricoles.FirstOrDefaultAsync(a => a.Id == actifId, cancellationToken);
            if (actif is not null) actif.Statut = ActifStatut.EnService;
        }

        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Maintenance validée.";
        return RedirectToAction(nameof(Maintenance));
    }

    public async Task<IActionResult> Parcelles(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.Parcelles.AsNoTracking()
            .Where(p => p.ExploitationId == exploitationId && p.IsActive)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);
        return View(items);
    }
}
