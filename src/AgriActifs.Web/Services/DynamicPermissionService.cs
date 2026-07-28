using AgriActifs.Web.Constants;
using AgriActifs.Web.Data;
using AgriActifs.Web.Models.Authorization;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace AgriActifs.Web.Services;

public class DynamicPermissionService(
    ApplicationDbContext dbContext,
    UserManager<Models.Identity.ApplicationUser> userManager,
    IMemoryCache cache) : IDynamicPermissionService
{
    private const string CacheKeyPrefix = "user-permissions:";

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionCode, CancellationToken cancellationToken = default)
    {
        if (user.IsInRole(AppRoles.SuperAdmin))
            return true;

        if (user.Identity?.IsAuthenticated != true)
            return false;

        var userId = userManager.GetUserId(user);
        if (userId is null)
            return false;

        var permissions = await GetUserPermissionCodesAsync(userId, cancellationToken);
        return permissions.Contains(permissionCode);
    }

    public async Task<bool> HasPermissionAsync(
        ClaimsPrincipal user,
        string resource,
        PermissionAction action,
        string? propertyName = null,
        CancellationToken cancellationToken = default)
    {
        var code = propertyName is null
            ? $"{resource}.{action}"
            : $"{resource}.{propertyName}.{action}";

        return await HasPermissionAsync(user, code, cancellationToken);
    }

    private const string CacheVersionKey = "permission-cache-version";

    public async Task<IReadOnlyList<string>> GetUserPermissionCodesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var version = cache.GetOrCreate(CacheVersionKey, _ => Guid.NewGuid().ToString())!;
        var cacheKey = $"{CacheKeyPrefix}{userId}:{version}";
        if (cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached is not null)
            return cached;

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return [];

        var roleIds = await userManager.GetRolesAsync(user);
        if (roleIds.Count == 0)
            return [];

        var roleIdValues = await dbContext.Roles
            .Where(r => roleIds.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var codes = await dbContext.RolePermissionGrants
            .AsNoTracking()
            .Where(g => g.IsGranted && roleIdValues.Contains(g.RoleId))
            .Join(dbContext.PermissionDefinitions.AsNoTracking().Where(p => p.IsActive),
                g => g.PermissionDefinitionId,
                p => p.Id,
                (_, p) => p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        cache.Set(cacheKey, codes, TimeSpan.FromMinutes(5));
        return codes;
    }

    public void InvalidateCache() => cache.Set(CacheVersionKey, Guid.NewGuid().ToString());
}
