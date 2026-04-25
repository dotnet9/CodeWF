namespace WebApp.Models;

public class PageBannerViewModel
{
    public string Eyebrow { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IReadOnlyList<string> MetaItems { get; init; } = [];
}
