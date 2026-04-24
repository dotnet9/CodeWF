using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class DonationModel : PageModel
{
    private readonly AppService _appService;

    public string HtmlContent { get; private set; } = string.Empty;

    public DonationModel(AppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        var (_, htmlContent) = await _appService.ReadDonationAsync();
        HtmlContent = htmlContent ?? "<p>赞助内容加载中...</p>";
    }
}
