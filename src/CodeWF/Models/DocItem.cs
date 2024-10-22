namespace CodeWF.Models;

public class DocItem
{
    public string? Name { get; set; }
    public string? Memo { get; set; }
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public string? HtmlContent { get; set; }
    public List<DocItem>? Children { get; set; }
}