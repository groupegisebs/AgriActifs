namespace AgriActifs.Web.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string? entityId = null, string? details = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Models.AuditLog>> GetRecentAsync(int count = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Models.AuditLog>> GetLoginHistoryAsync(int count = 20, CancellationToken cancellationToken = default);
}
