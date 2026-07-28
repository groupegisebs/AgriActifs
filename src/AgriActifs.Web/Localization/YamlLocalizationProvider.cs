using System.Collections.Concurrent;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AgriActifs.Web.Localization;

public class YamlLocalizationProvider(
    IWebHostEnvironment environment,
    ILogger<YamlLocalizationProvider> logger) : IYamlLocalizationProvider
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();
    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public string GetDefaultsPath(string culture) =>
        Path.Combine(environment.ContentRootPath, "Localization", "Defaults", $"{culture}.yaml");

    public string GetOverridePath(string culture)
    {
        var dir = Path.Combine(environment.ContentRootPath, "App_Data", "Localization");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"{culture}.yaml");
    }

    public IReadOnlyDictionary<string, string> GetStrings(string culture)
    {
        culture = NormalizeCulture(culture);
        return _cache.GetOrAdd(culture, LoadCulture);
    }

    public IReadOnlyList<string> GetAvailableCultures()
    {
        var cultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var defaultsDir = Path.Combine(environment.ContentRootPath, "Localization", "Defaults");
        if (Directory.Exists(defaultsDir))
        {
            foreach (var file in Directory.GetFiles(defaultsDir, "*.yaml"))
                cultures.Add(Path.GetFileNameWithoutExtension(file));
        }

        var overrideDir = Path.Combine(environment.ContentRootPath, "App_Data", "Localization");
        if (Directory.Exists(overrideDir))
        {
            foreach (var file in Directory.GetFiles(overrideDir, "*.yaml"))
                cultures.Add(Path.GetFileNameWithoutExtension(file));
        }

        return cultures.OrderBy(c => c).ToList();
    }

    public IReadOnlyList<string> GetAllKeys(string? referenceCulture = null)
    {
        referenceCulture ??= "fr-FR";
        return GetStrings(referenceCulture).Keys.OrderBy(k => k).ToList();
    }

    public void Reload() => _cache.Clear();

    public async Task SaveOverrideAsync(string culture, string yamlContent, CancellationToken cancellationToken = default)
    {
        culture = NormalizeCulture(culture);
        ValidateYaml(yamlContent);
        var path = GetOverridePath(culture);
        await File.WriteAllTextAsync(path, yamlContent, Encoding.UTF8, cancellationToken);
        Reload();
    }

    public async Task<string> ReadMergedYamlAsync(string culture, CancellationToken cancellationToken = default)
    {
        culture = NormalizeCulture(culture);
        var merged = GetStrings(culture);
        var nested = Unflatten(merged);
        return _serializer.Serialize(nested);
    }

    public async Task ResetToDefaultsAsync(string culture, CancellationToken cancellationToken = default)
    {
        culture = NormalizeCulture(culture);
        var overridePath = GetOverridePath(culture);
        if (File.Exists(overridePath))
            File.Delete(overridePath);
        Reload();
        await Task.CompletedTask;
    }

    private IReadOnlyDictionary<string, string> LoadCulture(string culture)
    {
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var defaultPath = GetDefaultsPath(culture);
        if (File.Exists(defaultPath))
            MergeYamlFile(defaultPath, merged);

        var overridePath = GetOverridePath(culture);
        if (File.Exists(overridePath))
            MergeYamlFile(overridePath, merged);

        if (merged.Count == 0 && !culture.Equals("fr-FR", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Aucune traduction pour {Culture}, repli fr-FR.", culture);
            return LoadCulture("fr-FR");
        }

        return merged;
    }

    private void MergeYamlFile(string path, Dictionary<string, string> target)
    {
        try
        {
            var yaml = File.ReadAllText(path, Encoding.UTF8);
            var root = _deserializer.Deserialize<Dictionary<string, object>>(yaml) ?? [];
            Flatten(string.Empty, root, target);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lecture YAML {Path}", path);
        }
    }

    private static void Flatten(string prefix, Dictionary<string, object> node, Dictionary<string, string> target)
    {
        foreach (var (key, value) in node)
        {
            var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
            if (value is Dictionary<object, object> dictObj)
            {
                var dict = dictObj.ToDictionary(k => k.Key.ToString()!, v => v.Value);
                FlattenObject(fullKey, dict, target);
            }
            else if (value is Dictionary<string, object> dictStr)
                Flatten(fullKey, dictStr, target);
            else
                target[fullKey] = value?.ToString() ?? string.Empty;
        }
    }

    private static void FlattenObject(string prefix, Dictionary<string, object?> node, Dictionary<string, string> target)
    {
        foreach (var (key, value) in node)
        {
            var fullKey = $"{prefix}.{key}";
            if (value is Dictionary<object, object> dictObj)
            {
                var dict = dictObj.ToDictionary(k => k.Key.ToString()!, v => (object?)v.Value);
                FlattenObject(fullKey, dict, target);
            }
            else if (value is Dictionary<string, object> dictStr)
                Flatten(fullKey, dictStr, target);
            else
                target[fullKey] = value?.ToString() ?? string.Empty;
        }
    }

    private static Dictionary<string, object> Unflatten(IReadOnlyDictionary<string, string> flat)
    {
        var root = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in flat.OrderBy(k => k.Key))
        {
            var parts = key.Split('.');
            var current = root;
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (!current.TryGetValue(parts[i], out var next) || next is not Dictionary<string, object> dict)
                {
                    dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    current[parts[i]] = dict;
                }
                current = dict;
            }
            current[parts[^1]] = value;
        }
        return root;
    }

    private void ValidateYaml(string yamlContent)
    {
        var root = _deserializer.Deserialize<Dictionary<string, object>>(yamlContent)
            ?? throw new InvalidOperationException("YAML invalide.");
        _ = Unflatten(FlattenRoot(root));
    }

    private static Dictionary<string, string> FlattenRoot(Dictionary<string, object> root)
    {
        var target = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Flatten(string.Empty, root, target);
        return target;
    }

    private static string NormalizeCulture(string culture) =>
        string.IsNullOrWhiteSpace(culture) ? "fr-FR" : culture.Trim();
}
