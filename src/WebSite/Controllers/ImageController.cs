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
            // 解析尺寸
            var convertSizes = sizes.Split(',').Select(uint.Parse).ToArray();

            // 保存上传的文件
            var fullPath = await SaveFileAsync(sourceImage, env);
            var fileName = $"{Guid.NewGuid():N}.ico";
            var icoFullPath = Path.Combine(env.WebRootPath, IconFolder, fileName);

            // 确保目录存在
            Directory.CreateDirectory(Path.Combine(env.WebRootPath, IconFolder));

            // 生成图标
            await ImageHelper.MergeGenerateIcon(fullPath, icoFullPath, convertSizes);

            // 返回可访问的URL
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

            // 创建临时文件夹存放分离的图标
            var folderName = $"icons_{Guid.NewGuid():N}";
            var iconFolderPath = Path.Combine(env.WebRootPath, IconFolder, folderName);
            Directory.CreateDirectory(iconFolderPath);

            // 保存上传的文件并生成图标
            var sourceFilePath = await SaveFileAsync(sourceImage, env);
            await ImageHelper.SeparateGenerateIcon(sourceFilePath, iconFolderPath, convertSizes);

            // 创建压缩文件
            var zipFileName = $"{folderName}.zip";
            var zipFilePath = Path.Combine(env.WebRootPath, IconFolder, zipFileName);

            // 压缩文件夹
            await Task.Run(() => sevenZipCompressor.Zip(iconFolderPath, zipFilePath));

            // 清理临时文件夹
            Directory.Delete(iconFolderPath, true);

            // 返回zip文件的URL
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

            // 确保目录存在
            Directory.CreateDirectory(Path.Combine(env.WebRootPath, IconFolder));

            // 生成二维码
            QrCodeGenerator.GenerateQrCode(request.Title, generatedUrl, qrCodePath);

            // 返回可访问的URL
            var qrCodeUrl = $"/{IconFolder}/{fileName}";
            return Ok(new
            {
                success = true,
                qrCodeUrl = qrCodeUrl,
                generatedUrl = generatedUrl
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
    }
}