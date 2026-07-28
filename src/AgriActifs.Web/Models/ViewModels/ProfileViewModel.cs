using System.ComponentModel.DataAnnotations;

namespace AgriActifs.Web.Models.ViewModels;

public class ProfileViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Prénom")]
    public string? FirstName { get; set; }

    [Display(Name = "Nom")]
    public string? LastName { get; set; }

    [Phone, Display(Name = "Téléphone")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Entreprise")]
    public string? Company { get; set; }

    [Display(Name = "Poste")]
    public string? JobTitle { get; set; }

    [Display(Name = "Langue")]
    public string PreferredLanguage { get; set; } = "fr-FR";

    [Display(Name = "Thème")]
    public string Theme { get; set; } = "light";

    [Display(Name = "URL photo")]
    public string? PhotoUrl { get; set; }

    [Display(Name = "Notifications email")]
    public bool EmailNotifications { get; set; } = true;
}
