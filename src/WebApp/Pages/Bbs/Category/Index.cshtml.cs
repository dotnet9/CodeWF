using WebApp.Models;
using WebApp.Services;
using WebApp.Options;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace WebApp.Pages.Bbs.Category;

public class IndexModel : PageModel
{
    private readonly AppService _appService;
    private readonly IOptions<SiteOption> _siteOption;

    public string CategoryName { get; set; } = "所有文章";
    public string? CategoryMemo { get; set; }
    public string? CurrentSlug { get; private set; }
    public bool IsDirectoryPage => string.IsNullOrWhiteSpace(CurrentSlug);
    public List<BlogPost> Posts { get; set; } = [];
    public List<CategoryItem> Categories { get; set; } = [];
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

        Categories = await _appService.GetAllCategoryItemsAsync() ?? [];
        if (string.IsNullOrWhiteSpace(slug))
        {
            CategoryName = "全部分类";
            CategoryMemo = "按分类浏览站内文章内容。";
            return;
        }

        if (string.Equals(slug, WebApp.Extensions.ConstantUtil.DefaultCategory, StringComparison.OrdinalIgnoreCase))
        {
            CategoryName = "所有分类";
            CategoryMemo = "聚焦同一技术主题下的文章，方便按方向连续阅读。";
            var defaultPageData = await _appService.GetPostByCategory(PageIndex, PageSize, slug, null);
            Posts = defaultPageData.Data;
            Total = defaultPageData.Total;
            return;
        }

        var category = Categories.FirstOrDefault(c => c.Slug == slug);
        if (category == null)
        {
            CategoryName = "分类不存在";
            Posts = [];
            Total = 0;
            return;
        }

        CategoryName = category.Name ?? CategoryName;
        CategoryMemo = category.Memo;
        var pageData = await _appService.GetPostByCategory(PageIndex, PageSize, slug, null);
        Posts = pageData.Data;
        Total = pageData.Total;
    }
}
