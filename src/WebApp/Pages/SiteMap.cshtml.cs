using CodeWF.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class SiteMapModel : PageModel
{
    private readonly AppService _appService;

    public string SiteMapContent { get; set; } = string.Empty;

    public SiteMapModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        SiteMapContent = await _appService.GetSiteMapAsync();
    }
}
