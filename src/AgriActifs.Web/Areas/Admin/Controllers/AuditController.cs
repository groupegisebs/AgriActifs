using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class AuditController(IAuditService auditService) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var logs = await auditService.GetRecentAsync(100, cancellationToken);
        return View(logs);
    }
}
