using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public List<BlogPost>? Posts { get; set; }

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        Posts = await _appService.GetBannerPostAsync();
    }
}
