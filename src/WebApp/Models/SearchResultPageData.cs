namespace WebApp.Models;

public record SearchResultPageData(
    int PageIndex,
    int PageSize,
    int Total,
    List<SearchResultItem> Data,
    int ToolCount,
    int DocCount,
    int PostCount);
