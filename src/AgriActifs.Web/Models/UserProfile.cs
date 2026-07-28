namespace AgriActifs.Web.Models;

public class UserProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string PreferredLanguage { get; set; } = "fr-FR";
    public string Theme { get; set; } = "light";
    public string? TimeZone { get; set; }
    public bool EmailNotifications { get; set; } = true;

    public Identity.ApplicationUser? User { get; set; }
}
