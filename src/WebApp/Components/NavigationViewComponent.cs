using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Components;

public class NavigationViewComponent : ViewComponent
{
    private readonly AppService _appService;

    public NavigationViewComponent(AppService appService)
    {
        _appService = appService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var albums = await _appService.GetAllAlbumItemsAsync();
        var categories = await _appService.GetAllCategoryItemsAsync();
        var posts = await _appService.GetAllBlogPostsAsync() ?? [];

        var model = new NavigationViewModel
        {
            Albums = (albums ?? [])
                .Where(item =>
                    !string.Equals(item.Slug, "default", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(item.Name)
                    && !string.IsNullOrWhiteSpace(item.Slug))
                .OrderBy(item => item.Sort)
                .Select(item => new NavigationBrowseItem(
                    item.Name!,
                    item.Slug!,
                    item.Memo,
                    posts.Count(post => post.Albums?.Contains(item.Name, StringComparer.OrdinalIgnoreCase) == true)))
                .ToList(),
            Categories = (categories ?? [])
                .Where(item =>
                    !string.Equals(item.Slug, "default", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(item.Name)
                    && !string.IsNullOrWhiteSpace(item.Slug))
                .OrderBy(item => item.Sort)
                .Select(item => new NavigationBrowseItem(
                    item.Name!,
                    item.Slug!,
                    item.Memo,
                    posts.Count(post => post.Categories?.Contains(item.Name, StringComparer.OrdinalIgnoreCase) == true)))
                .ToList()
        };

        return View(model);
    }
}

public sealed record NavigationBrowseItem(string Name, string Slug, string? Memo, int PostCount);

public class NavigationViewModel
{
    public List<NavigationBrowseItem> Albums { get; set; } = new();
    public List<NavigationBrowseItem> Categories { get; set; } = new();
}
