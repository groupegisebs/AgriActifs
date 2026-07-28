using System.Security.Claims;
using AgriActifs.Web.Authorization;
using AgriActifs.Web.Constants;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace AgriActifs.Tests.Authorization;

public class EndpointAuthorizationFilterTests
{
    private static async Task<bool> IsAllowedAsync(
        ClaimsPrincipal user,
        string area,
        string controller,
        string action,
        string httpMethod,
        string? permissionCode = "Users.View")
    {
        var endpointService = new Mock<ISecuredEndpointService>();
        endpointService
            .Setup(s => s.GetRequiredPermissionCodeAsync(area, controller, action, httpMethod, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionCode);

        var permissionService = new Mock<IDynamicPermissionService>();
        permissionService
            .Setup(s => s.HasPermissionAsync(user, permissionCode!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var filter = new EndpointAuthorizationFilter(endpointService.Object, permissionService.Object);
        var context = CreateContext(user, area, controller, action, httpMethod);

        await filter.OnAuthorizationAsync(context);
        return context.Result is null;
    }

    private static AuthorizationFilterContext CreateContext(
        ClaimsPrincipal user,
        string area,
        string controller,
        string action,
        string httpMethod)
    {
        var httpContext = new DefaultHttpContext { User = user, Request = { Method = httpMethod } };
        var routeData = new RouteData();
        routeData.Values["area"] = area;
        routeData.Values["controller"] = controller;
        routeData.Values["action"] = action;

        var actionDescriptor = new ControllerActionDescriptor
        {
            ControllerName = controller,
            ActionName = action
        };

        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
        return new AuthorizationFilterContext(actionContext, []);
    }

    private static ClaimsPrincipal CreateUser(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Test", nameType: ClaimTypes.Name, roleType: ClaimTypes.Role));

    [Fact]
    public async Task SuperAdmin_IsAllowed_WithoutPermissionCheck()
    {
        var user = CreateUser(new Claim(ClaimTypes.Role, AppRoles.SuperAdmin));
        Assert.True(await IsAllowedAsync(user, "Admin", "Users", "Index", "GET", permissionCode: null));
    }

    [Fact]
    public async Task UserWithMappedPermission_IsAllowed()
    {
        var user = CreateUser(new Claim(ClaimTypes.Role, "Editor"));
        Assert.True(await IsAllowedAsync(user, "Admin", "Users", "Index", "GET"));
    }

    [Fact]
    public async Task UserWithoutPermission_IsDenied()
    {
        var user = CreateUser(new Claim(ClaimTypes.Role, "Viewer"));
        var endpointService = new Mock<ISecuredEndpointService>();
        endpointService
            .Setup(s => s.GetRequiredPermissionCodeAsync("Admin", "Users", "Index", "GET", It.IsAny<CancellationToken>()))
            .ReturnsAsync("Users.View");

        var permissionService = new Mock<IDynamicPermissionService>();
        permissionService
            .Setup(s => s.HasPermissionAsync(user, "Users.View", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var filter = new EndpointAuthorizationFilter(endpointService.Object, permissionService.Object);
        var context = CreateContext(user, "Admin", "Users", "Index", "GET");

        await filter.OnAuthorizationAsync(context);
        Assert.IsType<ForbidResult>(context.Result);
    }

    [Fact]
    public async Task UnmappedAdminEndpoint_IsDenied()
    {
        var user = CreateUser(new Claim(ClaimTypes.Role, "Editor"));
        var endpointService = new Mock<ISecuredEndpointService>();
        endpointService
            .Setup(s => s.GetRequiredPermissionCodeAsync("Admin", "Unknown", "Index", "GET", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var permissionService = new Mock<IDynamicPermissionService>();
        var filter = new EndpointAuthorizationFilter(endpointService.Object, permissionService.Object);
        var context = CreateContext(user, "Admin", "Unknown", "Index", "GET");

        await filter.OnAuthorizationAsync(context);
        Assert.IsType<ForbidResult>(context.Result);
    }
}
