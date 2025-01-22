using CodeWF.Tools;
using CodeWF.Tools.FileExtensions;
using Microsoft.AspNetCore.Mvc;
using WebSite.ViewModels;

namespace WebSite.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class ImageController : ControllerBase
{
    private const string IconFolder = "UploadIcons";

    [HttpPost]
    public async Task<IActionResult> MergeGenerateIconAsync([FromForm] ConvertIconRequest request,
        [FromServices] IWebHostEnvironment env)
    {
        var fullPath = await SaveFileAsync(request, env);
        var icoFullPath = Path.ChangeExtension(fullPath, ".ico");
        await ImageHelper.MergeGenerateIcon(fullPath, icoFullPath, request.ConvertSizes);
        return Ok(new { Data = icoFullPath, Code = 2001, Msg = "转换成功" });
    }

    [HttpPost]
    public async Task<IActionResult> SeparateGenerateIcon([FromForm] ConvertIconRequest request,
        [FromServices] IWebHostEnvironment env, [FromServices] ISevenZipCompressor sevenZipCompressor)
    {
        var fullPath = await SaveFileAsync(request, env);
        var iconFolderName = Guid.NewGuid().ToString("N");
        var iconFolderPath = Path.Combine(env.ContentRootPath, IconFolder, iconFolderName);
        var iconZipPath = $"{iconFolderPath}.7z";
        await ImageHelper.MergeGenerateIcon(fullPath, iconFolderPath, request.ConvertSizes);
        sevenZipCompressor.Zip(iconFolderPath, iconZipPath);
        return Ok(new { Data = iconZipPath, Code = 2001, Msg = "转换成功" });
    }

    private async Task<string> SaveFileAsync(ConvertIconRequest request, IWebHostEnvironment env)
    {
        var saveFileName = Guid.NewGuid().ToString("N");
        var saveFolder = Path.Combine(env.ContentRootPath, IconFolder);
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        var saveFullPath = Path.Combine(saveFolder, saveFileName);
        await using FileStream fs = new FileStream(saveFullPath, FileMode.Create);
        await request.SourceImage.CopyToAsync(fs);
        fs.Flush();
        return saveFullPath;
    }
}