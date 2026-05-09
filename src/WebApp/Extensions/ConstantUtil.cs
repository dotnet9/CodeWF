using WebApp.Models;
using System.Text;
using WebApp.Services;

namespace WebApp.Extensions;

public static class ConstantUtil
{
    public const string DefaultCategory = "default";
    private static string LocalPath(string path) => RequestLanguage.LocalizePath(path);

    public static string GetBlogUrl(int? pageIndex = null) =>
        pageIndex.HasValue && pageIndex.Value > 1
            ? LocalPath($"/post?pageIndex={pageIndex.Value}")
            : LocalPath("/post");
    public static string GetCategoryDirectoryUrl() => LocalPath("/cat");
    public static string GetCategoryUrl(string slug) => LocalPath($"/cat/{slug}");
    public static string GetAlbumDirectoryUrl() => LocalPath("/album");
    public static string GetAlbumUrl(string slug) => LocalPath($"/album/{slug}");
    public static string GetTagDirectoryUrl() => LocalPath("/tag");
    public static string GetTagUrl(string tag) =>
        string.IsNullOrWhiteSpace(NormalizeTagName(tag))
            ? GetTagDirectoryUrl()
            : LocalPath($"/tag/{Uri.EscapeDataString(NormalizeTagName(tag))}");
    // 标签名在 URL、文件和页面展示之间来回转换，统一做 Trim + FormC 归一化能减少同义重复。
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
    public static string GetPostUrl(BlogPostBrief post) => LocalPath($"/{post.Date?.Year:D4}/{post.Date?.Month:D2}/{post.Slug}");
    public static string GetProjectDirectoryUrl() => LocalPath("/project");
    public static string GetProjectUrl(string? slug) => LocalPath($"/project/{slug}");
    public static string GetSearchUrl(string? query = null, int? pageIndex = null)
    {
        // 搜索页统一收敛到 /s，既能保持链接简短，也方便在布局层做 robots 特殊处理。
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
            ? LocalPath($"/s?{string.Join("&", parameters)}")
            : LocalPath("/s");
    }

    public static string GetDocUrl(string? slug) => GetProjectUrl(slug);
    public static string GetToolUrl(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return LocalPath("/tool");
        }

        var normalizedSlug = slug.Trim();
        if (normalizedSlug.StartsWith('/'))
        {
            return LocalPath(normalizedSlug);
        }

        return normalizedSlug.ToLowerInvariant() switch
        {
            "nuoche" => LocalPath("/nuoche"),
            "ico" or "icon" or "icon-converter" => LocalPath("/icon"),
            "timestamp" => LocalPath("/timestamp"),
            "fuli" or "compound-interest-calculator" => LocalPath("/fuli"),
            "slugify-string-translator" => LocalPath("/slugify-string"),
            _ => LocalPath($"/tool/{normalizedSlug}")
        };
    }

    public static string GetPostGithubPath(string? githubRepository, BlogPost? post) =>
        $"{githubRepository}/blob/main/{post?.Date?.Year:D4}/{post?.Date?.Month:D2}/{post?.Slug}.md";
}
