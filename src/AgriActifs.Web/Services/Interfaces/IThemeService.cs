using AgriActifs.Web.Models;

namespace AgriActifs.Web.Services.Interfaces;

public interface IThemeService
{
    Task<IReadOnlyList<ThemeDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ThemeDefinition?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ThemeDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ThemeDefinition theme, CancellationToken cancellationToken = default);
    Task UpdateAsync(ThemeDefinition theme, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task SetActiveThemeAsync(int themeId, CancellationToken cancellationToken = default);
    string BuildCssBlock(string cssVariablesJson);
    void InvalidateCache();
}
