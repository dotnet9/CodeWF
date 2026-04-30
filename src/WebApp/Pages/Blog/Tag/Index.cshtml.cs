using WebApp.Extensions;
using WebApp.Models;
using WebApp.Options;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace WebApp.Pages.Blog.Tag;

public class IndexModel : PageModel
{
    private readonly AppService _appService;
    private readonly IOptions<SiteOption> _siteOption;

    public string TagName { get; set; } = "全部标签";
    public string? CurrentSlug { get; private set; }
    public bool IsDirectoryPage => string.IsNullOrWhiteSpace(CurrentSlug);
    public List<BlogPostBrief> Posts { get; set; } = [];
    public List<TagItem> Tags { get; set; } = [];
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
        CurrentSlug = string.IsNullOrWhiteSpace(slug)
            ? null
            : ConstantUtil.DecodeTagSlug(slug);
        PageIndex = pageIndex > 0 ? pageIndex : 1;

        Tags = await _appService.GetAllTagItemsAsync();
        if (string.IsNullOrWhiteSpace(CurrentSlug))
        {
            TagName = "全部标签";
            return;
        }

        var tag = Tags.FirstOrDefault(item =>
            string.Equals(item.Name, CurrentSlug, StringComparison.OrdinalIgnoreCase));
        if (tag == null)
        {
            TagName = "标签不存在";
            Posts = [];
            Total = 0;
            return;
        }

        TagName = tag.Name;
        var pageData = await _appService.GetPostByTag(PageIndex, PageSize, TagName);
        Posts = pageData.Data;
        Total = pageData.Total;
    }

    public string GetTagUrl(string tag) => ConstantUtil.GetTagUrl(tag);
}
