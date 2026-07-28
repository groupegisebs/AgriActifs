namespace AgriActifs.Web.Configuration;

public class MailGatewayOptions
{
    public const string SectionName = "Email:MailGateway";

    public string BaseUrl { get; set; } = "https://gisemailsender.gisebs.com";
    public string ApiKey { get; set; } = string.Empty;
    public string ClientCode { get; set; } = "AGRIACTIFS";
    public string TransactionalTemplateCode { get; set; } = "TRANSACTIONAL";
    public int RequestTimeoutSeconds { get; set; } = 30;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl)
        && !string.IsNullOrWhiteSpace(ApiKey)
        && !string.IsNullOrWhiteSpace(ClientCode);

    public Uri GetBaseUri()
    {
        if (!Uri.TryCreate(BaseUrl.TrimEnd('/'), UriKind.Absolute, out var uri))
            throw new InvalidOperationException("Email:MailGateway:BaseUrl invalide.");

        return uri;
    }
}
