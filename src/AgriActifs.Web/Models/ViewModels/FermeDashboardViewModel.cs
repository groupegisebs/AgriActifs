using AgriActifs.Web.Models.Ferme;

namespace AgriActifs.Web.Models.ViewModels;

public class FermeDashboardViewModel
{
    public string ExploitationName { get; set; } = string.Empty;
    public string? UserDisplayName { get; set; }
    public int CurrentExploitationId { get; set; }
    public IReadOnlyList<ExploitationOptionItem> Accessible { get; set; } = [];

    public int Parcelles { get; set; }
    public int ActifsTotal { get; set; }
    public int ActifsOperationnels { get; set; }
    public int ActifsMaintenance { get; set; }
    public int ActifsEnPanne { get; set; }
    public int ActifsHorsService { get; set; }
    public int ActifsReforme { get; set; }
    public decimal ValeurParc { get; set; }
    public int StocksBas { get; set; }
    public int InterventionsOuvertes { get; set; }
    public int AlertesTotal { get; set; }
    public decimal CoutMaintenanceMois { get; set; }
    public decimal TauxDisponibilite { get; set; }

    public IReadOnlyList<CategorySlice> ActifsParCategorie { get; set; } = [];
    public IReadOnlyList<UpcomingMaintenanceItem> MaintenancesAVenir { get; set; } = [];
    public IReadOnlyList<CriticalActifItem> ActifsCritiques { get; set; } = [];
    public IReadOnlyList<StockAlertItem> AlertesStock { get; set; } = [];
    public IReadOnlyList<DashboardAlertItem> Alertes { get; set; } = [];
    public IReadOnlyList<RecentActivityItem> ActiviteRecente { get; set; } = [];
}

public record ExploitationOptionItem(int Id, string Name);

public record CategorySlice(string Label, int Count, string Color);

public record UpcomingMaintenanceItem(
    int Id,
    string Title,
    string? ActifName,
    DateTime PlannedDate,
    string Type);

public record CriticalActifItem(
    int Id,
    string Code,
    string Name,
    string Statut,
    string Tone);

public record StockAlertItem(int Id, string Sku, string Name, decimal Quantity, decimal ReorderLevel);

public record RecentActivityItem(string Title, string Detail, DateTime At);

public record DashboardAlertItem(string Tone, string Message, string? LinkController, string? LinkAction, int? LinkId);

public class ActifDetailsViewModel
{
    public ActifAgricole Actif { get; set; } = null!;
    public IReadOnlyList<InterventionMaintenance> Interventions { get; set; } = [];
    public IReadOnlyList<DocumentFerme> Documents { get; set; } = [];
    public decimal CoutMaintenanceTotal { get; set; }
    public bool ServiceDueByHours { get; set; }
    public bool ServiceDueByDate { get; set; }
    public bool WarrantyExpiring { get; set; }
}

public class InterventionFormViewModel
{
    public InterventionMaintenance Intervention { get; set; } = new();
    public List<InterventionPieceLine> PieceLines { get; set; } = [];
}

public class InterventionPieceLine
{
    public int? StockArticleId { get; set; }
    public decimal Quantity { get; set; }
}

public class ParcelleDetailsViewModel
{
    public Parcelle Parcelle { get; set; } = null!;
    public IReadOnlyList<ActifAgricole> Actifs { get; set; } = [];
    public IReadOnlyList<InterventionMaintenance> Interventions { get; set; } = [];
}

public class IrrigationIndexViewModel
{
    public IReadOnlyList<IrrigationSecteur> Secteurs { get; set; } = [];
    public IReadOnlyList<ActifAgricole> Pompes { get; set; } = [];
}

public class EnergieIndexViewModel
{
    public IReadOnlyList<ActifAgricole> Actifs { get; set; } = [];
    public IReadOnlyList<EnergieReleve> Releves { get; set; } = [];
    public decimal KwhMois { get; set; }
    public decimal CoutMois { get; set; }
}

public class RapportsFermeViewModel
{
    public decimal ValeurParc { get; set; }
    public decimal CoutMaintenanceAnnee { get; set; }
    public int Pannes { get; set; }
    public int InterventionsCloturees { get; set; }
    public IReadOnlyList<TcoRow> TcoRows { get; set; } = [];
}

public record TcoRow(
    int Id,
    string Code,
    string Name,
    decimal Acquisition,
    decimal Maintenance,
    decimal Residual,
    decimal Tco,
    decimal Annualized);

public class CarteFermeViewModel
{
    public string ExploitationName { get; set; } = string.Empty;
    public double CenterLat { get; set; } = 45.6308;
    public double CenterLng { get; set; } = -72.9569;
    public IReadOnlyList<MapMarker> Actifs { get; set; } = [];
    public IReadOnlyList<MapMarker> Parcelles { get; set; } = [];
    public IReadOnlyList<MapMarker> Capteurs { get; set; } = [];
    public IReadOnlyList<MapMarker> Irrigation { get; set; } = [];
}

public record MapMarker(
    int Id,
    string Kind,
    string Label,
    string Detail,
    double Lat,
    double Lng,
    string Tone,
    string? Link);

public class ReadinessViewModel
{
    public IReadOnlyList<ReadinessItem> Items { get; set; } = [];
    public int ReadyCount { get; set; }
    public int BlockedCount { get; set; }
}

public record ReadinessItem(
    int ActiviteId,
    string Title,
    string Type,
    DateTime PlannedDate,
    string? Parcelle,
    string? Actif,
    bool Ready,
    string StatusLabel,
    string Reason);

public class TerrainDashboardViewModel
{
    public string ExploitationName { get; set; } = string.Empty;
    public int Alertes { get; set; }
    public int InterventionsOuvertes { get; set; }
    public int StocksBas { get; set; }
    public int EnPanne { get; set; }
    public IReadOnlyList<UpcomingMaintenanceItem> Aujourdhui { get; set; } = [];
}

public class CapteurDetailsViewModel
{
    public CapteurIoT Capteur { get; set; } = null!;
    public IReadOnlyList<CapteurLecture> Lectures { get; set; } = [];
}
