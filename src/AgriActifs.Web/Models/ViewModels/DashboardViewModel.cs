namespace AgriActifs.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int LockedUsers { get; set; }
    public int TotalRoles { get; set; }
    public int AvailableReports { get; set; }
    public IReadOnlyList<AuditLogSummary> RecentLogins { get; set; } = [];
    public IReadOnlyList<AuditLogSummary> RecentActions { get; set; } = [];
}

public class AuditLogSummary
{
    public string Action { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
