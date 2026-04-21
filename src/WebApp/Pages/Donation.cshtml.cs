using WebApp.Options;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace WebApp.Pages;

public class DonationModel : PageModel
{
    private readonly AppService _appService;
    private readonly IOptions<SiteOption> _siteOption;

    public string HtmlContent { get; set; } = string.Empty;
    public string DebugInfo { get; set; } = string.Empty;

    public DonationModel(AppService appService, IOptions<SiteOption> siteOption)
    {
        _appService = appService;
        _siteOption = siteOption;
    }

    public async Task OnGetAsync()
    {
        var localAssetsDir = _siteOption.Value.LocalAssetsDir;
        var filePath = Path.Combine(localAssetsDir ?? "", "site", "pays", "Donation.md");
        var fileExists = System.IO.File.Exists(filePath);

        DebugInfo = $"LocalAssetsDir: {localAssetsDir}<br>FilePath: {filePath}<br>Exists: {fileExists}";

        var (_, htmlContent) = await _appService.ReadDonationAsync();
        HtmlContent = htmlContent ?? "<p>赞助内容加载中...</p>";
    }
}
