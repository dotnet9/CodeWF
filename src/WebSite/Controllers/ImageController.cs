using CodeWF.Tools.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace WebSite.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class ImageController : ControllerBase
{
    private const int MaxSize = 10 * 1024 * 1024;
    private const string IconFolder = "UploadIcons";

    [HttpPost]
    public async Task<IActionResult> MergeGenerateIconAsync(IFormFile sourceImage, uint[]? sizes,
        [FromServices] IWebHostEnvironment env)
    {
        return await ProcessIconAsync(sourceImage, sizes, env, ImageHelper.MergeGenerateIcon);
    }

    [HttpPost]
    public async Task<IActionResult> SeparateGenerateIcon(IFormFile sourceImage, uint[]? sizes,
        [FromServices] IWebHostEnvironment env)
    {
        return await ProcessIconAsync(sourceImage, sizes, env, ImageHelper.SeparateGenerateIcon);
    }

    private async Task<IActionResult> ProcessIconAsync(IFormFile sourceImage, uint[]? sizes,
        IWebHostEnvironment env, Func<string, string, uint[], Task> iconGenerator)
    {
        if (sizes is not { Length: > 0 })
        {
            return BadRequest(new { Code = 1001, Msg = "未提供转换尺寸" });
        }

        var fullPath = await SaveFileAsync(sourceImage, env);
        var icoFullPath = Path.ChangeExtension(fullPath, ".ico");
        await iconGenerator(fullPath, icoFullPath, sizes);
        return Ok(new { Data = icoFullPath, Code = 2001, Msg = "转换成功" });
    }

    private async Task<string> SaveFileAsync(IFormFile sourceImage, IWebHostEnvironment env)
    {
        if (sourceImage.Length is 0 or > MaxSize)
        {
            throw new BadHttpRequestException("不支持大于10MB的文件", 1001);
        }

        var fileName = Guid.NewGuid().ToString("N");
        var fullPath = Path.Combine(env.ContentRootPath, "Uploads", fileName);
        await using FileStream fs = new FileStream(fullPath, FileMode.Create);
        await sourceImage.CopyToAsync(fs);
        fs.Flush();
        return fullPath;
    }
}