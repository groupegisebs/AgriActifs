using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class PermissionMatrixController(IPermissionAdminService permissionAdminService) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await permissionAdminService.GetHabilitationMatrixAsync(cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMatrix(SaveHabilitationMatrixViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            await permissionAdminService.SaveHabilitationMatrixAsync(model.Grants, cancellationToken);
            TempData["Success"] = "Matrice des habilitations enregistrée.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> EditRole(string roleId, CancellationToken cancellationToken)
    {
        var matrix = await permissionAdminService.GetMatrixAsync(cancellationToken);
        var role = matrix.Roles.FirstOrDefault(r => r.RoleId == roleId);
        if (role is null)
            return NotFound();

        ViewBag.RoleName = role.RoleName;
        ViewBag.RoleId = roleId;
        ViewBag.Categories = matrix.Categories;
        ViewBag.GrantedIds = matrix.Permissions
            .Where(p => p.GrantsByRoleId.GetValueOrDefault(roleId))
            .Select(p => p.PermissionId)
            .ToHashSet();

        return View(matrix.Permissions);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(SaveRolePermissionsViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            await permissionAdminService.SaveRoleGrantsAsync(model.RoleId, model.GrantedPermissionIds, cancellationToken);
            TempData["Success"] = "Habilitations du rôle enregistrées.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(EditRole), new { roleId = model.RoleId });
        }
    }

    public async Task<IActionResult> Model(string resource, CancellationToken cancellationToken)
    {
        var model = await permissionAdminService.GetModelPermissionsAsync(resource, cancellationToken);
        if (model.EntityActions.Count == 0 && model.Properties.Count == 0)
            return NotFound();

        return View(model);
    }
}
