namespace WebApp.Models;

public class SearchBlockedKeywordGroup
{
    public int Sort { get; set; }
    public string? Name { get; set; }
    public string? Memo { get; set; }
    public List<string>? Keywords { get; set; }
}
