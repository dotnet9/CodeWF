using CodeWF.Models;
using CodeWF.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class TimeLineModel : PageModel
{
    private readonly AppService _appService;

    public List<TimeLineItem>? TimeLines { get; set; }

    public TimeLineModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        TimeLines = await _appService.GetTimeLineItemsAsync();
    }
}
