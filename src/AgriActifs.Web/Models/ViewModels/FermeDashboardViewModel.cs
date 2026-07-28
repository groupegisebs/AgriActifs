using AgriActifs.Web.Models.Ferme;

namespace AgriActifs.Web.Models.ViewModels;

public class FermeDashboardViewModel
{
    public string ExploitationName { get; set; } = string.Empty;
    public int CurrentExploitationId { get; set; }
    public IReadOnlyList<ExploitationOptionItem> Accessible { get; set; } = [];

    public int Parcelles { get; set; }
    public int ActifsTotal { get; set; }
    public int ActifsOperationnels { get; set; }
    public int ActifsMaintenance { get; set; }
    public int ActifsHorsService { get; set; }
    public int ActifsReforme { get; set; }
    public decimal ValeurParc { get; set; }
    public int StocksBas { get; set; }
    public int InterventionsOuvertes { get; set; }
    public decimal CoutMaintenanceMois { get; set; }

    public IReadOnlyList<CategorySlice> ActifsParCategorie { get; set; } = [];
    public IReadOnlyList<UpcomingMaintenanceItem> MaintenancesAVenir { get; set; } = [];
    public IReadOnlyList<CriticalActifItem> ActifsCritiques { get; set; } = [];
    public IReadOnlyList<StockAlertItem> AlertesStock { get; set; } = [];
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
