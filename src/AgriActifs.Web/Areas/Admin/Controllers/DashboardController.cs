using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class DashboardController(
    IUserService userService,
    IRoleService roleService,
    IReportService reportService,
    IAuditService auditService) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var recentLogins = await auditService.GetLoginHistoryAsync(10, cancellationToken);
        var recentActions = await auditService.GetRecentAsync(10, cancellationToken);

        var model = new DashboardViewModel
        {
            TotalUsers = await userService.CountAsync(cancellationToken),
            ActiveUsers = await userService.CountAsync(cancellationToken),
            LockedUsers = await userService.CountLockedAsync(cancellationToken),
            TotalRoles = await roleService.CountAsync(cancellationToken),
            AvailableReports = await reportService.CountAsync(cancellationToken),
            RecentLogins = recentLogins.Select(x => new AuditLogSummary
            {
                Action = x.Action,
                UserName = x.UserName,
                CreatedAt = x.CreatedAt
            }).ToList(),
            RecentActions = recentActions.Select(x => new AuditLogSummary
            {
                Action = x.Action,
                UserName = x.UserName,
                CreatedAt = x.CreatedAt
            }).ToList()
        };

        return View(model);
    }
}
