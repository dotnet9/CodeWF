using System.Net;
using System.Text.RegularExpressions;
using WebApp.Extensions;
using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Blog.Post;

public sealed record ArticleTopicLink(string Label, string Url, string IconClass);
public sealed record ArticleExploreLink(string Title, string Url, string IconClass, string Description);
public sealed record RelatedPostCard(string Title, string Url, string? Description, string ContextLabel, DateTime? UpdatedAt);

public class IndexModel : PageModel
{
    private readonly AppService _appService;
    private const int RelatedPostLimit = 4;

    public BlogPost? Post { get; set; }
    public BlogPost? PreviousPost { get; private set; }
    public BlogPost? NextPost { get; private set; }
    public IReadOnlyList<ArticleTopicLink> CategoryLinks { get; private set; } = [];
    public IReadOnlyList<ArticleTopicLink> AlbumLinks { get; private set; } = [];
    public IReadOnlyList<ArticleTopicLink> TagLinks { get; private set; } = [];
    public IReadOnlyList<ArticleExploreLink> ExploreLinks { get; private set; } = [];
    public IReadOnlyList<RelatedPostCard> RelatedPosts { get; private set; } = [];
    public int EstimatedReadingMinutes { get; private set; }
    public int HeadingCount { get; private set; }

    public IndexModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync(int year, int month, string slug)
    {
        Post = await _appService.GetPostBySlug(slug);
        if (Post == null)
        {
            return;
        }

        var posts = await _appService.GetAllBlogPostsAsync() ?? [];
        // 文章列表已按发布时间倒序排列，这里直接复用当前位置计算上一篇/下一篇。
        var currentIndex = posts.FindIndex(item =>
            string.Equals(item.Slug, Post.Slug, StringComparison.OrdinalIgnoreCase));

        if (currentIndex > 0)
        {
            PreviousPost = posts[currentIndex - 1];
        }

        if (currentIndex >= 0 && currentIndex < posts.Count - 1)
        {
            NextPost = posts[currentIndex + 1];
        }

        var categories = await _appService.GetAllCategoryItemsAsync() ?? [];
        // 分类/专题名称来自 Front Matter，先转成名称 -> slug 的索引，后面生成跳转链接更稳定。
        var categoryLookup = categories
            .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Slug))
            .GroupBy(item => item.Name!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Slug!, StringComparer.OrdinalIgnoreCase);

        var albums = await _appService.GetAllAlbumItemsAsync() ?? [];
        var albumLookup = albums
            .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Slug))
            .GroupBy(item => item.Name!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Slug!, StringComparer.OrdinalIgnoreCase);

        CategoryLinks = BuildTopicLinks(Post.Categories, categoryLookup, ConstantUtil.GetCategoryUrl, "fas fa-folder-open");
        AlbumLinks = BuildTopicLinks(Post.Albums, albumLookup, ConstantUtil.GetAlbumUrl, "fas fa-layer-group");
        TagLinks = BuildTagLinks(Post.Tags);
        ExploreLinks = BuildExploreLinks(CategoryLinks, AlbumLinks, TagLinks);
        RelatedPosts = BuildRelatedPosts(Post, posts, RelatedPostLimit);
        EstimatedReadingMinutes = EstimateReadingMinutes(Post.HtmlContent ?? Post.Content);
        HeadingCount = CountArticleHeadings(Post.HtmlContent ?? Post.Content);
    }

    private static IReadOnlyList<ArticleTopicLink> BuildTopicLinks(
        IEnumerable<string>? values,
        IReadOnlyDictionary<string, string> lookup,
        Func<string, string> urlFactory,
        string iconClass)
    {
        if (values == null)
        {
            return [];
        }

        return values
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Select(static item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(item => new ArticleTopicLink(
                item,
                urlFactory(lookup.TryGetValue(item, out var slug) ? slug : Uri.EscapeDataString(item)),
                iconClass))
            .ToList();
    }

    private static IReadOnlyList<ArticleTopicLink> BuildTagLinks(IEnumerable<string>? tags)
    {
        if (tags == null)
        {
            return [];
        }

        return tags
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Select(static item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(tag => new ArticleTopicLink(tag, ConstantUtil.GetTagUrl(tag), "fas fa-hashtag"))
            .ToList();
    }

    private static IReadOnlyList<ArticleExploreLink> BuildExploreLinks(
        IReadOnlyList<ArticleTopicLink> categories,
        IReadOnlyList<ArticleTopicLink> albums,
        IReadOnlyList<ArticleTopicLink> tags)
    {
        // 优先给当前文章补一条“继续阅读”路径，再兜底站内工具和项目入口，避免侧栏完全空掉。
        var links = new List<ArticleExploreLink>();

        if (categories.FirstOrDefault() is { } primaryCategory)
        {
            links.Add(new ArticleExploreLink("继续看同分类", primaryCategory.Url, primaryCategory.IconClass, primaryCategory.Label));
        }

        if (albums.FirstOrDefault() is { } primaryAlbum)
        {
            links.Add(new ArticleExploreLink("继续追这个专题", primaryAlbum.Url, primaryAlbum.IconClass, primaryAlbum.Label));
        }

        if (tags.FirstOrDefault() is { } primaryTag)
        {
            links.Add(new ArticleExploreLink("继续看同标签", primaryTag.Url, primaryTag.IconClass, primaryTag.Label));
        }

        links.Add(new ArticleExploreLink("顺手逛工具库", "/tool", "fas fa-screwdriver-wrench", "从文章跳到实用工具"));
        links.Add(new ArticleExploreLink("看看相关项目", ConstantUtil.GetProjectDirectoryUrl(), "fas fa-cube", "继续查看开源项目与用法说明"));

        return links
            .GroupBy(link => link.Url, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(4)
            .ToList();
    }

    private static IReadOnlyList<RelatedPostCard> BuildRelatedPosts(BlogPost currentPost, IEnumerable<BlogPost> posts, int limit)
    {
        var currentCategories = ToHashSet(currentPost.Categories);
        var currentAlbums = ToHashSet(currentPost.Albums);
        var currentTags = ToHashSet(currentPost.Tags);

        // 分类权重最高，其次专题、标签；分数不足时再用最近更新文章补齐卡片数量。
        var related = posts
            .Where(post =>
                !string.IsNullOrWhiteSpace(post.Slug)
                && !string.Equals(post.Slug, currentPost.Slug, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(post.Title))
            .Select(post =>
            {
                var reasons = new List<string>();
                var score = 0;

                var sharedCategories = CountOverlap(currentCategories, post.Categories);
                if (sharedCategories > 0)
                {
                    score += sharedCategories * 6;
                    reasons.Add("同分类");
                }

                var sharedAlbums = CountOverlap(currentAlbums, post.Albums);
                if (sharedAlbums > 0)
                {
                    score += sharedAlbums * 5;
                    reasons.Add("同专题");
                }

                var sharedTags = CountOverlap(currentTags, post.Tags);
                if (sharedTags > 0)
                {
                    score += sharedTags * 4;
                    reasons.Add("同标签");
                }

                return new
                {
                    Post = post,
                    Score = score,
                    ContextLabel = string.Join(" / ", reasons.Take(2))
                };
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Post.Lastmod ?? item.Post.Date ?? DateTime.MinValue)
            .Take(limit)
            .Select(item => new RelatedPostCard(
                item.Post.Title!,
                ConstantUtil.GetPostUrl(item.Post),
                item.Post.Description,
                item.ContextLabel,
                item.Post.Lastmod ?? item.Post.Date))
            .ToList();

        if (related.Count >= limit)
        {
            return related;
        }

        var existingUrls = related
            .Select(static item => item.Url)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var post in posts
                     .Where(post =>
                         !string.IsNullOrWhiteSpace(post.Slug)
                         && !string.Equals(post.Slug, currentPost.Slug, StringComparison.OrdinalIgnoreCase)
                         && !string.IsNullOrWhiteSpace(post.Title))
                     .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue))
        {
            var url = ConstantUtil.GetPostUrl(post);
            if (!existingUrls.Add(url))
            {
                continue;
            }

            related.Add(new RelatedPostCard(
                post.Title!,
                url,
                post.Description,
                "近期更新",
                post.Lastmod ?? post.Date));

            if (related.Count >= limit)
            {
                break;
            }
        }

        return related;
    }

    private static HashSet<string> ToHashSet(IEnumerable<string>? values) =>
        values?
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Select(static item => item.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
        ?? [];

    private static int CountOverlap(HashSet<string> currentValues, IEnumerable<string>? candidateValues)
    {
        if (currentValues.Count == 0 || candidateValues == null)
        {
            return 0;
        }

        return candidateValues.Count(value =>
            !string.IsNullOrWhiteSpace(value)
            && currentValues.Contains(value.Trim()));
    }

    private static int CountArticleHeadings(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return 0;
        }

        return Regex.Matches(content, "<h[2-4]\\b", RegexOptions.IgnoreCase).Count;
    }

    private static int EstimateReadingMinutes(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return 1;
        }

        // 中文按字数、英文按词数估算阅读量，混排文章会比单纯按空格切词更接近真实体感。
        var plainText = Regex.Replace(content, "<[^>]+>", " ");
        plainText = WebUtility.HtmlDecode(plainText);

        var cjkCharacters = Regex.Matches(plainText, "[\\u4e00-\\u9fff]").Count;
        var latinWords = Regex.Matches(plainText, "[A-Za-z0-9_]+").Count;
        var readingUnits = cjkCharacters + (latinWords * 2);

        return Math.Max(1, (int)Math.Ceiling(readingUnits / 320d));
    }
}
