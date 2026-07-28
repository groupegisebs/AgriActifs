using System.ComponentModel.DataAnnotations;
using AgriActifs.Web.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace AgriActifs.Web.Areas.Identity.Pages.Account;

public class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration,
    IStringLocalizer<LoginModel> localizer,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public bool AllowPublicRegistration { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        AllowPublicRegistration = configuration.GetValue("Security:AllowPublicRegistration", false);
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        AllowPublicRegistration = configuration.GetValue("Security:AllowPublicRegistration", false);

        if (!ModelState.IsValid)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("Utilisateur connecté : {Email}", Input.Email);
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });

        if (result.IsLockedOut)
        {
            logger.LogWarning("Compte verrouillé : {Email}", Input.Email);
            return RedirectToPage("./Lockout");
        }

        if (result.IsNotAllowed)
            ModelState.AddModelError(string.Empty, localizer["Auth.Login.AccountNotActivated"]);
        else
            ModelState.AddModelError(string.Empty, localizer["Auth.Login.InvalidCredentials"]);

        ReturnUrl = returnUrl;
        return Page();
    }
}
