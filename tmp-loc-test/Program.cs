using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

foreach (var culture in new[] { "en-US", "fr-FR" })
{
    var path = $@"c:\Users\bedig\source\repos\GISEEnterpriseEcosystemSolution\AgriActifs\src\AgriActifs.Web\Localization\Defaults\{culture}.yaml";
    var yaml = File.ReadAllText(path);
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();
    var root = deserializer.Deserialize<Dictionary<string, object>>(yaml) ?? new();
    Console.WriteLine($"{culture} OK top={root.Count} Nav={(root.ContainsKey("Nav") || root.ContainsKey("nav"))}");
}
