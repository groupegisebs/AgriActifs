namespace AgriActifs.Web.Services.Email;

public class AuthMessageSenderOptions
{
    public const string SectionName = "Email";

    public string? SendGridKey { get; set; }
    public string FromEmail { get; set; } = "noreply@gisebs.local";
    public string FromName { get; set; } = "GISEBS Secure MVC";
}
