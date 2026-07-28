using AgriActifs.Web.Data;
using AgriActifs.Web.Models.Authorization;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Areas.Admin.Controllers;

public class SecuredEndpointsController(
    ISecuredEndpointService securedEndpointService,
    IMvcEndpointScanner endpointScanner,
    ApplicationDbContext dbContext) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var endpoints = await securedEndpointService.GetAllAsync(cancellationToken);
        return View(endpoints);
    }

    public async Task<IActionResult> Discover(CancellationToken cancellationToken)
    {
        var endpoints = await endpointScanner.DiscoverAllAsync(cancellationToken);
        return View(endpoints.Where(e => !e.IsMapped).ToList());
    }

    public async Task<IActionResult> Create(string? area, string? controller, string? action, string? httpMethod, CancellationToken cancellationToken)
    {
        await PopulatePermissionsAsync(cancellationToken);
        return View(new SecuredEndpointEditModel
        {
            Area = area,
            Controller = controller ?? string.Empty,
            Action = action ?? string.Empty,
            HttpMethod = httpMethod,
            IsActive = true
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SecuredEndpointEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulatePermissionsAsync(cancellationToken);
            return View(model);
        }

        try
        {
            await securedEndpointService.CreateAsync(model, cancellationToken);
            TempData["Success"] = "Endpoint sécurisé créé.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulatePermissionsAsync(cancellationToken);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await securedEndpointService.GetForEditAsync(id, cancellationToken);
        if (model is null)
            return NotFound();

        await PopulatePermissionsAsync(cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SecuredEndpointEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulatePermissionsAsync(cancellationToken);
            return View(model);
        }

        try
        {
            await securedEndpointService.SaveAsync(model, cancellationToken);
            TempData["Success"] = "Endpoint mis à jour.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulatePermissionsAsync(cancellationToken);
            return View(model);
        }
    }

    private async Task PopulatePermissionsAsync(CancellationToken cancellationToken)
    {
        var permissions = await dbContext.PermissionDefinitions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Code)
            .ToListAsync(cancellationToken);

        ViewBag.Permissions = new SelectList(permissions, nameof(PermissionDefinition.Id), nameof(PermissionDefinition.Code));
    }
}
