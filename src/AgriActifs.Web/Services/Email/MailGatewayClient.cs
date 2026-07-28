using System.Net.Http.Json;
using System.Text.Json;
using AgriActifs.Web.Configuration;
using Microsoft.Extensions.Options;

namespace AgriActifs.Web.Services.Email;

public interface IMailGatewayClient
{
    bool IsConfigured { get; }
    Task<bool> TrySendTransactionalAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public class MailGatewayClient(
    IHttpClientFactory httpClientFactory,
    IOptions<MailGatewayOptions> options,
    ILogger<MailGatewayClient> logger) : IMailGatewayClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly MailGatewayOptions _options = options.Value;

    public bool IsConfigured => _options.IsConfigured;

    public async Task<bool> TrySendTransactionalAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
            return false;

        try
        {
            var client = httpClientFactory.CreateClient("MailGateway");
            var request = new MailGatewaySendRequest
            {
                ClientCode = _options.ClientCode,
                TemplateCode = _options.TransactionalTemplateCode,
                To = [toEmail.Trim()],
                SubjectData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Subject"] = subject
                },
                BodyData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Subject"] = subject,
                    ["HtmlBody"] = htmlBody
                }
            };

            using var response = await client.PostAsJsonAsync("api/mail/send", request, JsonOptions, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            MailGatewaySendResponse? payload = null;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    payload = JsonSerializer.Deserialize<MailGatewaySendResponse>(raw, JsonOptions);
                }
                catch (JsonException)
                {
                    // body non JSON
                }
            }

            if (response.IsSuccessStatusCode && payload?.Success == true)
            {
                logger.LogInformation(
                    "Email TRANSACTIONAL mis en file SecureMail pour {Email} — {MailCode} ({Status})",
                    toEmail, payload.MailCode, payload.Status);
                return true;
            }

            var error = payload?.Error
                ?? (string.IsNullOrWhiteSpace(raw) ? response.ReasonPhrase : raw.Length > 500 ? raw[..500] : raw);
            logger.LogError(
                "Échec SecureMail pour {Email}. HTTP {Status}. {Error}",
                toEmail, (int)response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception SecureMail pour {Email}", toEmail);
            return false;
        }
    }
}
