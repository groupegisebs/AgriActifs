using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class UserRolesController(IRoleService roleService) : AdminControllerBase
{
    public async Task<IActionResult> Edit(string userId, CancellationToken cancellationToken)
    {
        var model = await roleService.GetUserRolesAsync(userId, cancellationToken);
        if (model is null)
            return NotFound();

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string userId, IList<string> selectedRoles, CancellationToken cancellationToken)
    {
        var (success, error) = await roleService.UpdateUserRolesAsync(userId, selectedRoles, cancellationToken);
        if (!success)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Edit), new { userId });
        }

        TempData["Success"] = "Rôles mis à jour.";
        return RedirectToAction("Index", "Users");
    }
}
