using AgriActifs.Web.Constants;
using AgriActifs.Web.Data;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Services;

public class ReportService(
    ApplicationDbContext dbContext,
    IDynamicPermissionService permissionService,
    IHttpContextAccessor httpContextAccessor) : IReportService
{
    public async Task<IReadOnlyList<Models.ReportDefinition>> GetAvailableReportsAsync(CancellationToken cancellationToken = default)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return [];

        var reports = await dbContext.ReportDefinitions
            .Where(x => x.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (user.IsInRole(AppRoles.SuperAdmin))
            return reports;

        var available = new List<Models.ReportDefinition>();
        foreach (var report in reports)
        {
            if (await permissionService.HasPermissionAsync(user, report.RequiredPermissionCode, cancellationToken))
                available.Add(report);
        }

        return available;
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        dbContext.ReportDefinitions.CountAsync(x => x.IsActive, cancellationToken);
}
