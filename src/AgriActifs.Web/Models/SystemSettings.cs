namespace AgriActifs.Web.Models;

public class SystemSettings
{
    public int Id { get; set; }
    public string AppName { get; set; } = "AgriActifs";
    public string? Tagline { get; set; }
    public string? LogoUrl { get; set; }
    public int ActiveThemeId { get; set; } = 1;
    public string DefaultCulture { get; set; } = "fr-FR";
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
    public bool RequireConfirmedEmail { get; set; } = true;
    public bool RequireTwoFactor { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
