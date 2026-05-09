using Microsoft.AspNetCore.Razor.TagHelpers;
using WebApp.Services;

namespace WebApp.TagHelpers;

[HtmlTargetElement("a", Attributes = "href")]
[HtmlTargetElement("form", Attributes = "action")]
public sealed class LocalizedLinkTagHelper : TagHelper
{
    private static readonly HashSet<string> LocalizedAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "href",
        "action"
    };

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        foreach (var attributeName in LocalizedAttributes)
        {
            var attribute = output.Attributes[attributeName];
            var value = attribute?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            output.Attributes.SetAttribute(attributeName, RequestLanguage.LocalizePath(value));
        }
    }
}
