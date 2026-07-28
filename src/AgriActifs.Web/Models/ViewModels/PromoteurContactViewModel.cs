using System.ComponentModel.DataAnnotations;

namespace AgriActifs.Web.Models.ViewModels;

public enum DemandePromoteurType
{
    [Display(Name = "Demande de quotation / devis")]
    Quotation = 0,

    [Display(Name = "Demande d'information")]
    Information = 1,

    [Display(Name = "Demande de démonstration")]
    Demonstration = 2,

    [Display(Name = "Demande de formation")]
    Formation = 3,

    [Display(Name = "Demande de partenariat")]
    Partenariat = 4,

    [Display(Name = "Autre")]
    Autre = 5
}

public class PromoteurContactViewModel
{
    [Required(ErrorMessage = "Le type de demande est obligatoire.")]
    [Display(Name = "Type de demande")]
    public DemandePromoteurType TypeDemande { get; set; } = DemandePromoteurType.Demonstration;

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(120)]
    [Display(Name = "Nom complet")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'entreprise ou l'exploitation est obligatoire.")]
    [StringLength(160)]
    [Display(Name = "Entreprise / exploitation")]
    public string Organisation { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse courriel est obligatoire.")]
    [EmailAddress]
    [StringLength(200)]
    [Display(Name = "Courriel")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(40)]
    [Display(Name = "Téléphone")]
    public string? Telephone { get; set; }

    [Required(ErrorMessage = "Le message est obligatoire.")]
    [StringLength(4000, MinimumLength = 10)]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    public string? PromoterName { get; set; }
    public string? PromoterEmail { get; set; }
    public string? PromoterPhone { get; set; }
}
