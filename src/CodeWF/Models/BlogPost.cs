namespace CodeWF.Models;

public class BlogPostBrief
{
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
}

public class BlogPost : BlogPostBrief
{
    public string? Content { get; set; }
    public string? HtmlContent { get; set; }
}

public class CopyRights
{
    public const string Reprinted = nameof(Reprinted);
    public const string Contributes = nameof(Contributes);
    public const string Original = nameof(Original);
}