using System.Text.Json;
using AgriActifs.Web.Data;
using AgriActifs.Web.Models;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AgriActifs.Web.Services;

public class AppContextService(
    ApplicationDbContext dbContext,
    IMemoryCache cache,
    IHttpContextAccessor httpContextAccessor) : IAppContextService
{
    private const string CacheKey = "AppContextSnapshot";

    public async Task<AppContextSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var cacheKey = userId is null ? CacheKey : $"{CacheKey}:{userId}";

        if (cache.TryGetValue(cacheKey, out AppContextSnapshot? cached) && cached is not null)
            return cached;

        var settings = await dbContext.SystemSettings.AsNoTracking().FirstAsync(cancellationToken);
        var theme = await dbContext.ThemeDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == settings.ActiveThemeId, cancellationToken)
            ?? await dbContext.ThemeDefinitions.AsNoTracking().FirstAsync(cancellationToken);

        var bootstrapMode = "light";
        if (userId is not null)
        {
            var profile = await dbContext.UserProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
            if (profile?.Theme is "dark")
                bootstrapMode = "dark";
        }

        var snapshot = new AppContextSnapshot
        {
            AppName = settings.AppName,
            Tagline = settings.Tagline,
            LogoUrl = settings.LogoUrl,
            DefaultCulture = settings.DefaultCulture,
            ActiveThemeId = theme.Id,
            ActiveThemeCode = theme.Code,
            ActiveThemeName = theme.Name,
            ThemeCssVariables = theme.CssVariables,
            BootstrapColorMode = bootstrapMode
        };

        cache.Set(cacheKey, snapshot, TimeSpan.FromMinutes(5));
        return snapshot;
    }

    public void InvalidateCache()
    {
        if (cache is MemoryCache mc)
            mc.Compact(1.0);
    }
}

public class ThemeService(
    ApplicationDbContext dbContext,
    IMemoryCache cache,
    IAppContextService appContextService) : IThemeService
{
    private const string ActiveThemeCacheKey = "ActiveThemeCss";

    public async Task<IReadOnlyList<ThemeDefinition>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.ThemeDefinitions.AsNoTracking().OrderBy(t => t.Name).ToListAsync(cancellationToken);

    public async Task<ThemeDefinition?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await dbContext.ThemeDefinitions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<ThemeDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await dbContext.ThemeDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == code, cancellationToken);

    public async Task<int> CreateAsync(ThemeDefinition theme, CancellationToken cancellationToken = default)
    {
        theme.CreatedAt = DateTime.UtcNow;
        theme.IsSystem = false;
        dbContext.ThemeDefinitions.Add(theme);
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        return theme.Id;
    }

    public async Task UpdateAsync(ThemeDefinition theme, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.ThemeDefinitions.FirstOrDefaultAsync(t => t.Id == theme.Id, cancellationToken)
            ?? throw new InvalidOperationException("Thème introuvable.");

        existing.Name = theme.Name;
        existing.Description = theme.Description;
        existing.CssVariables = theme.CssVariables;
        existing.IsActive = theme.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var theme = await dbContext.ThemeDefinitions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Thème introuvable.");

        if (theme.IsSystem)
            throw new InvalidOperationException("Les thèmes système ne peuvent pas être supprimés.");

        var settings = await dbContext.SystemSettings.FirstAsync(cancellationToken);
        if (settings.ActiveThemeId == id)
            throw new InvalidOperationException("Impossible de supprimer le thème actif.");

        dbContext.ThemeDefinitions.Remove(theme);
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
    }

    public async Task SetActiveThemeAsync(int themeId, CancellationToken cancellationToken = default)
    {
        _ = await dbContext.ThemeDefinitions.FirstOrDefaultAsync(t => t.Id == themeId, cancellationToken)
            ?? throw new InvalidOperationException("Thème introuvable.");

        var settings = await dbContext.SystemSettings.FirstAsync(cancellationToken);
        settings.ActiveThemeId = themeId;
        settings.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        InvalidateCache();
    }

    public string BuildCssBlock(string cssVariablesJson)
    {
        try
        {
            var vars = JsonSerializer.Deserialize<Dictionary<string, string>>(cssVariablesJson) ?? [];
            if (vars.Count == 0)
                return string.Empty;

            var lines = vars.Select(kv => $"  {kv.Key}: {kv.Value};");
            return $":root {{\n{string.Join("\n", lines)}\n}}";
        }
        catch
        {
            return string.Empty;
        }
    }

    public void InvalidateCache()
    {
        cache.Remove(ActiveThemeCacheKey);
        appContextService.InvalidateCache();
    }
}
