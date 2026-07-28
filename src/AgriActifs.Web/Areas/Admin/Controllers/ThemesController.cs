using AgriActifs.Web.Models;
using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class ThemesController(IThemeService themeService, ISystemSettingsService settingsService) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var themes = await themeService.GetAllAsync(cancellationToken);
        var settings = await settingsService.GetAsync(cancellationToken);
        ViewBag.ActiveThemeId = settings.ActiveThemeId;
        return View(themes);
    }

    public IActionResult Create() => View(new ThemeEditViewModel { Code = $"custom-{DateTime.UtcNow:yyyyMMdd}" });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ThemeEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var entity = ThemeMapper.ToEntity(model);
        entity.Code = model.Code.Trim().ToLowerInvariant().Replace(' ', '-');
        await themeService.CreateAsync(entity, cancellationToken);
        TempData["Success"] = "Thème créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var theme = await themeService.GetByIdAsync(id, cancellationToken);
        if (theme is null)
            return NotFound();
        return View(ThemeMapper.ToViewModel(theme));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ThemeEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        await themeService.UpdateAsync(ThemeMapper.ToEntity(model), cancellationToken);
        TempData["Success"] = "Thème enregistré.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        await themeService.SetActiveThemeAsync(id, cancellationToken);
        TempData["Success"] = "Thème activé pour l'application.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await themeService.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Thème supprimé.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duplicate(int id, CancellationToken cancellationToken)
    {
        var theme = await themeService.GetByIdAsync(id, cancellationToken);
        if (theme is null)
            return NotFound();
        var copy = ThemeMapper.ToViewModel(theme);
        copy.Id = 0;
        copy.Code = $"{theme.Code}-copy";
        copy.Name = $"{theme.Name} (copie)";
        copy.IsSystem = false;
        return View("Create", copy);
    }

    public async Task<IActionResult> DownloadYaml(int id, CancellationToken cancellationToken)
    {
        var theme = await themeService.GetByIdAsync(id, cancellationToken);
        if (theme is null)
            return NotFound();
        return File(System.Text.Encoding.UTF8.GetBytes(theme.CssVariables), "application/json", $"{theme.Code}.theme.json");
    }
}
