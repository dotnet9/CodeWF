namespace CodeWF.Models;

public class ToolItem
{
    public string? Name { get; set; }
    public string? Memo { get; set; }
    public string? Slug { get; set; }
    public string? Repository { get; set; }
    public List<ToolItem>? Children { get; set; }
}