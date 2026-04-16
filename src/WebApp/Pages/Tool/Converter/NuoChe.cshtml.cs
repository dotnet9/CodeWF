using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HashidsNet;
using CodeWF.Options;
using Microsoft.Extensions.Options;

namespace WebApp.Pages.Tool.Converter;

[IgnoreAntiforgeryToken]
public class NuoCheModel : PageModel
{
    private readonly IOptions<SiteOption> _siteOption;

    public NuoCheModel(IOptions<SiteOption> siteOption)
    {
        _siteOption = siteOption;
    }

    [BindProperty(SupportsGet = true)]
    public string? P { get; set; }

    public long? DecodePhone { get; set; }

    public void OnGet()
    {
        if (!string.IsNullOrWhiteSpace(P))
        {
            try
            {
                DecodePhone = new Hashids("codewf").DecodeLong(P)[0];
            }
            catch
            {
                DecodePhone = null;
            }
        }
    }
}