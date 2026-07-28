using AgriActifs.Web.Constants;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgriActifs.Web.Authorization;

public class EndpointAuthorizationFilter(
    ISecuredEndpointService securedEndpointService,
    IDynamicPermissionService permissionService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
            return;

        if (HasAllowAnonymous(descriptor))
            return;

        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
            return;

        if (user.IsInRole(AppRoles.SuperAdmin))
            return;

        var area = context.RouteData.Values.TryGetValue("area", out var areaValue) ? areaValue as string : null;
        if (string.Equals(area, "Identity", StringComparison.OrdinalIgnoreCase))
            return;

        var controller = context.RouteData.Values["controller"]?.ToString() ?? descriptor.ControllerName;
        var action = context.RouteData.Values["action"]?.ToString() ?? descriptor.ActionName;
        var httpMethod = context.HttpContext.Request.Method.ToUpperInvariant();

        var permissionCode = await securedEndpointService.GetRequiredPermissionCodeAsync(
            area, controller, action, httpMethod, context.HttpContext.RequestAborted);

        if (permissionCode is not null)
        {
            if (!await permissionService.HasPermissionAsync(user, permissionCode, context.HttpContext.RequestAborted))
                context.Result = new ForbidResult();
            return;
        }

        if (string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase))
            context.Result = new ForbidResult();
    }

    private static bool HasAllowAnonymous(ControllerActionDescriptor descriptor)
    {
        if (descriptor.EndpointMetadata.Any(m => m is IAllowAnonymous))
            return true;

        if (descriptor.ControllerTypeInfo is { } controllerType
            && controllerType.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
            return true;

        if (descriptor.MethodInfo is { } methodInfo
            && methodInfo.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
            return true;

        return false;
    }
}
