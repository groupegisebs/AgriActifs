namespace AgriActifs.Web.Localization;

public interface IYamlLocalizationProvider
{
    IReadOnlyDictionary<string, string> GetStrings(string culture);
    IReadOnlyList<string> GetAvailableCultures();
    IReadOnlyList<string> GetAllKeys(string? referenceCulture = null);
    void Reload();
    Task SaveOverrideAsync(string culture, string yamlContent, CancellationToken cancellationToken = default);
    Task<string> ReadMergedYamlAsync(string culture, CancellationToken cancellationToken = default);
    Task ResetToDefaultsAsync(string culture, CancellationToken cancellationToken = default);
    string GetDefaultsPath(string culture);
    string GetOverridePath(string culture);
}
