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

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);

    public IndexModel(AppService appService, IOptions<SiteOption> siteOption)
    {
        _appService = appService;
        _siteOption = siteOption;
    }

    public async Task OnGetAsync(string slug, int pageIndex = 1)
    {
        PageIndex = pageIndex > 0 ? pageIndex : 1;

        Albums = await _appService.GetAllAlbumItemsAsync();
        var album = Albums?.FirstOrDefault(c => c.Slug == slug);
        if (album != null)
        {
            AlbumName = album.Name ?? "所有专辑";
        }

        var pageData = await _appService.GetPostByAlbum(PageIndex, PageSize, slug, null);
        Posts = pageData?.Data;
        Total = pageData?.Total ?? 0;
    }
}
