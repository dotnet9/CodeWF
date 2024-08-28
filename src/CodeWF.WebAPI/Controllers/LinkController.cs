using System.Text.Json;
using CodeWF.WebAPI.Options;
using CodeWF.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CodeWF.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LinkController(
    ILogger<LinkController> logger,
    IOptions<SiteOption> siteOptions,
    IMemoryCache memoryCache)
    : ControllerBase
{
    /// <summary>
    ///     获取所有友情链接
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<Link>?> GetAsync()
    {
        const string cacheKey = $"{nameof(LinkController)}_{nameof(GetAsync)}";
        if (memoryCache.TryGetValue(cacheKey, out List<Link>? links)) return links;

        var file = Path.Combine(siteOptions.Value.Assets!, "site", "FriendLink.json");
        if (!System.IO.File.Exists(file))
        {
            throw new Exception("没有数据！");
        }

        var jsonContent = await System.IO.File.ReadAllTextAsync(file);
        links = JsonSerializer.Deserialize<List<Link>>(jsonContent)!;
        memoryCache.Set(cacheKey, links);
        return links;
    }
}