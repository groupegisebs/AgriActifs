using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AgriActifs.Web.Controllers;

[AllowAnonymous]
public class ContactController(
    IOptions<DemoOptions> demoOptions,
    IEmailSender emailSender,
    ILogger<ContactController> logger) : Controller
{
    private readonly DemoOptions _demo = demoOptions.Value;

    [HttpGet]
    public IActionResult Promoteur(DemandePromoteurType? type = null)
    {
        if (!_demo.Enabled)
            return NotFound();

        var model = CreateModel(type ?? DemandePromoteurType.Demonstration);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Promoteur(PromoteurContactViewModel model)
    {
        if (!_demo.Enabled)
            return NotFound();

        model.PromoterName = _demo.PromoterName;
        model.PromoterEmail = _demo.PromoterEmail;
        model.PromoterPhone = _demo.PromoterPhone;

        if (!ModelState.IsValid)
            return View(model);

        var typeLabel = GetDisplayName(model.TypeDemande);
        var subject = $"[AgriActifs Démo] {typeLabel} — {model.Organisation}";
        var body = $"""
            <p><strong>Nouvelle demande promoteur (environnement démo)</strong></p>
            <ul>
              <li><strong>Type :</strong> {System.Net.WebUtility.HtmlEncode(typeLabel)}</li>
              <li><strong>Nom :</strong> {System.Net.WebUtility.HtmlEncode(model.Nom)}</li>
              <li><strong>Organisation :</strong> {System.Net.WebUtility.HtmlEncode(model.Organisation)}</li>
              <li><strong>Courriel :</strong> {System.Net.WebUtility.HtmlEncode(model.Email)}</li>
              <li><strong>Téléphone :</strong> {System.Net.WebUtility.HtmlEncode(model.Telephone ?? "—")}</li>
            </ul>
            <p><strong>Message :</strong></p>
            <p>{System.Net.WebUtility.HtmlEncode(model.Message).Replace("\n", "<br/>")}</p>
            """;

        try
        {
            await emailSender.SendEmailAsync(_demo.PromoterEmail, subject, body);
            TempData["Success"] = "Votre demande a été transmise au promoteur. Nous vous recontacterons sous peu.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Contact promoteur enregistré localement (envoi email échoué). Type={Type} From={Email} Org={Org}",
                typeLabel, model.Email, model.Organisation);
            TempData["Success"] =
                "Votre demande a été enregistrée. Le promoteur a été notifié (ou le sera dès que l'envoi courriel sera disponible).";
        }

        return RedirectToAction(nameof(Promoteur), new { type = model.TypeDemande });
    }

    private PromoteurContactViewModel CreateModel(DemandePromoteurType type) => new()
    {
        TypeDemande = type,
        PromoterName = _demo.PromoterName,
        PromoterEmail = _demo.PromoterEmail,
        PromoterPhone = _demo.PromoterPhone
    };

    private static string GetDisplayName(DemandePromoteurType type)
    {
        var member = typeof(DemandePromoteurType).GetMember(type.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? type.ToString();
    }
}
