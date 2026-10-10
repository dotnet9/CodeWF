namespace CodeWF.Api.Models;

public class BlogPostBrief
{
    public string? SourcePath { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? Lastmod { get; set; }
    public string? Copyright { get; set; }
    public bool Banner { get; set; }
    public string? Author { get; set; }
    public string? LastModifyUser { get; set; }
    public string? OriginalTitle { get; set; }
    public string? OriginalLink { get; set; }
    public bool Draft { get; set; }
    public string? Cover { get; set; }
    public List<string>? Albums { get; set; }
    public List<string>? Categories { get; set; }
    public List<string>? Tags { get; set; }
    public string? Url { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public string? ContextLabel { get; set; }
}

public sealed class BlogPost : BlogPostBrief
{
    public string? Content { get; set; }
    public string? HtmlContent { get; set; }
    public int EstimatedReadingMinutes { get; set; }
    public int HeadingCount { get; set; }
    public BlogPostBrief? PreviousPost { get; set; }
    public BlogPostBrief? NextPost { get; set; }
    public List<BlogPostBrief> RelatedPosts { get; set; } = [];
}

public sealed class TaxonomyItem
{
    public int Sort { get; set; }
    public string? Name { get; set; }
    public string? Memo { get; set; }
    public string? Slug { get; set; }
    public int PostCount { get; set; }
}

public sealed class TagItem
{
    public string Name { get; set; } = string.Empty;
    public int PostCount { get; set; }
}

public sealed class SearchBlockedKeywordGroup
{
    public int Sort { get; set; }
    public string? Name { get; set; }
    public string? Memo { get; set; }
    public List<string>? Keywords { get; set; }
}

public sealed class FriendLinkItem
{
    public int Index { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Link { get; set; }
    public string? Logo { get; set; }
}

public sealed class TimelineItem
{
    public DateTime? Time { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
}

public sealed record MarkdownPage(string? Markdown, string? HtmlContent);

public sealed class SiteSettingsRequest
{
    public string? AppTitle { get; set; }
    public string? Domain { get; set; }
    public string? Memo { get; set; }
    public string? Owner { get; set; }
    public string? OwnerDesc { get; set; }
    public string? Favicon { get; set; }
    public string? LocalAssetsDir { get; set; }
    public string? AssetBaseUrl { get; set; }
    public string? RemoteAssetsRepository { get; set; }
    public int StartYear { get; set; }
    public string? BaiAn { get; set; }
    public string? WeChatName { get; set; }
    public string? WeChatImg { get; set; }
    public string? DefaultCulture { get; set; }
    public List<string>? SupportedCultures { get; set; }
}

public sealed record SiteSettingsResult(bool Success, string Message, SiteInfo? Site = null);

public sealed record SiteInfo(
    string AppTitle,
    string Domain,
    string Memo,
    string Owner,
    string? OwnerDesc,
    string? Favicon,
    string LocalAssetsDir,
    string AssetBaseUrl,
    string? RemoteAssetsRepository,
    int StartYear,
    string DefaultCulture,
    IReadOnlyList<string> SupportedCultures,
    string? BaiAn,
    string? WeChatName,
    string? WeChatImg);

public sealed record HomePageData(
    SiteInfo Site,
    IReadOnlyList<BlogPostBrief> RecentPosts,
    IReadOnlyList<BlogPostBrief> BannerPosts,
    IReadOnlyList<TaxonomyItem> Categories,
    IReadOnlyList<TaxonomyItem> Albums,
    IReadOnlyDictionary<string, int> Counts);

public sealed record PagedResult<T>(
    int PageIndex,
    int PageSize,
    int Total,
    IReadOnlyList<T> Data);

public enum SearchResultKind
{
    Post = 2
}

public sealed class SearchResultItem
{
    public SearchResultKind Kind { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = "/";
    public string? Summary { get; init; }
    public string? MatchedSnippet { get; init; }
    public string? Context { get; init; }
    public string? Slug { get; init; }
    public string? SourcePath { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int Score { get; init; }
}

public sealed record SearchResultPageData(
    int PageIndex,
    int PageSize,
    int Total,
    IReadOnlyList<SearchResultItem> Data,
    int PostCount,
    bool IsBlocked = false,
    string? Notice = null);

