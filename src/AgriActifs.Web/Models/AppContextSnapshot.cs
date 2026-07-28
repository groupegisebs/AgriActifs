namespace AgriActifs.Web.Models;

public class AppContextSnapshot
{
    public string AppName { get; init; } = "AgriActifs";
    public string? Tagline { get; init; }
    public string? LogoUrl { get; init; }
    public string DefaultCulture { get; init; } = "fr-FR";
    public int ActiveThemeId { get; init; } = 1;
    public string ActiveThemeCode { get; init; } = "default";
    public string ActiveThemeName { get; init; } = "AgriActifs";
    public string ThemeCssVariables { get; init; } = ThemeDefaultsJson.Default;
    public string BootstrapColorMode { get; init; } = "light";
}

public static class ThemeDefaultsJson
{
    public const string Default = """{"--gise-primary":"#1b4d3e","--gise-primary-dark":"#12352b","--gise-accent":"#c4a35a","--gise-accent-soft":"#f3ead3","--gise-success":"#2f6f4e","--gise-warning":"#b8860b","--gise-danger":"#b42318","--gise-sidebar":"#12352b","--gise-sidebar-hover":"#1b4d3e","--gise-sidebar-active":"#2a6350","--gise-surface":"#ffffff","--gise-bg":"#eef3ef","--gise-border":"#d5e0d8","--gise-text":"#12352b","--gise-text-muted":"#5c7268"}""";
}
