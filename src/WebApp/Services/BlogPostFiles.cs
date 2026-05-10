using System.Text;
using WebApp.Extensions;
using WebApp.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WebApp.Services;

internal static class BlogPostFiles
{
    public const string MetadataExtension = ".yml";

    private static readonly IDeserializer FrontMatterDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public static string GetMetadataPath(string markdownFilePath) =>
        Path.ChangeExtension(markdownFilePath, MetadataExtension);

    public static async Task<BlogPost> ReadAsync(string markdownFilePath, bool renderContent)
    {
        var markdown = await File.ReadAllTextAsync(markdownFilePath, Encoding.UTF8);
        var metadataPath = GetMetadataPath(markdownFilePath);

        string frontMatterText;
        string markdownContent;
        if (File.Exists(metadataPath))
        {
            // 新格式约定：a.md 只放正文，a.yml 放文章元数据；旧文章如果仍带 Front Matter，会在这里剥离掉。
            frontMatterText = await File.ReadAllTextAsync(metadataPath, Encoding.UTF8);
            markdownContent = StripInlineFrontMatter(markdown).Trim();
        }
        else if (TrySplitInlineFrontMatter(markdown, out frontMatterText, out markdownContent))
        {
            // 兼容旧格式，便于读取历史翻译产物和未迁移的临时内容。
            markdownContent = markdownContent.Trim();
        }
        else
        {
            throw new InvalidOperationException("Invalid markdown format. No ending '---' found for Front Matter.");
        }

        BlogPost blogPost;
        try
        {
            blogPost = FrontMatterDeserializer.Deserialize<BlogPost>(frontMatterText) ?? new BlogPost();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize blog post front matter: {metadataPath}. {ex.Message}");
            blogPost = new BlogPost();
        }

        if (renderContent)
        {
            blogPost.Content = markdownContent;
            blogPost.HtmlContent = markdownContent.ToHtml();
        }

        blogPost.SourcePath = markdownFilePath;
        return blogPost;
    }

    public static async Task<BlogPost> ReadMetadataAsync(string metadataPath)
    {
        var frontMatterText = await File.ReadAllTextAsync(metadataPath, Encoding.UTF8);
        return FrontMatterDeserializer.Deserialize<BlogPost>(frontMatterText) ?? new BlogPost();
    }

    public static async Task<string> ReadBodyAsync(string markdownFilePath)
    {
        var markdown = await File.ReadAllTextAsync(markdownFilePath, Encoding.UTF8);
        return StripInlineFrontMatter(markdown).Trim();
    }

    public static string SerializeMetadata(BlogPost post)
    {
        var builder = new StringBuilder();

        AppendString(builder, "title", post.Title);
        AppendString(builder, "slug", post.Slug);
        AppendString(builder, "description", post.Description);
        AppendDate(builder, "date", post.Date);
        AppendDate(builder, "lastmod", post.Lastmod);
        AppendString(builder, "cover", post.Cover);
        AppendBool(builder, "banner", post.Banner);
        AppendStringList(builder, "categories", post.Categories);
        AppendStringList(builder, "albums", post.Albums);
        AppendStringList(builder, "tags", post.Tags);
        AppendString(builder, "author", post.Author);
        AppendString(builder, "lastModifyUser", post.LastModifyUser);
        AppendString(builder, "originalTitle", post.OriginalTitle);
        AppendString(builder, "originalLink", post.OriginalLink);
        AppendString(builder, "copyright", post.Copyright);
        AppendBool(builder, "draft", post.Draft);

        return builder.ToString().TrimEnd() + Environment.NewLine;
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

    private static void AppendString(StringBuilder builder, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.Append(key).Append(": ").AppendLine(Quote(value.Trim()));
    }

    private static void AppendDate(StringBuilder builder, string key, DateTime? value)
    {
        if (value.HasValue)
        {
            builder.Append(key).Append(": ").AppendLine(value.Value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    private static void AppendBool(StringBuilder builder, string key, bool value) =>
        builder.Append(key).Append(": ").AppendLine(value ? "true" : "false");

    private static void AppendStringList(StringBuilder builder, string key, IReadOnlyCollection<string>? values)
    {
        var filteredValues = values?
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .ToList();
        if (filteredValues is not { Count: > 0 })
        {
            return;
        }

        builder.AppendLine($"{key}:");
        foreach (var value in filteredValues)
        {
            builder.Append("  - ").AppendLine(Quote(value));
        }
    }

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\r", string.Empty, StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal)}\"";

    private static string NormalizeLineEndings(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}
