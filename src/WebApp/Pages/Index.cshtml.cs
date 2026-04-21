using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public List<BlogPost>? Posts { get; set; }
    public List<AlbumItem>? Albums { get; set; }
    public List<CategoryItem>? Categories { get; set; }

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        Posts = await _appService.GetBannerPostAsync();
        Albums = await _appService.GetAllAlbumItemsAsync();
        Categories = await _appService.GetAllCategoryItemsAsync();
    }
}
