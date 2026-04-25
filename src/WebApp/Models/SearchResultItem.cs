namespace WebApp.Models;

public enum SearchResultKind
{
    Tool = 0,
    Doc = 1,
    Post = 2
}

public class SearchResultItem
{
    public SearchResultKind Kind { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = "/";
    public string? Summary { get; init; }
    public string? MatchedSnippet { get; init; }
    public string? Context { get; init; }
    public string? Slug { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int Score { get; init; }

    public string KindLabel => Kind switch
    {
        SearchResultKind.Tool => "工具",
        SearchResultKind.Doc => "文档",
        SearchResultKind.Post => "文章",
        _ => "结果"
    };

    public string KindCssClass => Kind switch
    {
        SearchResultKind.Tool => "tool",
        SearchResultKind.Doc => "doc",
        SearchResultKind.Post => "post",
        _ => "result"
    };
}
