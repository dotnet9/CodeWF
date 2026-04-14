using CodeWF.Models;
using CodeWF.Services;
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

        var model = new NavigationViewModel
        {
            Albums = albums ?? new List<AlbumItem>(),
            Categories = categories ?? new List<CategoryItem>()
        };

        return View(model);
    }
}

public class NavigationViewModel
{
    public List<AlbumItem> Albums { get; set; } = new();
    public List<CategoryItem> Categories { get; set; } = new();
}
