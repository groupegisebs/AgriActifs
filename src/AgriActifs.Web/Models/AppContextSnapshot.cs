namespace AgriActifs.Web.Models;

public class AppContextSnapshot
{
    public string AppName { get; init; } = "GISEBS Secure MVC Starter";
    public string? Tagline { get; init; }
    public string? LogoUrl { get; init; }
    public string DefaultCulture { get; init; } = "fr-FR";
    public int ActiveThemeId { get; init; } = 1;
    public string ActiveThemeCode { get; init; } = "default";
    public string ActiveThemeName { get; init; } = "GISEBS Default";
    public string ThemeCssVariables { get; init; } = ThemeDefaultsJson.Default;
    public string BootstrapColorMode { get; init; } = "light";
}

public static class ThemeDefaultsJson
{
    public const string Default = """{"--gise-primary":"#1e40af","--gise-primary-dark":"#1e3a8a","--gise-accent":"#0ea5e9","--gise-accent-soft":"#e0f2fe","--gise-success":"#059669","--gise-warning":"#d97706","--gise-danger":"#dc2626","--gise-sidebar":"#0f172a","--gise-sidebar-hover":"#1e293b","--gise-sidebar-active":"#2563eb","--gise-surface":"#ffffff","--gise-bg":"#f1f5f9","--gise-border":"#e2e8f0","--gise-text":"#0f172a","--gise-text-muted":"#64748b"}""";
}
