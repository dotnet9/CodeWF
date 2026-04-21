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
    public List<BlogPost>? Posts { get; set; }
    public List<CategoryItem>? Categories { get; set; }
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

        Categories = await _appService.GetAllCategoryItemsAsync();
        var category = Categories?.FirstOrDefault(c => c.Slug == slug);
        if (category != null)
        {
            CategoryName = category.Name;
        }

        var pageData = await _appService.GetPostByCategory(PageIndex, PageSize, slug, null);
        Posts = pageData?.Data;
        Total = pageData?.Total ?? 0;
    }
}