using AgriActifs.Web.Localization;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.ViewComponents;

public class LanguageSelectorViewComponent(IYamlLocalizationProvider localizationProvider) : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var cultures = localizationProvider.GetAvailableCultures();
        var current = System.Globalization.CultureInfo.CurrentUICulture.Name;
        return View((cultures, current));
    }
}
