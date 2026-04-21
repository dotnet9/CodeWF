using WebApp.Models;
using WebApp.Services;
using WebApp.Options;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace WebApp.Pages.Bbs.Album;

public class IndexModel : PageModel
{
    private readonly AppService _appService;
    private readonly IOptions<SiteOption> _siteOption;

    public string AlbumName { get; set; } = "所有专辑";
    public List<BlogPost>? Posts { get; set; }
    public List<AlbumItem>? Albums { get; set; }
    public string Owner => _siteOption.Value.Owner ?? "沙漠尽头的狼";

    public IndexModel(AppService appService, IOptions<SiteOption> siteOption)
    {
        _appService = appService;
        _siteOption = siteOption;
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
