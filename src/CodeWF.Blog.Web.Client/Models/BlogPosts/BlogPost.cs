namespace CodeWF.Blog.Web.Client.Models.BlogPosts;

public class BlogPost
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
    public string? Content { get; set; }
}