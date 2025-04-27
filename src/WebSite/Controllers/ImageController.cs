using CodeWF.Options;
using CodeWF.Tools;
using CodeWF.Tools.FileExtensions;
using CodeWF.Tools.Image;
using HashidsNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebSite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private const string IconFolder = "UploadIcons";

    [HttpPost("merge")]
    [AllowAnonymous]
    public async Task<IActionResult> MergeGenerateIconAsync([FromForm] IFormFile sourceImage, [FromForm] string sizes,
        [FromServices] IWebHostEnvironment env)
    {
        try
        {
            var convertSizes = sizes.Split(',').Select(uint.Parse).ToArray();

            var fullPath = await SaveFileAsync(sourceImage, env);
            var fileName = $"{Guid.NewGuid():N}.ico";
            var icoFullPath = Path.Combine(env.WebRootPath, IconFolder, fileName);

            Directory.CreateDirectory(Path.Combine(env.WebRootPath, IconFolder));

            await ImageHelper.MergeGenerateIcon(fullPath, icoFullPath, convertSizes);

            var iconUrl = $"/{IconFolder}/{fileName}";
            return Ok(new { success = true, url = iconUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("separate")]
    [AllowAnonymous]
    public async Task<IActionResult> SeparateGenerateIconAsync([FromForm] IFormFile sourceImage,
        [FromForm] string sizes, [FromServices] IWebHostEnvironment env,
        [FromServices] ISevenZipCompressor sevenZipCompressor)
    {
        try
        {
            var convertSizes = sizes.Split(',').Select(uint.Parse).ToArray();

            var folderName = $"icons_{Guid.NewGuid():N}";
            var iconFolderPath = Path.Combine(env.WebRootPath, IconFolder, folderName);
            Directory.CreateDirectory(iconFolderPath);

            var sourceFilePath = await SaveFileAsync(sourceImage, env);
            await ImageHelper.SeparateGenerateIcon(sourceFilePath, iconFolderPath, convertSizes);

            var zipFileName = $"{folderName}.zip";
            var zipFilePath = Path.Combine(env.WebRootPath, IconFolder, zipFileName);

            await Task.Run(() => sevenZipCompressor.Zip(iconFolderPath, zipFilePath));

            Directory.Delete(iconFolderPath, true);

            var zipUrl = $"/{IconFolder}/{zipFileName}";
            return Ok(new { success = true, url = zipUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("nuoche")]
    [AllowAnonymous]
    public async Task<IActionResult> NuoCheAsync([FromBody] NuoCheRequest request,
        [FromServices] IWebHostEnvironment env,
        [FromServices] IOptions<SiteOption> siteOption)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new { success = false, message = "标题和手机号码不能为空" });
            }

            if (!long.TryParse(request.PhoneNumber, out long phoneNumberLong))
            {
                return BadRequest(new { success = false, message = "无效的手机号码" });
            }

            var encodedPhone = new Hashids("codewf").EncodeLong(phoneNumberLong);
            var generatedUrl = $"{siteOption.Value.Domain}/nuoche?p={encodedPhone}";

            var fileName = $"qrcode_{Guid.NewGuid():N}.png";
            var qrCodePath = Path.Combine(env.WebRootPath, IconFolder, fileName);

            Directory.CreateDirectory(Path.Combine(env.WebRootPath, IconFolder));

            QrCodeGenerator.GenerateQrCode(request.Title, generatedUrl, qrCodePath, request.SubTitle);

            var qrCodeUrl = $"/{IconFolder}/{fileName}";
            return Ok(new
            {
                success = true,
                qrCodeUrl,
                generatedUrl
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    private async Task<string> SaveFileAsync(IFormFile file, IWebHostEnvironment env)
    {
        var saveFileName = Guid.NewGuid().ToString("N");
        var saveFolder = Path.Combine(env.WebRootPath, IconFolder);
        Directory.CreateDirectory(saveFolder);

        var saveFullPath = Path.Combine(saveFolder, saveFileName);
        await using var fs = new FileStream(saveFullPath, FileMode.Create);
        await file.CopyToAsync(fs);
        return saveFullPath;
    }

    public class NuoCheRequest
    {
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public string? SubTitle { get; set; }
    }
}