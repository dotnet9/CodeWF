using CodeWF.Models;
using CodeWF.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Components;

public class FriendLinkViewComponent : ViewComponent
{
    private readonly AppService _appService;

    public FriendLinkViewComponent(AppService appService)
    {
        _appService = appService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var links = await _appService.GetAllFriendLinkItemsAsync();
        var model = new FriendLinkViewModel
        {
            Links = links ?? new List<FriendLinkItem>()
        };

        return View(model);
    }
}

public class FriendLinkViewModel
{
    public List<FriendLinkItem> Links { get; set; } = new();
}
