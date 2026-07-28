using System.Globalization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace AgriActifs.Web.Localization;

public class YamlStringLocalizer(IYamlLocalizationProvider provider, string culture) : IStringLocalizer
{
    public LocalizedString this[string name]
    {
        get
        {
            var strings = provider.GetStrings(culture);
            return strings.TryGetValue(name, out var value)
                ? new LocalizedString(name, value, resourceNotFound: false)
                : new LocalizedString(name, $"[{name}]", resourceNotFound: true);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var localized = this[name];
            var formatted = localized.ResourceNotFound
                ? localized.Value
                : string.Format(CultureInfo.CurrentCulture, localized.Value, arguments);
            return new LocalizedString(name, formatted, localized.ResourceNotFound);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        foreach (var (key, value) in provider.GetStrings(culture))
            yield return new LocalizedString(key, value, false);
    }
}

public class YamlStringLocalizerFactory(IYamlLocalizationProvider provider) : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource) =>
        Create(CultureInfo.CurrentUICulture.Name, resourceSource.FullName ?? resourceSource.Name);

    public IStringLocalizer Create(string baseName, string location) =>
        new YamlStringLocalizer(provider, CultureInfo.CurrentUICulture.Name);
}

public class YamlViewLocalizer(IYamlLocalizationProvider provider) : IHtmlLocalizer
{
    private IStringLocalizer Inner => new YamlStringLocalizer(provider, CultureInfo.CurrentUICulture.Name);

    public LocalizedHtmlString this[string name] => new(Inner[name].Name, Inner[name].Value, Inner[name].ResourceNotFound);

    public LocalizedHtmlString this[string name, params object[] arguments]
    {
        get
        {
            var localized = Inner[name, arguments];
            return new LocalizedHtmlString(localized.Name, localized.Value, localized.ResourceNotFound);
        }
    }

    public LocalizedString GetString(string name) => Inner[name];

    public LocalizedString GetString(string name, params object[] arguments) => Inner[name, arguments];

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        Inner.GetAllStrings(includeParentCultures);
}

public class YamlViewLocalizerFactory(IYamlLocalizationProvider provider) : IHtmlLocalizerFactory
{
    public IHtmlLocalizer Create(Type resourceSource) => new YamlViewLocalizer(provider);
    public IHtmlLocalizer Create(string baseName, string location) => new YamlViewLocalizer(provider);
}
