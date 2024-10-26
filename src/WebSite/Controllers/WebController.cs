using CodeWF.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebSite.Controllers;

public class WebController(IOptions<SiteOption> siteOption, AppService appService) : ControllerBase
{
    [Route("/rss")]
    public async Task<ContentResult> RssAsync()
    {
        var rss = await appService.GetRssAsync();

        return Content(rss ?? "", "application/xml");
    }
}