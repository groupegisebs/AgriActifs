using AgriActifs.Web.Data;
using AgriActifs.Web.Models.Ferme;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Services;

public interface IFermeNotificationService
{
    Task SyncAlertsAsync(int exploitationId, CancellationToken cancellationToken = default);
}

public class FermeNotificationService(ApplicationDbContext db) : IFermeNotificationService
{
    public async Task SyncAlertsAsync(int exploitationId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var warrantyLimit = today.AddDays(30);
        var desired = new List<(string Key, string Title, string Message, NotificationSeverity Sev, string? Ctrl, string? Act, int? Id)>();

        var actifs = await db.ActifsAgricoles.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var a in actifs.Where(x => x.Statut == ActifStatut.EnPanne))
            desired.Add(($"panne:{a.Id}", "Actif en panne", $"{a.InternalCode} — {a.Name}", NotificationSeverity.Danger, "Actifs", "Details", a.Id));

        foreach (var a in actifs.Where(x =>
                     (x.NextServiceDate is not null && x.NextServiceDate.Value.Date <= today)
                     || (x.EngineHours is not null && x.NextServiceHours is not null && x.EngineHours >= x.NextServiceHours)))
            desired.Add(($"service:{a.Id}", "Entretien dû", $"{a.InternalCode} — service préventif requis", NotificationSeverity.Warning, "Actifs", "Details", a.Id));

        foreach (var a in actifs.Where(x => x.WarrantyEndDate is not null && x.WarrantyEndDate <= warrantyLimit && x.WarrantyEndDate >= today))
            desired.Add(($"warranty:{a.Id}", "Garantie expire bientôt", $"{a.InternalCode} — fin {a.WarrantyEndDate:yyyy-MM-dd}", NotificationSeverity.Info, "Actifs", "Details", a.Id));

        var stocks = await db.StockArticles.AsNoTracking()
            .Where(s => s.ExploitationId == exploitationId && s.IsActive && s.QuantityOnHand <= s.ReorderLevel)
            .ToListAsync(cancellationToken);
        foreach (var s in stocks)
            desired.Add(($"stock:{s.Id}", "Stock faible", $"{s.Name} ({s.QuantityOnHand} ≤ seuil {s.ReorderLevel})", NotificationSeverity.Warning, "Stocks", "Index", null));

        var late = await db.Interventions.AsNoTracking()
            .Where(i => i.ExploitationId == exploitationId
                        && i.PlannedDate < today
                        && i.Statut != InterventionStatut.Cloturee
                        && i.Statut != InterventionStatut.Annulee)
            .Take(20)
            .ToListAsync(cancellationToken);
        foreach (var i in late)
            desired.Add(($"late:{i.Id}", "Maintenance en retard", i.Title, NotificationSeverity.Danger, "Maintenance", "Edit", i.Id));

        var keys = desired.Select(d => d.Key).ToHashSet();
        var existing = await db.FermeNotifications
            .Where(n => n.ExploitationId == exploitationId && n.DedupeKey != null)
            .ToListAsync(cancellationToken);

        foreach (var n in existing.Where(e => e.DedupeKey is not null && !keys.Contains(e.DedupeKey) && !e.IsRead))
            n.IsRead = true;

        foreach (var d in desired)
        {
            if (existing.Any(e => e.DedupeKey == d.Key && !e.IsRead))
                continue;
            if (existing.Any(e => e.DedupeKey == d.Key && e.CreatedAt.Date == today))
                continue;

            db.FermeNotifications.Add(new FermeNotification
            {
                ExploitationId = exploitationId,
                Title = d.Title,
                Message = d.Message,
                Severity = d.Sev,
                DedupeKey = d.Key,
                LinkController = d.Ctrl,
                LinkAction = d.Act,
                LinkId = d.Id
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
