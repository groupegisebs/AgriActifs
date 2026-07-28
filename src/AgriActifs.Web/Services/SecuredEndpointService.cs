using AgriActifs.Web.Data;
using AgriActifs.Web.Models.Authorization;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AgriActifs.Web.Services;

public class SecuredEndpointService(
    ApplicationDbContext dbContext,
    IMemoryCache cache) : ISecuredEndpointService
{
    private const string CacheKey = "secured-endpoints-map";

    public async Task<string?> GetRequiredPermissionCodeAsync(
        string? area,
        string controller,
        string action,
        string httpMethod,
        CancellationToken cancellationToken = default)
    {
        var map = await GetMapAsync(cancellationToken);
        var normalizedArea = NormalizeArea(area);
        var normalizedMethod = httpMethod.ToUpperInvariant();

        if (map.TryGetValue(BuildKey(normalizedArea, controller, action, normalizedMethod), out var code))
            return code;

        if (map.TryGetValue(BuildKey(normalizedArea, controller, action, null), out code))
            return code;

        return null;
    }

    public async Task<IReadOnlyList<SecuredEndpointListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SecuredEndpoints
            .AsNoTracking()
            .Include(e => e.Permission)
            .OrderBy(e => e.Area)
            .ThenBy(e => e.Controller)
            .ThenBy(e => e.Action)
            .ThenBy(e => e.HttpMethod)
            .Select(e => new SecuredEndpointListItem(
                e.Id,
                e.Area,
                e.Controller,
                e.Action,
                e.HttpMethod,
                e.Permission!.Code,
                e.Permission.DisplayName,
                e.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<SecuredEndpointEditModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.SecuredEndpoints.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
            return null;

        return new SecuredEndpointEditModel
        {
            Id = entity.Id,
            Area = entity.Area,
            Controller = entity.Controller,
            Action = entity.Action,
            HttpMethod = entity.HttpMethod,
            PermissionDefinitionId = entity.PermissionDefinitionId,
            IsActive = entity.IsActive
        };
    }

    public async Task SaveAsync(SecuredEndpointEditModel model, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.SecuredEndpoints.FirstOrDefaultAsync(e => e.Id == model.Id, cancellationToken)
            ?? throw new InvalidOperationException("Endpoint introuvable.");

        entity.Area = NormalizeArea(model.Area);
        entity.Controller = model.Controller;
        entity.Action = model.Action;
        entity.HttpMethod = string.IsNullOrWhiteSpace(model.HttpMethod) ? null : model.HttpMethod.ToUpperInvariant();
        entity.PermissionDefinitionId = model.PermissionDefinitionId;
        entity.IsActive = model.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
    }

    public async Task CreateAsync(SecuredEndpointEditModel model, CancellationToken cancellationToken = default)
    {
        dbContext.SecuredEndpoints.Add(new SecuredEndpoint
        {
            Area = NormalizeArea(model.Area),
            Controller = model.Controller,
            Action = model.Action,
            HttpMethod = string.IsNullOrWhiteSpace(model.HttpMethod) ? null : model.HttpMethod.ToUpperInvariant(),
            PermissionDefinitionId = model.PermissionDefinitionId,
            IsActive = model.IsActive
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
    }

    public void InvalidateCache() => cache.Remove(CacheKey);

    private async Task<Dictionary<string, string>> GetMapAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out Dictionary<string, string>? cached) && cached is not null)
            return cached;

        var endpoints = await dbContext.SecuredEndpoints
            .AsNoTracking()
            .Where(e => e.IsActive)
            .Include(e => e.Permission)
            .ToListAsync(cancellationToken);

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var endpoint in endpoints)
        {
            var key = BuildKey(NormalizeArea(endpoint.Area), endpoint.Controller, endpoint.Action, endpoint.HttpMethod);
            map[key] = endpoint.Permission!.Code;
        }

        cache.Set(CacheKey, map, TimeSpan.FromMinutes(5));
        return map;
    }

    internal static string BuildKey(string? area, string controller, string action, string? httpMethod) =>
        $"{area ?? ""}|{controller}|{action}|{httpMethod ?? ""}";

    internal static string? NormalizeArea(string? area) =>
        string.IsNullOrWhiteSpace(area) ? null : area;
}
