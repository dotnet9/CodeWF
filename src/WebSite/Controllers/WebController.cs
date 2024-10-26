using CodeWF.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Mime;

namespace WebSite.Controllers;

public class WebController(IOptions<SiteOption> siteOption, AppService appService) : ControllerBase
{
    const string ContentType = "application/xml";

    [Route("/rss")]
    public async Task<IActionResult> GetRssAsync()
    {
        var rss = await appService.GetRssAsync();

        return Content(rss, ContentType);
    }

    [Route("/sitemap.xml")]
    public async Task<IActionResult> GetSiteMapAsync()
    {
        const string cacheKey = "sitemap.xml";
        ContentDisposition cd = new()
        {
            FileName = cacheKey,
            Inline = true
        };
        Response.Headers.Append("Content-Disposition", cd.ToString());
        var rss = await appService.GetSiteMapAsync();

        return Content(rss, ContentType);
    }
}