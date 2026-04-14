using CodeWF.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class RssModel : PageModel
{
    private readonly AppService _appService;

    public string RssContent { get; set; } = string.Empty;

    public RssModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        RssContent = await _appService.GetRssAsync();
    }
}
