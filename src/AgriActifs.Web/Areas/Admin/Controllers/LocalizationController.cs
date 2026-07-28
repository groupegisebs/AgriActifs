using AgriActifs.Web.Localization;
using AgriActifs.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class LocalizationController(IYamlLocalizationProvider localizationProvider) : AdminControllerBase
{
    public IActionResult Index()
    {
        var cultures = localizationProvider.GetAvailableCultures();
        return View(cultures);
    }

    public async Task<IActionResult> Edit(string culture, CancellationToken cancellationToken)
    {
        culture = string.IsNullOrWhiteSpace(culture) ? "fr-FR" : culture;
        var yaml = await localizationProvider.ReadMergedYamlAsync(culture, cancellationToken);
        var referenceKeys = localizationProvider.GetAllKeys("fr-FR");
        var currentKeys = localizationProvider.GetStrings(culture).Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = referenceKeys.Where(k => !currentKeys.Contains(k)).ToList();

        return View(new LocalizationEditViewModel
        {
            Culture = culture,
            YamlContent = yaml,
            MissingKeys = missing,
            AvailableCultures = localizationProvider.GetAvailableCultures()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LocalizationEditViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            await localizationProvider.SaveOverrideAsync(model.Culture, model.YamlContent, cancellationToken);
            TempData["Success"] = $"Traductions {model.Culture} enregistrées.";
            return RedirectToAction(nameof(Edit), new { culture = model.Culture });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.AvailableCultures = localizationProvider.GetAvailableCultures();
            model.MissingKeys = [];
            return View(model);
        }
    }

    public async Task<IActionResult> Download(string culture, CancellationToken cancellationToken)
    {
        culture = string.IsNullOrWhiteSpace(culture) ? "fr-FR" : culture;
        var yaml = await localizationProvider.ReadMergedYamlAsync(culture, cancellationToken);
        return File(System.Text.Encoding.UTF8.GetBytes(yaml), "application/x-yaml", $"{culture}.yaml");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(string culture, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            TempData["Error"] = "Fichier vide.";
            return RedirectToAction(nameof(Edit), new { culture });
        }

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync(cancellationToken);
        await localizationProvider.SaveOverrideAsync(culture, content, cancellationToken);
        TempData["Success"] = $"Fichier {culture}.yaml importé.";
        return RedirectToAction(nameof(Edit), new { culture });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset(string culture, CancellationToken cancellationToken)
    {
        await localizationProvider.ResetToDefaultsAsync(culture, cancellationToken);
        TempData["Success"] = $"Traductions {culture} réinitialisées.";
        return RedirectToAction(nameof(Edit), new { culture });
    }
}
