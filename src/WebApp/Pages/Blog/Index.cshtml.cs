using WebApp.Models;
using WebApp.Services;
using WebApp.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Blog;

public class IndexModel : PageModel
{
    private readonly AppService _appService;

    public List<BlogPostBrief> Posts { get; private set; } = [];
    public List<CategoryItem> Categories { get; private set; } = [];
    public List<AlbumItem> Albums { get; private set; } = [];
    public List<DiscoveryLinkCard> GettingStartedLinks { get; private set; } = [];
    public List<DiscoveryLinkCard> SerialReadingLinks { get; private set; } = [];
    public List<DiscoveryPostCard> RandomPosts { get; private set; } = [];
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
        Albums = await _appService.GetAllAlbumItemsAsync() ?? [];

        var allPosts = await _appService.GetAllBlogPostBriefsAsync() ?? [];
        GettingStartedLinks = BuildGettingStartedLinks(allPosts, Categories, Albums);
        SerialReadingLinks = BuildSerialReadingLinks(allPosts, Albums);
        // “随机发现”刻意排除当前列表页已展示的文章，降低同屏重复感。
        RandomPosts = BuildDiscoveryPosts(
            allPosts.Where(post =>
                Posts.All(listed => !string.Equals(listed.Slug, post.Slug, StringComparison.OrdinalIgnoreCase)))
            .ToList(),
            3);
    }

    private static List<DiscoveryLinkCard> BuildGettingStartedLinks(
        IReadOnlyList<BlogPostBrief> allPosts,
        IReadOnlyList<CategoryItem> categories,
        IReadOnlyList<AlbumItem> albums)
    {
        var links = new List<DiscoveryLinkCard>();

        if (allPosts.FirstOrDefault() is { } latestPost)
        {
            links.Add(new DiscoveryLinkCard(
                "先看更新",
                "从最新文章进入",
                latestPost.Title ?? "最近更新",
                ConstantUtil.GetPostUrl(latestPost)));
        }

        var topCategory = categories
            .Where(item =>
                !string.Equals(item.Slug, ConstantUtil.DefaultCategory, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(item.Name)
                && !string.IsNullOrWhiteSpace(item.Slug))
            .Select(item => new
            {
                Item = item,
                Count = allPosts.Count(post => post.Categories?.Contains(item.Name, StringComparer.OrdinalIgnoreCase) == true)
            })
            .OrderByDescending(item => item.Count)
            .FirstOrDefault();

        if (topCategory != null)
        {
            links.Add(new DiscoveryLinkCard(
                "按主题看",
                $"先逛 {topCategory.Item.Name}",
                $"{topCategory.Count} 篇文章，适合按技术方向快速筛选",
                ConstantUtil.GetCategoryUrl(topCategory.Item.Slug!)));
        }

        var topAlbum = TakeRandom(albums
            .Where(item =>
                !string.Equals(item.Slug, ConstantUtil.DefaultCategory, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(item.Name)
                && !string.IsNullOrWhiteSpace(item.Slug))
            .Select(item => new
            {
                Item = item,
                Count = allPosts.Count(post => post.Albums?.Contains(item.Name, StringComparer.OrdinalIgnoreCase) == true)
            })
            .Where(static item => item.Count > 0)
            .ToList(), 1)
            .FirstOrDefault();

        if (topAlbum != null)
        {
            links.Add(new DiscoveryLinkCard(
                "连续阅读",
                $"跟着专题读 {topAlbum.Item.Name}",
                $"{topAlbum.Count} 篇文章，更适合系统连读",
                ConstantUtil.GetAlbumUrl(topAlbum.Item.Slug!)));
        }

        return links;
    }

    private static List<DiscoveryLinkCard> BuildSerialReadingLinks(
        IReadOnlyList<BlogPostBrief> allPosts,
        IReadOnlyList<AlbumItem> albums)
    {
        var candidates = albums
            .Where(item =>
                !string.Equals(item.Slug, ConstantUtil.DefaultCategory, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(item.Name)
                && !string.IsNullOrWhiteSpace(item.Slug))
            .Select(item => new
            {
                Item = item,
                Count = allPosts.Count(post => post.Albums?.Contains(item.Name, StringComparer.OrdinalIgnoreCase) == true)
            })
            .Where(item => item.Count > 0)
            .ToList();

        return TakeRandom(candidates, 4)
            .Select(item => new DiscoveryLinkCard(
                "专题连读",
                item.Item.Name!,
                $"{item.Count} 篇文章，适合连续阅读",
                ConstantUtil.GetAlbumUrl(item.Item.Slug!)))
            .ToList();
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
            var label = post.Categories?.FirstOrDefault()
                ?? post.Albums?.FirstOrDefault()
                ?? "随机发现";

            items.Add(new DiscoveryPostCard(
                "随机发现",
                post.Title ?? "未命名文章",
                post.Description ?? "换个方向看看，也许正好碰到你感兴趣的主题。",
                ConstantUtil.GetPostUrl(post),
                label));
        }

        return items;
    }

    private static List<T> TakeRandom<T>(IReadOnlyList<T> source, int count)
    {
        if (source.Count == 0 || count <= 0)
        {
            return [];
        }

        var items = source.ToList();
        var take = Math.Min(count, items.Count);

        // 使用局部洗牌而不是 OrderBy(Random)，可读性和性能都更可控。
        for (var index = 0; index < take; index++)
        {
            var swapIndex = Random.Shared.Next(index, items.Count);
            (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
        }

        return items.Take(take).ToList();
    }
}
