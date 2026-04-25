using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public List<BlogPost> Posts { get; private set; } = [];
    public List<BlogPost> LatestPosts { get; private set; } = [];
    public List<AlbumItem> Albums { get; private set; } = [];
    public List<CategoryItem> Categories { get; private set; } = [];
    public int TotalPosts { get; private set; }
    public int TotalDocNodes { get; private set; }
    public int TotalToolEntries { get; private set; }

    public int TotalAlbums => Math.Max(0, Albums.Count(item => !string.Equals(item.Slug, "default", StringComparison.OrdinalIgnoreCase)));
    public int TotalCategories => Math.Max(0, Categories.Count(item => !string.Equals(item.Slug, "default", StringComparison.OrdinalIgnoreCase)));
    public BlogPost? SpotlightPost => LatestPosts.FirstOrDefault() ?? Posts.FirstOrDefault();

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        var allPosts = await _appService.GetAllBlogPostsAsync() ?? [];
        LatestPosts = allPosts.Take(4).ToList();
        Posts = (await _appService.GetBannerPostAsync())?.Take(6).ToList() ?? [];
        if (Posts.Count == 0)
        {
            Posts = allPosts.Take(6).ToList();
        }

        Albums = await _appService.GetAllAlbumItemsAsync() ?? [];
        Categories = await _appService.GetAllCategoryItemsAsync() ?? [];
        TotalPosts = allPosts.Count;

        var docItems = await _appService.GetAllDocItemsAsync() ?? [];
        TotalDocNodes = docItems.Count + docItems.Sum(item => item.Children?.Count ?? 0);

        var toolItems = await _appService.GetAllToolItemsAsync() ?? [];
        TotalToolEntries = toolItems.Sum(item => Math.Max(1, item.Children?.Count ?? 0));
    }
}
