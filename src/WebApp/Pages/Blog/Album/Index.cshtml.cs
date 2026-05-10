using WebApp.Models;
using WebApp.Services;
using WebApp.Options;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace WebApp.Pages.Blog.Album;

public class IndexModel : PageModel
{
    private readonly AppService _appService;
    private readonly IOptions<SiteOption> _siteOption;

    public string AlbumName { get; set; } = "所有专辑";
    public string? AlbumMemo { get; set; }
    public string? CurrentSlug { get; private set; }
    public bool IsDirectoryPage => string.IsNullOrWhiteSpace(CurrentSlug);
    public bool IsDefaultPage => string.Equals(CurrentSlug, WebApp.Extensions.ConstantUtil.DefaultCategory, StringComparison.OrdinalIgnoreCase);
    public List<BlogPostBrief> Posts { get; set; } = [];
    public List<AlbumItem> Albums { get; set; } = [];
    public string Owner => _siteOption.Value.Owner ?? _siteOption.Value.AppTitle ?? "码坊";

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);

    public IndexModel(AppService appService, IOptions<SiteOption> siteOption)
    {
        _appService = appService;
        _siteOption = siteOption;
    }

    public async Task OnGetAsync(string? slug, int pageIndex = 1)
    {
        CurrentSlug = slug;
        PageIndex = pageIndex > 0 ? pageIndex : 1;

        Albums = await _appService.GetAllAlbumItemsAsync() ?? [];
        if (string.IsNullOrWhiteSpace(slug))
        {
            AlbumName = "全部专题";
            AlbumMemo = null;
            return;
        }

        if (string.Equals(slug, WebApp.Extensions.ConstantUtil.DefaultCategory, StringComparison.OrdinalIgnoreCase))
        {
            AlbumName = "所有专辑";
            AlbumMemo = null;
            var defaultPageData = await _appService.GetPostByAlbum(PageIndex, PageSize, slug, null);
            Posts = defaultPageData.Data;
            Total = defaultPageData.Total;
            return;
        }

        var album = Albums.FirstOrDefault(c => c.Slug == slug);
        if (album == null)
        {
            AlbumName = WebApp.Extensions.ConstantUtil.DecodeTagSlug(slug);
            AlbumMemo = null;
        }
        else
        {
            AlbumName = album.Name ?? "所有专辑";
            AlbumMemo = album.Memo;
        }

        var pageData = await _appService.GetPostByAlbum(PageIndex, PageSize, slug, null);
        Posts = pageData.Data;
        Total = pageData.Total;
    }
}
