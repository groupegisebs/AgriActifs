using System.ComponentModel.DataAnnotations;

namespace AgriActifs.Web.Models.ViewModels;

public class SystemSettingsViewModel
{
    [Required]
    public string AppName { get; set; } = "GISEBS Secure MVC Starter";

    public string? Tagline { get; set; }
    public string? LogoUrl { get; set; }
    public int ActiveThemeId { get; set; } = 1;
    public string DefaultCulture { get; set; } = "fr-FR";

    public IReadOnlyList<ThemeListItemViewModel> AvailableThemes { get; set; } = [];
    public IReadOnlyList<string> AvailableCultures { get; set; } = ["fr-FR", "en-US"];

    public string DatabaseProvider { get; set; } = "PostgreSQL";
    public string DatabaseSchema { get; set; } = "agriactifs";
    public string DatabaseHost { get; set; } = string.Empty;

    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }

    [DataType(DataType.Password)]
    public string? SmtpPassword { get; set; }

    public bool SmtpUseSsl { get; set; } = true;
    public bool RequireConfirmedEmail { get; set; } = true;
    public bool RequireTwoFactor { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;

    public IFormFile? LogoFile { get; set; }
}

public class ThemeListItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
}

public class ThemeEditViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;

    public string Primary { get; set; } = "#1e40af";
    public string PrimaryDark { get; set; } = "#1e3a8a";
    public string Accent { get; set; } = "#0ea5e9";
    public string AccentSoft { get; set; } = "#e0f2fe";
    public string Sidebar { get; set; } = "#0f172a";
    public string SidebarHover { get; set; } = "#1e293b";
    public string SidebarActive { get; set; } = "#2563eb";
    public string Surface { get; set; } = "#ffffff";
    public string Background { get; set; } = "#f1f5f9";
    public string Border { get; set; } = "#e2e8f0";
    public string Text { get; set; } = "#0f172a";
    public string TextMuted { get; set; } = "#64748b";
}

public class LocalizationEditViewModel
{
    public string Culture { get; set; } = "fr-FR";
    public string YamlContent { get; set; } = string.Empty;
    public IReadOnlyList<string> MissingKeys { get; set; } = [];
    public IReadOnlyList<string> AvailableCultures { get; set; } = [];
}
