using CodeWF.Utils;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace CodeWF.Data;

public partial class Seed
{
    private const string Title = "title: ";
    private const string Banner = "banner: ";
    private const string Slug = "slug: ";
    private const string Description = "description: ";
    private const string Date = "date: ";
    private const string LastModifyDate = "lastmod: ";
    private const string Copyright = "copyright: ";
    private const string Author = "author: ";
    private const string OriginalTitle = "originaltitle: ";
    private const string OriginalLink = "originallink: ";
    private const string Draft = "draft: ";
    private const string Cover = "cover: ";
    private const string Categories = "categories: ";
    private const string Tags = "tags: ";


    private static async
        Task<(BlogPostSeedDto[]? BlogPosts, string[]?
            Tags)> GetPostAsync(string assetDir, List<CategoryEntity> cats)
    {
        List<string> allCategoryNames = cats.Select(cat => cat.DisplayName).ToList()!;
        HashSet<string> tagNames = new HashSet<string>();

        bool CheckCategoryExist(IReadOnlyCollection<string>? categoryNames)
        {
            if (categoryNames == null || !categoryNames.Any())
            {
                return true;
            }

            bool exist = categoryNames.All(albumName => allCategoryNames.Any(x => x == albumName));
            if (exist)
            {
                return true;
            }

            Debug.WriteLine($"文章头配置的分类未在分类列表中配置: {categoryNames.JoinAsString(",")}");
            return false;
        }


        void ReadTagName(string[]? data)
        {
            if (data == null || !data.Any())
            {
                return;
            }

            foreach (string s in data)
            {
                tagNames.Add(s);
            }
        }

        List<string> blogPostFiles = new List<string>();
        for (int i = 2019; i <= DateTime.Now.Year; i++)
        {
            blogPostFiles.AddRange(Directory.GetFiles(Path.Combine(assetDir, i.ToString()),
                "*.md",
                SearchOption.AllDirectories));
        }

        BlogPostSeedDto[] blogPosts = blogPostFiles.Select(Read).ToArray();
        foreach (BlogPostSeedDto blogPostOfMarkdown in blogPosts)
        {
            if (!CheckCategoryExist(blogPostOfMarkdown.Categories))
            {
                throw new ArgumentException(
                    $"The post category is not configured, please check. The post title is {blogPostOfMarkdown.Title}, category as {blogPostOfMarkdown.Categories?.JoinAsString(",")}.");
            }

            ReadTagName(blogPostOfMarkdown.Tags);
        }

        return (blogPosts, tagNames.ToArray());
    }


    private static BlogPostSeedDto Read(string markdownAbsolutePath)
    {
        BlogPostSeedDto blogPostOfMarkdown = new BlogPostSeedDto();
        string[] lines = File.ReadAllLines(markdownAbsolutePath);
        bool isReadStart = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("---"))
            {
                if (isReadStart == false)
                {
                    isReadStart = true;
                    continue;
                }

                int contentLineStartOfAllLines = i + 2;
                int contentLineLength = lines.Length - contentLineStartOfAllLines;
                string[] contentLines = new string[contentLineLength];
                Array.Copy(lines, contentLineStartOfAllLines, contentLines, 0, contentLineLength);
                blogPostOfMarkdown.Content = string.Join("\n", contentLines);
                break;
            }

