using WebApp.Models;
using WebApp.Services;
using WebApp.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public sealed record HomeBrowseItem(string Name, string Slug, string? Memo, int PostCount);

public class IndexModel : PageModel
{
    private readonly AppService _appService;
    private readonly I18nService _i18nService;
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

    public IndexModel(AppService appService, I18nService i18nService)
    {
        _appService = appService;
        _i18nService = i18nService;
    }

    public async Task OnGetAsync()
    {
        var allPosts = await _appService.GetAllBlogPostBriefsAsync() ?? [];
        LatestPosts = await _appService.LocalizeBlogPostBriefsAsync(allPosts.Take(4));
        // Hero 区域使用随机文章，避免首页长期只被同一组内容占据。
        HeroPosts = await _appService.LocalizeBlogPostBriefsAsync(TakeRandom(allPosts, 2));
        Posts = await _appService.GetBannerPostAsync(6) ?? [];
        if (Posts.Count == 0)
        {
            Posts = await _appService.LocalizeBlogPostBriefsAsync(allPosts.Take(6));
        }

        Albums = await _appService.GetAllAlbumItemsAsync() ?? [];
        Categories = await _appService.GetAllCategoryItemsAsync() ?? [];
        var albumCounts = await _appService.GetAlbumPostCountsBySlugAsync();
        var categoryCounts = await _appService.GetCategoryPostCountsBySlugAsync();
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
                GetPostCount(albumCounts, item.Slug)))
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
                GetPostCount(categoryCounts, item.Slug)))
            .OrderByDescending(item => item.PostCount)
            .ThenBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .Take(FeaturedCategoryLimit)
            .ToList();

        var latestPost = await _appService.LocalizeBlogPostBriefAsync(allPosts.FirstOrDefault());
        GettingStartedLinks = BuildGettingStartedLinks(latestPost, FeaturedCategories, FeaturedAlbums, _i18nService);
        var discoverySource = TakeRandom(
            allPosts.Where(post =>
                Posts.All(featured => !string.Equals(featured.Slug, post.Slug, StringComparison.OrdinalIgnoreCase)))
            .ToList(),
            3);
        DiscoveryPosts = BuildDiscoveryPosts(
            await _appService.LocalizeBlogPostBriefsAsync(discoverySource),
            _i18nService);

        TotalDocNodes = await _appService.GetDefaultDocNodeCountAsync();
        TotalToolEntries = await _appService.GetDefaultToolEntryCountAsync();
    }

    private static List<DiscoveryLinkCard> BuildGettingStartedLinks(
        BlogPostBrief? latestPost,
        IReadOnlyList<HomeBrowseItem> categories,
        IReadOnlyList<HomeBrowseItem> albums,
        I18nService i18nService)
    {
        var links = new List<DiscoveryLinkCard>();

        if (latestPost is not null)
        {
            links.Add(new DiscoveryLinkCard(
                i18nService.T("home.start.latestEyebrow", "从这里开始"),
                i18nService.T("home.start.latestTitle", "先看最新更新"),
                i18nService.Text(latestPost.Title ?? "最近更新"),
                ConstantUtil.GetPostUrl(latestPost)));
        }

        if (categories.FirstOrDefault() is { } category)
        {
            links.Add(new DiscoveryLinkCard(
                i18nService.T("home.start.categoryEyebrow", "内容地图"),
                i18nService.Format("home.start.categoryTitle", "先逛 {0}", i18nService.Text(category.Name)),
                i18nService.Format("home.start.categoryDescription", "{0} 篇文章，适合快速熟悉站内内容结构", category.PostCount),
                ConstantUtil.GetCategoryUrl(category.Slug)));
        }

        if (PickRandom(albums.Where(static item => item.PostCount > 0).ToList()) is { } album)
        {
            links.Add(new DiscoveryLinkCard(
                i18nService.T("home.start.albumEyebrow", "连续阅读"),
                i18nService.Format("home.start.albumTitle", "跟着专题读 {0}", i18nService.Text(album.Name)),
                i18nService.Format("home.start.albumDescription", "{0} 篇文章，适合按主题连续阅读", album.PostCount),
                ConstantUtil.GetAlbumUrl(album.Slug)));
        }

        links.Add(new DiscoveryLinkCard(
            i18nService.T("home.start.projectEyebrow", "项目索引"),
            i18nService.T("home.start.projectTitle", "看看开源项目"),
            i18nService.T("home.start.projectDescription", "这里整理了开源项目、NuGet 包和对应的使用说明。"),
            ConstantUtil.GetProjectDirectoryUrl()));

        return links.Take(4).ToList();
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
            items.Add(new DiscoveryPostCard(
                i18nService.T("home.discovery.eyebrow", "随机发现"),
                i18nService.Text(post.Title ?? "未命名文章"),
                i18nService.Text(post.Description ?? "换一篇看看，也许会撞上正想看的主题。"),
                ConstantUtil.GetPostUrl(post),
                i18nService.FormatDate(post.Lastmod ?? post.Date) is { Length: > 0 } dateLabel
                    ? dateLabel
                    : i18nService.T("search.kind.post", "文章")));
        }

        return items;
    }

    private static int GetPostCount(IReadOnlyDictionary<string, int> counts, string? slug) =>
        !string.IsNullOrWhiteSpace(slug) && counts.TryGetValue(slug, out var count) ? count : 0;

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
