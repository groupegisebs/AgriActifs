using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class SettingsController(ISystemSettingsService settingsService) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await settingsService.GetAsync(cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SystemSettingsViewModel model, CancellationToken cancellationToken)
    {
        if (model.LogoFile is { Length: > 0 })
        {
            try
            {
                model.LogoUrl = await settingsService.SaveLogoAsync(model.LogoFile, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.LogoFile), ex.Message);
            }
        }

        if (!ModelState.IsValid)
        {
            model.AvailableThemes = (await settingsService.GetAsync(cancellationToken)).AvailableThemes;
            model.AvailableCultures = (await settingsService.GetAsync(cancellationToken)).AvailableCultures;
            return View(model);
        }

        await settingsService.SaveAsync(model, cancellationToken);
        TempData["Success"] = "Paramètres enregistrés.";
        return RedirectToAction(nameof(Index));
    }
}
