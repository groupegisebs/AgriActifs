namespace AgriActifs.Web.Data;

public static class ThemeDefaults
{
    // Single-line JSON: EOL-independent (avoids LF/CRLF drift between Windows and Linux CI).
    public const string DefaultCssVariables =
        """{"--gise-primary": "#1b4d3e","--gise-primary-dark": "#12352b","--gise-accent": "#c4a35a","--gise-accent-soft": "#f3ead3","--gise-success": "#2f6f4e","--gise-warning": "#b8860b","--gise-danger": "#b42318","--gise-sidebar": "#12352b","--gise-sidebar-hover": "#1b4d3e","--gise-sidebar-active": "#2a6350","--gise-surface": "#ffffff","--gise-bg": "#eef3ef","--gise-border": "#d5e0d8","--gise-text": "#12352b","--gise-text-muted": "#5c7268"}""";

    public const string CorporateCssVariables =
        """{"--gise-primary": "#374151","--gise-primary-dark": "#1f2937","--gise-accent": "#6b7280","--gise-accent-soft": "#f3f4f6","--gise-success": "#059669","--gise-warning": "#d97706","--gise-danger": "#dc2626","--gise-sidebar": "#111827","--gise-sidebar-hover": "#1f2937","--gise-sidebar-active": "#4b5563","--gise-surface": "#ffffff","--gise-bg": "#f9fafb","--gise-border": "#e5e7eb","--gise-text": "#111827","--gise-text-muted": "#6b7280"}""";

    public const string OceanCssVariables =
        """{"--gise-primary": "#0d9488","--gise-primary-dark": "#0f766e","--gise-accent": "#06b6d4","--gise-accent-soft": "#cffafe","--gise-success": "#059669","--gise-warning": "#d97706","--gise-danger": "#dc2626","--gise-sidebar": "#134e4a","--gise-sidebar-hover": "#115e59","--gise-sidebar-active": "#0d9488","--gise-surface": "#ffffff","--gise-bg": "#f0fdfa","--gise-border": "#ccfbf1","--gise-text": "#134e4a","--gise-text-muted": "#5eead4"}""";

    public static IReadOnlyList<(int Id, string Code, string Name, string Description, string CssVariables)> SeedThemes =>
    [
        (1, "default", "AgriActifs", "Verts champs et or moisson", DefaultCssVariables),
        (2, "corporate", "Corporate", "Tons neutres professionnels", CorporateCssVariables),
        (3, "ocean", "Ocean", "Bleu-vert moderne", OceanCssVariables)
    ];
}
