using AgriActifs.Web.Data;
using AgriActifs.Web.Models;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Services;

public class AuditService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor) : IAuditService
{
    public async Task LogAsync(string action, string entityName, string? entityId = null, string? details = null, CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        dbContext.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            UserId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            UserName = user?.Identity?.Name,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString()
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetLoginHistoryAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .Where(x => x.Action == "Login" || x.Action == "Logout")
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
