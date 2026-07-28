using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Options;
using AgriActifs.Web.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AgriActifs.Web.Controllers;

[AllowAnonymous]
public class ContactController(
    IOptions<DemoOptions> demoOptions,
    IOptions<AuthMessageSenderOptions> emailOptions,
    IEmailSender emailSender,
    ILogger<ContactController> logger) : Controller
{
    /// <summary>Toutes les demandes promoteur sont adressées à ce destinataire.</summary>
    public const string ContactInbox = "CEO@GISEBS.COM";

    private readonly DemoOptions _demo = demoOptions.Value;
    private readonly AuthMessageSenderOptions _email = emailOptions.Value;

    [HttpGet]
    public IActionResult Promoteur(DemandePromoteurType? type = null)
    {
        if (!_demo.Enabled)
            return NotFound();

        return View(CreateModel(type ?? DemandePromoteurType.Demonstration));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Promoteur(PromoteurContactViewModel model)
    {
        if (!_demo.Enabled)
            return NotFound();

        model.PromoterName = _demo.PromoterName;
        model.PromoterEmail = ContactInbox;
        model.PromoterPhone = _demo.PromoterPhone;

        if (!ModelState.IsValid)
            return View(model);

        if (string.IsNullOrWhiteSpace(_email.SendGridKey))
        {
            ModelState.AddModelError(string.Empty,
                "L'envoi courriel n'est pas configuré (Email:SendGridKey). La demande n'a pas pu être transmise à CEO@GISEBS.COM.");
            return View(model);
        }

        var typeLabel = GetDisplayName(model.TypeDemande);
        var subject = $"[AgriActifs Démo] {typeLabel} — {model.Organisation}";
        var body = $"""
            <p><strong>Nouvelle demande promoteur (AgriActifs — environnement démo)</strong></p>
            <p>Destinataire : <strong>{ContactInbox}</strong></p>
            <ul>
              <li><strong>Type :</strong> {System.Net.WebUtility.HtmlEncode(typeLabel)}</li>
              <li><strong>Nom :</strong> {System.Net.WebUtility.HtmlEncode(model.Nom)}</li>
              <li><strong>Organisation :</strong> {System.Net.WebUtility.HtmlEncode(model.Organisation)}</li>
              <li><strong>Courriel (répondre à) :</strong> <a href="mailto:{System.Net.WebUtility.HtmlEncode(model.Email)}">{System.Net.WebUtility.HtmlEncode(model.Email)}</a></li>
              <li><strong>Téléphone :</strong> {System.Net.WebUtility.HtmlEncode(model.Telephone ?? "—")}</li>
            </ul>
            <p><strong>Message :</strong></p>
            <p>{System.Net.WebUtility.HtmlEncode(model.Message).Replace("\n", "<br/>")}</p>
            """;

        try
        {
            await emailSender.SendEmailAsync(ContactInbox, subject, body);
            logger.LogInformation(
                "Contact promoteur envoyé via EmailSender à {Inbox}. Type={Type} From={Email} Org={Org}",
                ContactInbox, typeLabel, model.Email, model.Organisation);
            TempData["Success"] = $"Votre demande a été transmise par courriel à {ContactInbox}. Nous vous recontacterons sous peu.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Échec envoi contact promoteur via EmailSender vers {Inbox}. Type={Type} From={Email}",
                ContactInbox, typeLabel, model.Email);
            ModelState.AddModelError(string.Empty,
                $"Échec de l'envoi vers {ContactInbox}. Réessayez plus tard ou contactez-nous directement.");
            return View(model);
        }

        return RedirectToAction(nameof(Promoteur), new { type = model.TypeDemande });
    }

    private PromoteurContactViewModel CreateModel(DemandePromoteurType type) => new()
    {
        TypeDemande = type,
        PromoterName = _demo.PromoterName,
        PromoterEmail = ContactInbox,
        PromoterPhone = _demo.PromoterPhone
    };

    private static string GetDisplayName(DemandePromoteurType type)
    {
        var member = typeof(DemandePromoteurType).GetMember(type.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? type.ToString();
    }
}
