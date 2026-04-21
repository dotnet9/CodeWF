using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Doc;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public List<DocItem>? DocItems { get; set; }

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        DocItems = await _appService.GetAllDocItemsAsync();
    }
}
