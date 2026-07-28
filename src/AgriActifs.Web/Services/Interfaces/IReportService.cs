namespace AgriActifs.Web.Services.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<Models.ReportDefinition>> GetAvailableReportsAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
