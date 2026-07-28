using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class ReportsController(IReportService reportService) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var reports = await reportService.GetAvailableReportsAsync(cancellationToken);
        return View(reports);
    }
}
