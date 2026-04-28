using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public sealed record HomeBrowseItem(string Name, string Slug, string? Memo, int PostCount);

public class IndexModel : PageModel
{
    private readonly AppService _appService;
    private const int FeaturedAlbumLimit = 4;
    private const int FeaturedCategoryLimit = 4;

    public List<BlogPost> Posts { get; private set; } = [];
    public List<BlogPost> LatestPosts { get; private set; } = [];
    public List<AlbumItem> Albums { get; private set; } = [];
    public List<CategoryItem> Categories { get; private set; } = [];
    public List<HomeBrowseItem> FeaturedAlbums { get; private set; } = [];
    public List<HomeBrowseItem> FeaturedCategories { get; private set; } = [];
    public int TotalPosts { get; private set; }
    public int TotalDocNodes { get; private set; }
    public int TotalToolEntries { get; private set; }
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

        FeaturedAlbums = Albums
            .Where(item =>
                !string.Equals(item.Slug, "default", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(item.Name)
                && !string.IsNullOrWhiteSpace(item.Slug))
            .Select(item => new HomeBrowseItem(
                item.Name!,
                item.Slug!,
                item.Memo,
                allPosts.Count(post => post.Albums?.Contains(item.Name, StringComparer.OrdinalIgnoreCase) == true)))
            .OrderByDescending(item => item.PostCount)
            .ThenBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .Take(FeaturedAlbumLimit)
            .ToList();

        FeaturedCategories = Categories
            .Where(item =>
                !string.Equals(item.Slug, "default", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(item.Name)
                && !string.IsNullOrWhiteSpace(item.Slug))
            .Select(item => new HomeBrowseItem(
                item.Name!,
                item.Slug!,
                item.Memo,
                allPosts.Count(post => post.Categories?.Contains(item.Name, StringComparer.OrdinalIgnoreCase) == true)))
            .OrderByDescending(item => item.PostCount)
            .ThenBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .Take(FeaturedCategoryLimit)
            .ToList();

        var docItems = await _appService.GetAllDocItemsAsync() ?? [];
        TotalDocNodes = docItems.Count + docItems.Sum(item => item.Children?.Count ?? 0);

        var toolItems = await _appService.GetAllToolItemsAsync() ?? [];
        TotalToolEntries = toolItems.Sum(item => Math.Max(1, item.Children?.Count ?? 0));
    }
}
