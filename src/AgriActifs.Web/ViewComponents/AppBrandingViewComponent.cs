using AgriActifs.Web.Models;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.ViewComponents;

public class AppBrandingViewComponent(IAppContextService appContextService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var snapshot = await appContextService.GetSnapshotAsync();
        return View(snapshot);
    }
}
