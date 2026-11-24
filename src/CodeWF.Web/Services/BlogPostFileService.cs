using System.Text;
using System.Text.RegularExpressions;
using CodeWF.Web.Models;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CodeWF.Web.Services;

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
        // 不用 UseYamlFrontMatter：其解析器在首行 --- 无闭合行时会把整篇吞进
        // front matter 块渲染成空，比漏切更狠；front matter 裁决统一走
        // TrySplitInlineFrontMatter（失败模式全部是"按正文渲染"，从不吃内容）
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
        blogPost.Cover = MakeCoverUrlAbsolute(blogPost.Cover, assetBaseUrl);
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
        var html = Markdown.ToHtml(StripInlineFrontMatter(markdown), MarkdownPipeline);
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
        var lines = normalized.Split('\n');

        // 开分隔行容忍尾随空白；不严格匹配时整篇按正文处理（正文可能以分隔线开头）。
        if (lines[0].Trim() != "---")
        {
            return false;
        }

        var close = -1;
        for (var i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "---")
            {
                close = i;
                break;
            }
        }

        if (close < 0)
        {
            return false;
        }

        var candidate = lines[1..close];
        // 候选块里必须至少有一行 "key:"，否则视为正文里夹着的两根分隔线：
        // 首行 --- 不代表它就是元数据开头，直认会把中间的正文整块吞进元数据再丢弃。
        if (!candidate.Any(IsYamlKeyLine))
        {
            return false;
        }

        frontMatter = string.Join('\n', candidate).Trim();
        body = string.Join('\n', lines[(close + 1)..]).TrimStart('\n');
        return true;
    }

    private static bool IsYamlKeyLine(string line) =>
        YamlKeyLineRegex().IsMatch(line.TrimStart());

    [GeneratedRegex("^[A-Za-z][\\w-]*\\s*:")]
    private static partial Regex YamlKeyLineRegex();

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

    /// <summary>
    /// front matter 的 cover 允许写资产库相对路径（如 2026/11/x.png），
    /// 统一拼接 CDN 基址——图片域名变更时只改 AssetBaseUrl 配置，文章不用动。
    /// </summary>
    private static string? MakeCoverUrlAbsolute(string? cover, string assetBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(cover)
            || string.IsNullOrWhiteSpace(assetBaseUrl)
            || IsAbsoluteOrSpecialUrl(cover))
        {
            return cover;
        }

        return $"{assetBaseUrl.TrimEnd('/')}/{cover.TrimStart('/')}";
    }

    [GeneratedRegex("(?<attr>src|href)=(?<quote>[\"'])(?<url>.*?)(\\k<quote>)", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlUrlAttributeRegex();
}
