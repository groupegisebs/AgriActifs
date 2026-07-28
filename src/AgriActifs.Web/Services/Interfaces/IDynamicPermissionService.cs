namespace AgriActifs.Web.Services.Interfaces;

public interface IDynamicPermissionService
{
    Task<bool> HasPermissionAsync(System.Security.Claims.ClaimsPrincipal user, string permissionCode, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(System.Security.Claims.ClaimsPrincipal user, string resource, Models.Authorization.PermissionAction action, string? propertyName = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetUserPermissionCodesAsync(string userId, CancellationToken cancellationToken = default);
    void InvalidateCache();
}
