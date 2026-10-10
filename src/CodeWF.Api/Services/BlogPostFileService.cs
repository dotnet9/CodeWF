using System.Text;
using System.Text.RegularExpressions;
using CodeWF.Api.Models;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CodeWF.Api.Services;

public sealed partial class BlogPostFileService
{
    public const string MetadataExtension = ".yml";

    private static readonly IDeserializer FrontMatterDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
        .UseAutoLinks()
        .UseBootstrap()
        .Build();

    /// <summary>
    /// Reads a blog post from disk. File reads are intentionally synchronous:
    /// sequential async file reads across hundreds of posts create long async task
    /// chains that have been observed to trigger StackOverflowException inside the
    /// .NET runtime's async file I/O machinery (dotnet/runtime#113189 pattern).
    /// </summary>
    public BlogPost Read(string markdownFilePath, string assetsRoot, string assetBaseUrl, string? metadataPath = null)
    {
        var markdown = File.ReadAllText(markdownFilePath, Encoding.UTF8);
        metadataPath ??= GetMetadataPath(markdownFilePath);

        string frontMatterText;
        string markdownContent;
        if (File.Exists(metadataPath))
        {
            frontMatterText = File.ReadAllText(metadataPath, Encoding.UTF8);
            markdownContent = StripInlineFrontMatter(markdown).Trim();
        }
        else if (TrySplitInlineFrontMatter(markdown, out frontMatterText, out markdownContent))
        {
            markdownContent = markdownContent.Trim();
        }
        else
        {
            frontMatterText = string.Empty;
            markdownContent = markdown.Trim();
        }

        BlogPost blogPost;
        try
        {
            blogPost = FrontMatterDeserializer.Deserialize<BlogPost>(frontMatterText) ?? new BlogPost();
        }
        catch
        {
            blogPost = new BlogPost();
        }

        blogPost.Content = markdownContent;
        blogPost.HtmlContent = MakeContentUrlsAbsolute(
            Markdown.ToHtml(markdownContent, MarkdownPipeline),
            markdownFilePath,
            assetsRoot,
            assetBaseUrl);
        blogPost.SourcePath = markdownFilePath;

        if (string.IsNullOrWhiteSpace(blogPost.Slug))
        {
            blogPost.Slug = Path.GetFileNameWithoutExtension(markdownFilePath);
        }

        ApplyDateFromPath(blogPost, markdownFilePath, assetsRoot);
        blogPost.Url = BuildPostUrl(blogPost);
        return blogPost;
    }

    public static string BuildPostUrl(BlogPostBrief post)
    {
        var date = post.Date ?? DateTime.Today;
        return $"/{date.Year:D4}/{date.Month:D2}/{post.Slug}";
    }

    public static string GetMetadataPath(string markdownFilePath) =>
        Path.ChangeExtension(markdownFilePath, MetadataExtension);

    public string RenderMarkdown(string markdown, string? sourcePath = null, string? assetsRoot = null, string? assetBaseUrl = null)
    {
        var html = Markdown.ToHtml(markdown, MarkdownPipeline);
        return sourcePath is null || assetsRoot is null || assetBaseUrl is null
            ? html
            : MakeContentUrlsAbsolute(html, sourcePath, assetsRoot, assetBaseUrl);
    }

    private static string StripInlineFrontMatter(string markdown) =>
        TrySplitInlineFrontMatter(markdown, out _, out var body) ? body : markdown;

    private static bool TrySplitInlineFrontMatter(string markdown, out string frontMatter, out string body)
    {
        frontMatter = string.Empty;
        body = markdown;

        var normalized = NormalizeLineEndings(markdown);
        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            return false;
        }

        var endMarker = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        var markerLength = "\n---\n".Length;
        if (endMarker < 0 && normalized.EndsWith("\n---", StringComparison.Ordinal))
        {
            endMarker = normalized.Length - "\n---".Length;
            markerLength = "\n---".Length;
        }

        if (endMarker < 0)
        {
            return false;
        }

        frontMatter = normalized[4..endMarker].Trim();
        body = normalized[(endMarker + markerLength)..].TrimStart('\n');
        return true;
    }

    private static string NormalizeLineEndings(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');


    private static void ApplyDateFromPath(BlogPostBrief post, string markdownFilePath, string assetsRoot)
    {
        var relative = Path.GetRelativePath(assetsRoot, markdownFilePath);
        var parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (parts.Length >= 3
            && int.TryParse(parts[0], out var year)
            && int.TryParse(parts[1], out var month))
        {
            post.Year = year;
            post.Month = month;
            post.Date ??= new DateTime(year, month, 1);
        }
    }

    private static string MakeContentUrlsAbsolute(string html, string markdownFilePath, string assetsRoot, string assetBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(assetBaseUrl))
        {
            return html;
        }

        var relativeFile = Path.GetRelativePath(assetsRoot, markdownFilePath).Replace('\\', '/');
        var relativeDirectory = Path.GetDirectoryName(relativeFile)?.Replace('\\', '/') ?? string.Empty;
        var baseUrl = assetBaseUrl.TrimEnd('/');

        return HtmlUrlAttributeRegex().Replace(html, match =>
        {
            var attribute = match.Groups["attr"].Value;
            var quote = match.Groups["quote"].Value;
            var url = match.Groups["url"].Value;
            if (IsAbsoluteOrSpecialUrl(url))
            {
                return match.Value;
            }

            var path = string.IsNullOrWhiteSpace(relativeDirectory)
                ? url.TrimStart('/')
                : $"{relativeDirectory}/{url.TrimStart('/')}";
            return $"{attribute}={quote}{baseUrl}/{path}{quote}";
        });
    }

    private static bool IsAbsoluteOrSpecialUrl(string url) =>
        url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("//", StringComparison.Ordinal)
        || url.StartsWith("/", StringComparison.Ordinal)
        || url.StartsWith("#", StringComparison.Ordinal)
        || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex("(?<attr>src|href)=(?<quote>[\"'])(?<url>.*?)(\\k<quote>)", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlUrlAttributeRegex();
}
