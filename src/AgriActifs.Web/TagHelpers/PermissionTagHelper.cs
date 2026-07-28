using AgriActifs.Web.Models.Authorization;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AgriActifs.Web.TagHelpers;

/// <summary>
/// Affiche le contenu uniquement si l'utilisateur a la permission configurée en BD.
/// Usage: &lt;gise-permission resource="User" action="View" property="Email"&gt;...&lt;/gise-permission&gt;
/// </summary>
[HtmlTargetElement("gise-permission")]
public class PermissionTagHelper(IDynamicPermissionService permissionService, IHttpContextAccessor httpContextAccessor) : TagHelper
{
    [HtmlAttributeName("resource")]
    public string Resource { get; set; } = string.Empty;

    [HtmlAttributeName("action")]
    public PermissionAction Action { get; set; } = PermissionAction.View;

    [HtmlAttributeName("property")]
    public string? Property { get; set; }

    [HtmlAttributeName("code")]
    public string? Code { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        var user = httpContextAccessor.HttpContext?.User;

        if (user is null)
        {
            output.SuppressOutput();
            return;
        }

        var allowed = !string.IsNullOrWhiteSpace(Code)
            ? await permissionService.HasPermissionAsync(user, Code)
            : await permissionService.HasPermissionAsync(user, Resource, Action, Property);

        if (!allowed)
        {
            output.SuppressOutput();
            return;
        }

        await output.GetChildContentAsync();
    }
}
