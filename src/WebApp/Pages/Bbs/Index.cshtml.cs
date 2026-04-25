using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Bbs;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public List<BlogPost> Posts { get; private set; } = [];
    public List<CategoryItem> Categories { get; private set; } = [];
    public int PageIndex { get; private set; } = 1;
    public int PageSize { get; private set; } = 10;
    public int Total { get; private set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync(int pageIndex = 1)
    {
        PageIndex = pageIndex > 0 ? pageIndex : 1;
        var pageData = await _appService.GetPagedBlogPostsAsync(PageIndex, PageSize);
        Posts = pageData.Data;
        Total = pageData.Total;
        Categories = await _appService.GetAllCategoryItemsAsync() ?? [];
    }
}
