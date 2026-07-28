using AgriActifs.Web.Data;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Services;

public class MvcEndpointScanner(
    IActionDescriptorCollectionProvider actionDescriptorProvider,
    ApplicationDbContext dbContext) : IMvcEndpointScanner
{
    public async Task<IReadOnlyList<DiscoveredMvcEndpoint>> DiscoverAllAsync(CancellationToken cancellationToken = default)
    {
        var mappedKeys = await dbContext.SecuredEndpoints
            .AsNoTracking()
            .Select(e => new { e.Area, e.Controller, e.Action, e.HttpMethod })
            .ToListAsync(cancellationToken);

        var mappedSet = mappedKeys
            .Select(e => SecuredEndpointService.BuildKey(
                SecuredEndpointService.NormalizeArea(e.Area),
                e.Controller,
                e.Action,
                e.HttpMethod))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var descriptors = actionDescriptorProvider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Where(d => typeof(Controller).IsAssignableFrom(d.ControllerTypeInfo))
            .ToList();

        var results = new List<DiscoveredMvcEndpoint>();

        foreach (var descriptor in descriptors)
        {
            var area = descriptor.RouteValues.TryGetValue("area", out var areaValue) ? areaValue : null;
            var controllerName = descriptor.RouteValues.TryGetValue("controller", out var c) ? c! : descriptor.ControllerName;
            var actionName = descriptor.RouteValues.TryGetValue("action", out var a) ? a! : descriptor.ActionName;

            var methods = descriptor.ActionConstraints?
                .OfType<HttpMethodActionConstraint>()
                .SelectMany(constraint => constraint.HttpMethods)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? ["GET"];

            foreach (var method in methods)
            {
                var normalizedMethod = method.ToUpperInvariant();
                var normalizedArea = SecuredEndpointService.NormalizeArea(area);
                var isMapped = mappedSet.Contains(SecuredEndpointService.BuildKey(normalizedArea, controllerName, actionName, normalizedMethod))
                    || mappedSet.Contains(SecuredEndpointService.BuildKey(normalizedArea, controllerName, actionName, null));

                results.Add(new DiscoveredMvcEndpoint(area, controllerName, actionName, normalizedMethod, isMapped));
            }
        }

        return results
            .DistinctBy(e => SecuredEndpointService.BuildKey(
                SecuredEndpointService.NormalizeArea(e.Area),
                e.Controller,
                e.Action,
                e.HttpMethod))
            .OrderBy(e => e.Area)
            .ThenBy(e => e.Controller)
            .ThenBy(e => e.Action)
            .ToList();
    }
}
