using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class RolesController(IRoleService roleService) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var roles = await roleService.GetAllAsync(cancellationToken);
        return View(roles);
    }

    public IActionResult Create() => View(new RoleEditViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, error, createdRoleId) = await roleService.CreateAsync(model, cancellationToken);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Erreur lors de la création.");
            return View(model);
        }

        TempData["Success"] = $"Rôle « {model.Name} » créé. Cochez maintenant ses habilitations.";
        return RedirectToAction("Index", "PermissionMatrix", new { area = "Admin" });
    }

    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var model = await roleService.GetForEditAsync(id, cancellationToken);
        if (model is null)
            return NotFound();

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, error) = await roleService.UpdateAsync(model, cancellationToken);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Erreur lors de la modification.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await roleService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
