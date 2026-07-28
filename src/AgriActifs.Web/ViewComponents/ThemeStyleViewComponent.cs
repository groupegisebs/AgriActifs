using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.ViewComponents;

public class ThemeStyleViewComponent(
    IAppContextService appContextService,
    IThemeService themeService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var snapshot = await appContextService.GetSnapshotAsync();
        var css = themeService.BuildCssBlock(snapshot.ThemeCssVariables);
        return View((css, snapshot.BootstrapColorMode));
    }
}
