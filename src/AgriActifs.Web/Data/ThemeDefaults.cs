namespace AgriActifs.Web.Data;

public static class ThemeDefaults
{
    public const string DefaultCssVariables = """
        {
          "--gise-primary": "#1e40af",
          "--gise-primary-dark": "#1e3a8a",
          "--gise-accent": "#0ea5e9",
          "--gise-accent-soft": "#e0f2fe",
          "--gise-success": "#059669",
          "--gise-warning": "#d97706",
          "--gise-danger": "#dc2626",
          "--gise-sidebar": "#0f172a",
          "--gise-sidebar-hover": "#1e293b",
          "--gise-sidebar-active": "#2563eb",
          "--gise-surface": "#ffffff",
          "--gise-bg": "#f1f5f9",
          "--gise-border": "#e2e8f0",
          "--gise-text": "#0f172a",
          "--gise-text-muted": "#64748b"
        }
        """;

    public const string CorporateCssVariables = """
        {
          "--gise-primary": "#374151",
          "--gise-primary-dark": "#1f2937",
          "--gise-accent": "#6b7280",
          "--gise-accent-soft": "#f3f4f6",
          "--gise-success": "#059669",
          "--gise-warning": "#d97706",
          "--gise-danger": "#dc2626",
          "--gise-sidebar": "#111827",
          "--gise-sidebar-hover": "#1f2937",
          "--gise-sidebar-active": "#4b5563",
          "--gise-surface": "#ffffff",
          "--gise-bg": "#f9fafb",
          "--gise-border": "#e5e7eb",
          "--gise-text": "#111827",
          "--gise-text-muted": "#6b7280"
        }
        """;

    public const string OceanCssVariables = """
        {
          "--gise-primary": "#0d9488",
          "--gise-primary-dark": "#0f766e",
          "--gise-accent": "#06b6d4",
          "--gise-accent-soft": "#cffafe",
          "--gise-success": "#059669",
          "--gise-warning": "#d97706",
          "--gise-danger": "#dc2626",
          "--gise-sidebar": "#134e4a",
          "--gise-sidebar-hover": "#115e59",
          "--gise-sidebar-active": "#0d9488",
          "--gise-surface": "#ffffff",
          "--gise-bg": "#f0fdfa",
          "--gise-border": "#ccfbf1",
          "--gise-text": "#134e4a",
          "--gise-text-muted": "#5eead4"
        }
        """;

    public static IReadOnlyList<(int Id, string Code, string Name, string Description, string CssVariables)> SeedThemes =>
    [
        (1, "default", "GISEBS Default", "Palette bleue d'origine", DefaultCssVariables),
        (2, "corporate", "Corporate", "Tons neutres professionnels", CorporateCssVariables),
        (3, "ocean", "Ocean", "Bleu-vert moderne", OceanCssVariables)
    ];
}
