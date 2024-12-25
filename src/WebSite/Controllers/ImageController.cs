using CodeWF.Tools.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace WebSite.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class ImageController : ControllerBase
{
    private const int MaxSize = 10 * 1024 * 1024;

    [HttpPost]
    public async Task<IActionResult> MergeGenerateIconAsync(IFormFile sourceImage, uint[] sizes,
        [FromServices] IWebHostEnvironment env)
    {
        if (sourceImage.Length == 0 || sourceImage.Length > MaxSize)
        {
            return BadRequest(new { Code = 1001, Msg = "不支持大于10MB的文件" });
        }

        if (sizes == null || sizes.Length <= 0)
        {
            return BadRequest(new { Code = 1001, Msg = "未提供转换尺寸" });
        }

        var fileName = Guid.NewGuid().ToString("N");
        var relativePath = Path.Combine("uploads", fileName);
        var fullPath = Path.Combine(env.ContentRootPath, "Uploads", fileName);
        var icoFullPath = Path.Combine(env.ContentRootPath, "Uploads", $"{fileName}.ico");
        using (FileStream fs = new FileStream(fullPath, FileMode.Create))
        {
            sourceImage.CopyTo(fs);
            fs.Flush();
        }

        ImageHelper.MergeGenerateIcon(fullPath, icoFullPath, sizes);
        return Ok(new { Data = icoFullPath, Code = 2001, Msg = "转换成功" });
    }

    [HttpPost]
    public async Task<IActionResult> SeparateGenerateIcon(IFormFile sourceImage, uint[] sizes,
        [FromServices] IWebHostEnvironment env)
    {
        if (sourceImage.Length == 0 || sourceImage.Length > MaxSize)
        {
            return BadRequest(new { Code = 1001, Msg = "不支持大于10MB的文件" });
        }

        if (sizes == null || sizes.Length <= 0)
        {
            return BadRequest(new { Code = 1001, Msg = "未提供转换尺寸" });
        }

        var fileName = Guid.NewGuid().ToString("N");
        var relativePath = Path.Combine("uploads", fileName);
        var fullPath = Path.Combine(env.ContentRootPath, "Uploads", fileName);
        var icoFullPath = Path.Combine(env.ContentRootPath, "Uploads", $"{fileName}.ico");
        using (FileStream fs = new FileStream(fullPath, FileMode.Create))
        {
            sourceImage.CopyTo(fs);
            fs.Flush();
        }

        ImageHelper.SeparateGenerateIcon(fullPath, icoFullPath, sizes);
        return Ok(new { Data = icoFullPath, Code = 2001, Msg = "转换成功" });
    }
}