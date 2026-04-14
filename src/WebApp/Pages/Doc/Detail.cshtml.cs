using CodeWF.Models;
using CodeWF.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Doc;

public class DetailModel : PageModel
{
    private readonly AppService _appService;

    public DocItem? Doc { get; set; }
    public List<DocItem>? DocItems { get; set; }

    public DetailModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync(string slug)
    {
        DocItems = await _appService.GetAllDocItemsAsync();
        Doc = await _appService.GetDocItemAsync(slug);
    }
}
