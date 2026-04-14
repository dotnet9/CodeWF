using CodeWF.Models;
using CodeWF.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Bbs.Album;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public string AlbumName { get; set; } = "所有专辑";
    public List<BlogPost>? Posts { get; set; }
    public List<AlbumItem>? Albums { get; set; }

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync(string slug)
    {
        Albums = await _appService.GetAllAlbumItemsAsync();
        var album = Albums?.FirstOrDefault(c => c.Slug == slug);
        if (album != null)
        {
            AlbumName = album.Name ?? "所有专辑";
        }

        var pageData = await _appService.GetPostByAlbum(1, 100, slug, null);
        Posts = pageData?.Data;
    }
}
