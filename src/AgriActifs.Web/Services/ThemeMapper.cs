using System.Text.Json;
using AgriActifs.Web.Models;
using AgriActifs.Web.Models.ViewModels;

namespace AgriActifs.Web.Services;

public static class ThemeMapper
{
    public static ThemeEditViewModel ToViewModel(ThemeDefinition theme)
    {
        var vars = ParseVars(theme.CssVariables);
        return new ThemeEditViewModel
        {
            Id = theme.Id,
            Code = theme.Code,
            Name = theme.Name,
            Description = theme.Description,
            IsSystem = theme.IsSystem,
            IsActive = theme.IsActive,
            Primary = Get(vars, "--gise-primary", "#1e40af"),
            PrimaryDark = Get(vars, "--gise-primary-dark", "#1e3a8a"),
            Accent = Get(vars, "--gise-accent", "#0ea5e9"),
            AccentSoft = Get(vars, "--gise-accent-soft", "#e0f2fe"),
            Sidebar = Get(vars, "--gise-sidebar", "#0f172a"),
            SidebarHover = Get(vars, "--gise-sidebar-hover", "#1e293b"),
            SidebarActive = Get(vars, "--gise-sidebar-active", "#2563eb"),
            Surface = Get(vars, "--gise-surface", "#ffffff"),
            Background = Get(vars, "--gise-bg", "#f1f5f9"),
            Border = Get(vars, "--gise-border", "#e2e8f0"),
            Text = Get(vars, "--gise-text", "#0f172a"),
            TextMuted = Get(vars, "--gise-text-muted", "#64748b")
        };
    }

    public static ThemeDefinition ToEntity(ThemeEditViewModel model) => new()
    {
        Id = model.Id,
        Code = model.Code,
        Name = model.Name,
        Description = model.Description,
        IsSystem = model.IsSystem,
        IsActive = model.IsActive,
        CssVariables = SerializeVars(model)
    };

    public static string SerializeVars(ThemeEditViewModel model) =>
        JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["--gise-primary"] = model.Primary,
            ["--gise-primary-dark"] = model.PrimaryDark,
            ["--gise-accent"] = model.Accent,
            ["--gise-accent-soft"] = model.AccentSoft,
            ["--gise-success"] = "#059669",
            ["--gise-warning"] = "#d97706",
            ["--gise-danger"] = "#dc2626",
            ["--gise-sidebar"] = model.Sidebar,
            ["--gise-sidebar-hover"] = model.SidebarHover,
            ["--gise-sidebar-active"] = model.SidebarActive,
            ["--gise-surface"] = model.Surface,
            ["--gise-bg"] = model.Background,
            ["--gise-border"] = model.Border,
            ["--gise-text"] = model.Text,
            ["--gise-text-muted"] = model.TextMuted
        });

    private static Dictionary<string, string> ParseVars(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string Get(Dictionary<string, string> vars, string key, string fallback) =>
        vars.TryGetValue(key, out var v) ? v : fallback;
}
