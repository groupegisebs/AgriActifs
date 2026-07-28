using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class UsersController(IUserService userService) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return View(users);
    }

    public IActionResult Create() => View(new UserEditViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, error) = await userService.CreateAsync(model, cancellationToken);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Erreur lors de la création.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var model = await userService.GetForEditAsync(id, cancellationToken);
        if (model is null)
            return NotFound();

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, error) = await userService.UpdateAsync(model, cancellationToken);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Erreur lors de la modification.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
    {
        await userService.DeactivateAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id, CancellationToken cancellationToken)
    {
        await userService.UnlockAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
