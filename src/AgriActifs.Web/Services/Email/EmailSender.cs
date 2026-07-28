using AgriActifs.Web.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AgriActifs.Web.Services.Email;

/// <summary>
/// Priorité : SecureMail (MailGateway) → SendGrid → log uniquement.
/// </summary>
public class EmailSender(
    IOptions<AuthMessageSenderOptions> optionsAccessor,
    IMailGatewayClient mailGateway,
    ILogger<EmailSender> logger) : IEmailSender
{
    private readonly AuthMessageSenderOptions _options = optionsAccessor.Value;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (await mailGateway.TrySendTransactionalAsync(email, subject, htmlMessage))
            return;

        if (!string.IsNullOrWhiteSpace(_options.SendGridKey))
        {
            await SendViaSendGridAsync(email, subject, htmlMessage);
            return;
        }

        logger.LogWarning(
            """
            [EMAIL - aucun canal configuré (SecureMail ApiKey ou SendGridKey)]
            To: {Email}
            Subject: {Subject}
            Body:
            {Body}
            """,
            email, subject, htmlMessage);
    }

    private async Task SendViaSendGridAsync(string email, string subject, string htmlMessage)
    {
        var client = new SendGridClient(_options.SendGridKey);
        var message = new SendGridMessage
        {
            From = new EmailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            PlainTextContent = htmlMessage,
            HtmlContent = htmlMessage
        };
        message.AddTo(new EmailAddress(email));
        message.SetClickTracking(false, false);

        var response = await client.SendEmailAsync(message);
        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Email envoyé à {Email} via SendGrid — sujet: {Subject}", email, subject);
            return;
        }

        var body = await response.Body.ReadAsStringAsync();
        logger.LogError(
            "Échec envoi email à {Email}. Status: {Status}. Response: {Response}",
            email, response.StatusCode, body);
        throw new InvalidOperationException($"Échec envoi email SendGrid: {response.StatusCode}");
    }
}
