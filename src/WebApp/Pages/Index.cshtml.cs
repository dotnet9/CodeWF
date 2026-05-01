using WebApp.Models;
using WebApp.Services;
using WebApp.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public sealed record HomeBrowseItem(string Name, string Slug, string? Memo, int PostCount);

public class IndexModel : PageModel
{
    private readonly AppService _appService;
    private const int FeaturedAlbumLimit = 4;
    private const int FeaturedCategoryLimit = 4;

    public List<BlogPostBrief> Posts { get; private set; } = [];
    public List<BlogPostBrief> HeroPosts { get; private set; } = [];
    public List<BlogPostBrief> LatestPosts { get; private set; } = [];
    public List<AlbumItem> Albums { get; private set; } = [];
    public List<CategoryItem> Categories { get; private set; } = [];
    public List<HomeBrowseItem> FeaturedAlbums { get; private set; } = [];
    public List<HomeBrowseItem> FeaturedCategories { get; private set; } = [];
    public List<DiscoveryLinkCard> GettingStartedLinks { get; private set; } = [];
    public List<DiscoveryPostCard> DiscoveryPosts { get; private set; } = [];
    public int TotalPosts { get; private set; }
    public int TotalDocNodes { get; private set; }
    public int TotalToolEntries { get; private set; }
    public BlogPostBrief? SpotlightPost => LatestPosts.FirstOrDefault() ?? Posts.FirstOrDefault();

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        var allPosts = await _appService.GetAllBlogPostBriefsAsync() ?? [];
        LatestPosts = allPosts.Take(4).ToList();
        // Hero 区域使用随机文章，避免首页长期只被同一组内容占据。
        HeroPosts = TakeRandom(allPosts, 2);
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

        GettingStartedLinks = BuildGettingStartedLinks(allPosts, FeaturedCategories, FeaturedAlbums);
        DiscoveryPosts = BuildDiscoveryPosts(
            allPosts.Where(post =>
                Posts.All(featured => !string.Equals(featured.Slug, post.Slug, StringComparison.OrdinalIgnoreCase)))
            .ToList(),
            3);

        var docItems = await _appService.GetAllDocItemsAsync() ?? [];
        TotalDocNodes = docItems.Count + docItems.Sum(item => item.Children?.Count ?? 0);

        var toolItems = await _appService.GetAllToolItemsAsync() ?? [];
        TotalToolEntries = toolItems.Sum(item => Math.Max(1, item.Children?.Count ?? 0));
    }

    private static List<DiscoveryLinkCard> BuildGettingStartedLinks(
        IReadOnlyList<BlogPostBrief> allPosts,
        IReadOnlyList<HomeBrowseItem> categories,
        IReadOnlyList<HomeBrowseItem> albums)
    {
        var links = new List<DiscoveryLinkCard>();

        if (allPosts.FirstOrDefault() is { } latestPost)
        {
            links.Add(new DiscoveryLinkCard(
                "从这里开始",
                "先看最新更新",
                latestPost.Title ?? "最近更新",
                ConstantUtil.GetPostUrl(latestPost)));
        }

        if (categories.FirstOrDefault() is { } category)
        {
            links.Add(new DiscoveryLinkCard(
                "内容地图",
                $"先逛 {category.Name}",
                $"{category.PostCount} 篇文章，适合快速熟悉站内内容结构",
                ConstantUtil.GetCategoryUrl(category.Slug)));
        }

        if (PickRandom(albums.Where(static item => item.PostCount > 0).ToList()) is { } album)
        {
            links.Add(new DiscoveryLinkCard(
                "连续阅读",
                $"跟着专题读 {album.Name}",
                $"{album.PostCount} 篇文章，适合按主题连续阅读",
                ConstantUtil.GetAlbumUrl(album.Slug)));
        }

        links.Add(new DiscoveryLinkCard(
            "项目索引",
            "看看开源项目",
            "这里整理了开源项目、NuGet 包和对应的使用说明。",
            ConstantUtil.GetProjectDirectoryUrl()));

        return links.Take(4).ToList();
    }

    private static List<DiscoveryPostCard> BuildDiscoveryPosts(IReadOnlyList<BlogPostBrief> posts, int count)
    {
        if (posts.Count == 0)
        {
            return [];
        }

        var items = new List<DiscoveryPostCard>();

        foreach (var post in TakeRandom(posts, count))
        {
            items.Add(new DiscoveryPostCard(
                "随机发现",
                post.Title ?? "未命名文章",
                post.Description ?? "换一篇看看，也许会撞上正想看的主题。",
                ConstantUtil.GetPostUrl(post),
                (post.Lastmod ?? post.Date)?.ToString("yyyy-MM-dd") ?? "文章"));
        }

        return items;
    }

    private static T? PickRandom<T>(IReadOnlyList<T> items)
    {
        return items.Count == 0
            ? default
            : items[Random.Shared.Next(items.Count)];
    }

    private static List<T> TakeRandom<T>(IReadOnlyList<T> source, int count)
    {
        if (source.Count == 0 || count <= 0)
        {
            return [];
        }

        var items = source.ToList();
        var take = Math.Min(count, items.Count);

        // 只洗牌前 count 个位置，足够拿到无重复随机项，成本也比完整乱序更低。
        for (var index = 0; index < take; index++)
        {
            var swapIndex = Random.Shared.Next(index, items.Count);
            (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
        }

        return items.Take(take).ToList();
    }
}
