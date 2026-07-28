using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace AgriActifs.Web.TagHelpers;

[HtmlTargetElement("gise-t")]
public class GiseTTagHelper(IStringLocalizerFactory localizerFactory) : TagHelper
{
    [HtmlAttributeName("key")]
    public string Key { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        var localizer = localizerFactory.Create(typeof(GiseTTagHelper));
        output.Content.SetContent(localizer[Key].Value);
    }
}
