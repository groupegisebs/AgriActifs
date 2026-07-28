using System.Text.Json.Serialization;

namespace AgriActifs.Web.Services.Email;

public sealed class MailGatewaySendRequest
{
    [JsonPropertyName("clientCode")]
    public string ClientCode { get; set; } = string.Empty;

    [JsonPropertyName("templateCode")]
    public string TemplateCode { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public List<string> To { get; set; } = [];

    [JsonPropertyName("subjectData")]
    public Dictionary<string, string>? SubjectData { get; set; }

    [JsonPropertyName("bodyData")]
    public Dictionary<string, string>? BodyData { get; set; }
}

public sealed class MailGatewaySendResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("mailCode")]
    public string? MailCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
