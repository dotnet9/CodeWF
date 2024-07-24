namespace CodeWF.Docs.Shared.Pages;

public partial class PostIntro
{
    [CascadingParameter(Name = "Culture")]
    private string? Culture { get; set; }
    
    IEnumerable<(string? Title, string? Href)>? _buildingBlocks;
    IEnumerable<(string? Title, string? Href)>? _utils;

    protected override async Task OnInitializedAsync()
    {
        var navs = await DocService.ReadNavsAsync("post");

        _buildingBlocks = GetNavs("building-blocks");
        _utils = GetNavs("utils");
        return;

        IEnumerable<(string? Title, string? Href)> GetNavs(string title)
        {
            return navs.FirstOrDefault(u => u.Title == title)?.Children?.Select(u => (u.Title, u.Href ?? u.Children?.FirstOrDefault()?.Href)) ??
                   new List<(string?, string?)>();
        }
    }
}
