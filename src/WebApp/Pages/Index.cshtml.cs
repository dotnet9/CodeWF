using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public List<BlogPost> Posts { get; private set; } = [];
    public List<AlbumItem> Albums { get; private set; } = [];
    public List<CategoryItem> Categories { get; private set; } = [];
    public int TotalPosts { get; private set; }

    public int TotalAlbums => Math.Max(0, Albums.Count(item => !string.Equals(item.Slug, "default", StringComparison.OrdinalIgnoreCase)));
    public int TotalCategories => Math.Max(0, Categories.Count(item => !string.Equals(item.Slug, "default", StringComparison.OrdinalIgnoreCase)));
    public BlogPost? SpotlightPost => Posts.FirstOrDefault();

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        var allPosts = await _appService.GetAllBlogPostsAsync();
        Posts = (await _appService.GetBannerPostAsync())?.Take(6).ToList() ?? [];
        Albums = await _appService.GetAllAlbumItemsAsync() ?? [];
        Categories = await _appService.GetAllCategoryItemsAsync() ?? [];
        TotalPosts = allPosts?.Count ?? 0;
    }
}