            if (lines[i].StartsWith(Title))
            {
                blogPostOfMarkdown.Title = lines[i][Title.Length..];
            }
            else if (lines[i].StartsWith(Slug))
            {
                blogPostOfMarkdown.Slug = lines[i][Slug.Length..];
            }
            else if (lines[i].StartsWith(Description))
            {
                blogPostOfMarkdown.Description = lines[i][Description.Length..];
            }
            else if (lines[i].StartsWith(Date))
            {
                blogPostOfMarkdown.Date = DateTime.Parse(lines[i][Date.Length..]);
            }
            else if (lines[i].StartsWith(LastModifyDate))
            {
                blogPostOfMarkdown.LastModifyDate = DateTime.Parse(lines[i][LastModifyDate.Length..]);
            }
            else if (lines[i].StartsWith(Copyright))
            {
                blogPostOfMarkdown.Copyright =
                    (CopyRightType)Enum.Parse(typeof(CopyRightType), lines[i][Copyright.Length..]);
            }
            else if (lines[i].StartsWith(Author))
            {
                blogPostOfMarkdown.Author = lines[i][Author.Length..];
            }
            else if (lines[i].StartsWith(OriginalTitle))
            {
                blogPostOfMarkdown.OriginalTitle = lines[i][OriginalTitle.Length..];
            }
            else if (lines[i].StartsWith(OriginalLink))
            {
                blogPostOfMarkdown.OriginalLink = lines[i][OriginalLink.Length..];
            }
            else if (lines[i].StartsWith(Draft))
            {
                blogPostOfMarkdown.Draft = bool.Parse(lines[i][Draft.Length..]);
            }
            else if (lines[i].StartsWith(Cover))
            {
                blogPostOfMarkdown.Cover = lines[i][Cover.Length..];
            }
            else if (lines[i].StartsWith(Categories))
            {
                blogPostOfMarkdown.Categories =
                    lines[i][Categories.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
            else if (lines[i].StartsWith(Tags))
            {
                blogPostOfMarkdown.Tags = lines[i][Tags.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
            else if (lines[i].StartsWith(Banner))
            {
                blogPostOfMarkdown.Banner = bool.Parse(lines[i][Banner.Length..]);
            }
        }

        blogPostOfMarkdown.LastModifyDate ??= blogPostOfMarkdown.Date;

        return blogPostOfMarkdown;
    }

    private static void Write(string markdownAbsolutePath, BlogPostSeedDto blogPostOfMarkdown)
    {
        List<string> lines = new List<string>();
        lines.Add("---");

        if (!string.IsNullOrWhiteSpace(blogPostOfMarkdown.Title))
        {
            lines.Add($"{Title}{blogPostOfMarkdown.Title}");
        }

        if (!string.IsNullOrWhiteSpace(blogPostOfMarkdown.Slug))
        {
            lines.Add($"{Slug}{blogPostOfMarkdown.Slug}");
        }

        if (!string.IsNullOrWhiteSpace(blogPostOfMarkdown.Description))
        {
            lines.Add($"{Description}{blogPostOfMarkdown.Description}");
        }

        if (blogPostOfMarkdown.Date != default)
        {
            lines.Add($"{Date}{blogPostOfMarkdown.Date:yyyy-MM-dd HH:mm:ss}");
        }

        if (blogPostOfMarkdown.LastModifyDate != null && blogPostOfMarkdown.LastModifyDate != default(DateTime))
        {
            lines.Add($"{LastModifyDate}{blogPostOfMarkdown.LastModifyDate:yyyy-MM-dd HH:mm:ss}");
        }

        lines.Add($"{Copyright}{blogPostOfMarkdown.Copyright}");
        if (!string.IsNullOrWhiteSpace(blogPostOfMarkdown.Author))
        {
            lines.Add($"{Author}{blogPostOfMarkdown.Author}");
        }

        if (!string.IsNullOrWhiteSpace(blogPostOfMarkdown.OriginalTitle))
        {
            lines.Add($"{OriginalTitle}{blogPostOfMarkdown.OriginalTitle}");
        }

        if (!string.IsNullOrWhiteSpace(blogPostOfMarkdown.OriginalLink))
        {
            lines.Add($"{OriginalLink}{blogPostOfMarkdown.OriginalLink}");
        }

        lines.Add($"{Draft}{blogPostOfMarkdown.Draft}");

        if (!string.IsNullOrWhiteSpace(blogPostOfMarkdown.Cover))
        {
            lines.Add($"{Cover}{blogPostOfMarkdown.Cover}");
        }

        if (blogPostOfMarkdown.Categories is { Length: > 0 })
        {
            lines.Add($"{Categories}{string.Join(',', blogPostOfMarkdown.Categories)}");
        }

        if (blogPostOfMarkdown.Tags is { Length: > 0 })
        {
            lines.Add($"{Tags}{string.Join(',', blogPostOfMarkdown.Tags)}");
        }

        lines.Add("---");

        if (File.Exists(markdownAbsolutePath))
        {
            File.Delete(markdownAbsolutePath);
        }

        File.WriteAllLines(markdownAbsolutePath, lines);
        File.AppendAllText(markdownAbsolutePath, $"\n{blogPostOfMarkdown.Content}");
    }
}

public class BlogPostSeedDto
{
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime Date { get; set; }
    public DateTime? LastModifyDate { get; set; }
    public CopyRightType Copyright { get; set; }
    public string Author { get; set; } = null!;
    public string? OriginalTitle { get; set; }
    public string? OriginalLink { get; set; }
    public bool Draft { get; set; }
    public string Cover { get; set; } = null!;
    public string[]? Categories { get; set; }
    public string[]? Tags { get; set; }
    public string Content { get; set; } = null!;
    public bool Banner { get; set; }
}

public enum CopyRightType
{
    [EnumMember(Value = "original")] Original,
    [EnumMember(Value = "reprinted")] Reprinted,
    [EnumMember(Value = "contributes")] Contributes
}

public static class CopyRightTypeExtensions
{
    public static string GetDescription(this CopyRightType copyRightType)
    {
        return copyRightType switch
        {
            CopyRightType.Original => "原创",
            CopyRightType.Reprinted => "转载",
            _ => "投稿"
        };
    }
}