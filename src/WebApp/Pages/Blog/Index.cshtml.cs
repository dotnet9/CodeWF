using WebApp.Models;
using WebApp.Services;
using WebApp.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Blog;

public class IndexModel : PageModel
{
    private readonly AppService _appService;
    private readonly I18nService _i18nService;

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

    public IndexModel(AppService appService, I18nService i18nService)
    {
        _appService = appService;
        _i18nService = i18nService;
    }

    public async Task OnGetAsync(int pageIndex = 1)
    {
        PageIndex = pageIndex > 0 ? pageIndex : 1;
        var pageData = await _appService.GetPagedBlogPostsAsync(PageIndex, PageSize);
        Posts = pageData.Data;
        Total = pageData.Total;
        Categories = await _appService.GetAllCategoryItemsAsync() ?? [];
        Albums = await _appService.GetAllAlbumItemsAsync() ?? [];
        var categoryCounts = await _appService.GetCategoryPostCountsBySlugAsync();
        var albumCounts = await _appService.GetAlbumPostCountsBySlugAsync();

        var allPosts = await _appService.GetAllBlogPostBriefsAsync() ?? [];
        var latestPost = await _appService.LocalizeBlogPostBriefAsync(allPosts.FirstOrDefault());
        GettingStartedLinks = BuildGettingStartedLinks(latestPost, Categories, categoryCounts, Albums, albumCounts, _i18nService);
        SerialReadingLinks = BuildSerialReadingLinks(Albums, albumCounts, _i18nService);
        // “随机发现”刻意排除当前列表页已展示的文章，降低同屏重复感。
        var randomSource = TakeRandom(
            allPosts.Where(post =>
                Posts.All(listed => !string.Equals(listed.Slug, post.Slug, StringComparison.OrdinalIgnoreCase)))
            .ToList(),
            3);
        RandomPosts = BuildDiscoveryPosts(
            await _appService.LocalizeBlogPostBriefsAsync(randomSource),
            _i18nService);
    }

    private static List<DiscoveryLinkCard> BuildGettingStartedLinks(
        BlogPostBrief? latestPost,
        IReadOnlyList<CategoryItem> categories,
        IReadOnlyDictionary<string, int> categoryCounts,
        IReadOnlyList<AlbumItem> albums,
        IReadOnlyDictionary<string, int> albumCounts,
        I18nService i18nService)
    {
        var links = new List<DiscoveryLinkCard>();

        if (latestPost is not null)
        {
            links.Add(new DiscoveryLinkCard(
                i18nService.T("blog.start.latestEyebrow", "先看更新"),
                i18nService.T("blog.start.latestTitle", "从最新文章进入"),
                i18nService.Text(latestPost.Title ?? "最近更新"),
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
                Count = GetPostCount(categoryCounts, item.Slug)
            })
            .OrderByDescending(item => item.Count)
            .FirstOrDefault();

        if (topCategory != null)
        {
            links.Add(new DiscoveryLinkCard(
                i18nService.T("blog.start.categoryEyebrow", "按主题看"),
                i18nService.Format("blog.start.categoryTitle", "先逛 {0}", i18nService.Text(topCategory.Item.Name)),
                i18nService.Format("blog.start.categoryDescription", "{0} 篇文章，适合按技术方向快速筛选", topCategory.Count),
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
                Count = GetPostCount(albumCounts, item.Slug)
            })
            .Where(static item => item.Count > 0)
            .ToList(), 1)
            .FirstOrDefault();

        if (topAlbum != null)
        {
            links.Add(new DiscoveryLinkCard(
                i18nService.T("blog.start.albumEyebrow", "连续阅读"),
                i18nService.Format("blog.start.albumTitle", "跟着专题读 {0}", i18nService.Text(topAlbum.Item.Name)),
                i18nService.Format("blog.start.albumDescription", "{0} 篇文章，更适合系统连读", topAlbum.Count),
                ConstantUtil.GetAlbumUrl(topAlbum.Item.Slug!)));
        }

        return links;
    }

    private static List<DiscoveryLinkCard> BuildSerialReadingLinks(
        IReadOnlyList<AlbumItem> albums,
        IReadOnlyDictionary<string, int> albumCounts,
        I18nService i18nService)
    {
        var candidates = albums
            .Where(item =>
                !string.Equals(item.Slug, ConstantUtil.DefaultCategory, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(item.Name)
                && !string.IsNullOrWhiteSpace(item.Slug))
            .Select(item => new
            {
                Item = item,
                Count = GetPostCount(albumCounts, item.Slug)
            })
            .Where(item => item.Count > 0)
            .ToList();

        return TakeRandom(candidates, 4)
            .Select(item => new DiscoveryLinkCard(
                i18nService.T("blog.serial.eyebrow", "专题连读"),
                i18nService.Text(item.Item.Name!),
                i18nService.Format("blog.serial.description", "{0} 篇文章，适合连续阅读", item.Count),
                ConstantUtil.GetAlbumUrl(item.Item.Slug!)))
            .ToList();
    }

    private static List<DiscoveryPostCard> BuildDiscoveryPosts(
        IReadOnlyList<BlogPostBrief> posts,
        I18nService i18nService)
    {
        if (posts.Count == 0)
        {
            return [];
        }

        var items = new List<DiscoveryPostCard>();

        foreach (var post in posts)
        {
            var label = post.Categories?.FirstOrDefault()
                ?? post.Albums?.FirstOrDefault()
                ?? i18nService.T("blog.random.fallbackLabel", "随机发现");

            items.Add(new DiscoveryPostCard(
                i18nService.T("blog.random.eyebrow", "随机发现"),
                i18nService.Text(post.Title ?? "未命名文章"),
                i18nService.Text(post.Description ?? "换个方向看看，也许正好碰到你感兴趣的主题。"),
                ConstantUtil.GetPostUrl(post),
                i18nService.Text(label)));
        }

        return items;
    }

    private static int GetPostCount(IReadOnlyDictionary<string, int> counts, string? slug) =>
        !string.IsNullOrWhiteSpace(slug) && counts.TryGetValue(slug, out var count) ? count : 0;

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
