using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Tool;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public List<ToolItem>? Tools { get; set; }

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        Tools = await _appService.GetAllToolItemsAsync();
    }
}