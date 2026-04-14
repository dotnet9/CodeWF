using CodeWF.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class AboutModel : PageModel
{
    private readonly AppService _appService;

    public string? HtmlContent { get; set; }

    public AboutModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        var (_, htmlContent) = await _appService.ReadAboutAsync();
        HtmlContent = htmlContent;
    }
}