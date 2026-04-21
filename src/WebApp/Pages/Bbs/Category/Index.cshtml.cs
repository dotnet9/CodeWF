using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Bbs.Category;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public string CategoryName { get; set; } = "所有文章";
    public List<BlogPost>? Posts { get; set; }
    public List<CategoryItem>? Categories { get; set; }

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync(string slug)
    {
        Categories = await _appService.GetAllCategoryItemsAsync();
        var category = Categories?.FirstOrDefault(c => c.Slug == slug);
        if (category != null)
        {
            CategoryName = category.Name;
        }

        var pageData = await _appService.GetPostByCategory(1, 100, slug, null);
        Posts = pageData?.Data;
    }
}