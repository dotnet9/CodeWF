using WebApp.Models;
using System.Text;

namespace WebApp.Extensions;

public static class ConstantUtil
{
    public const string DefaultCategory = "default";
    public static string GetBlogUrl(int? pageIndex = null) =>
        pageIndex.HasValue && pageIndex.Value > 1
            ? $"/post?pageIndex={pageIndex.Value}"
            : "/post";
    public static string GetCategoryDirectoryUrl() => "/cat";
    public static string GetCategoryUrl(string slug) => $"/cat/{slug}";
    public static string GetAlbumDirectoryUrl() => "/album";
    public static string GetAlbumUrl(string slug) => $"/album/{slug}";
    public static string GetTagDirectoryUrl() => "/tag";
    public static string GetTagUrl(string tag) =>
        string.IsNullOrWhiteSpace(NormalizeTagName(tag))
            ? GetTagDirectoryUrl()
            : $"/tag/{Uri.EscapeDataString(NormalizeTagName(tag))}";
    public static string NormalizeTagName(string? tag) =>
        string.IsNullOrWhiteSpace(tag)
            ? string.Empty
            : tag.Trim().Normalize(NormalizationForm.FormC);
    public static string DecodeTagSlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return string.Empty;
        }

        var trimmed = slug.Trim();
        try
        {
            return NormalizeTagName(Uri.UnescapeDataString(trimmed));
        }
        catch (UriFormatException)
        {
            return NormalizeTagName(trimmed);
        }
    }
    public static string GetPostUrl(BlogPostBrief post) => $"/{post.Date?.Year:D4}/{post.Date?.Month:D2}/{post.Slug}";
    public static string GetProjectDirectoryUrl() => "/project";
    public static string GetProjectUrl(string? slug) => $"/project/{slug}";
    public static string GetSearchUrl(string? query = null, int? pageIndex = null)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(query))
        {
            parameters.Add($"q={Uri.EscapeDataString(query)}");
        }

        if (pageIndex.HasValue && pageIndex.Value > 1)
        {
            parameters.Add($"pageIndex={pageIndex.Value}");
        }

        return parameters.Count > 0
            ? $"/s?{string.Join("&", parameters)}"
            : "/s";
    }

    public static string GetDocUrl(string? slug) => GetProjectUrl(slug);
    public static string GetToolUrl(string? slug) => slug switch
    {
        "slugify-string" => "/slugify-string",
        "timestamp" => "/timestamp",
        "ico" => "/icon",
        "nuoche" => "/nuoche",
        "fuli" => "/fuli",
        null or "" => "/tool",
        _ => $"/tool/{slug}"
    };

    public static string GetPostGithubPath(string? githubRepository, BlogPost? post) =>
        $"{githubRepository}/blob/main/{post?.Date?.Year:D4}/{post?.Date?.Month:D2}/{post?.Slug}.md";
}
