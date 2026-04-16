using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using HashidsNet;
using CodeWF.Tools;
using QRCoder;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private const string IconFolder = "UploadIcons";
    private readonly ILogger<ImageController> _logger;

    public ImageController(ILogger<ImageController> logger)
    {
        _logger = logger;
    }

    [HttpPost("merge")]
    public async Task<IActionResult> MergeGenerateIcon([FromForm] IFormFile sourceImage, [FromForm] string sizes)
    {
        try
        {
            var env = HttpContext.RequestServices.GetService<IWebHostEnvironment>();
            if (env == null) return BadRequest(new { success = false, message = "服务不可用" });

            var convertSizes = sizes.Split(',').Select(uint.Parse).ToArray();

            var folderPath = Path.Combine(env.WebRootPath, IconFolder);
            Directory.CreateDirectory(folderPath);

            var sourcePath = Path.Combine(folderPath, $"{Guid.NewGuid():N}.png");
            var icoFullPath = Path.Combine(folderPath, $"{Guid.NewGuid():N}.ico");

            await using (var fs = new FileStream(sourcePath, FileMode.Create))
            {
                await sourceImage.CopyToAsync(fs);
            }

            _logger.LogInformation("调用ImageHelper.MergeGenerateIcon, sourcePath={sourcePath}, icoFullPath={icoFullPath}, sizes={sizes}", sourcePath, icoFullPath, string.Join(",", convertSizes));
            await ImageHelper.MergeGenerateIcon(sourcePath, icoFullPath, convertSizes);

            System.IO.File.Delete(sourcePath);

            var iconUrl = $"/{IconFolder}/{Path.GetFileName(icoFullPath)}";
            return Ok(new { success = true, url = iconUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MergeGenerateIcon错误: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("separate")]
    public async Task<IActionResult> SeparateGenerateIcon([FromForm] IFormFile sourceImage, [FromForm] string sizes)
    {
        try
        {
            var env = HttpContext.RequestServices.GetService<IWebHostEnvironment>();
            if (env == null) return BadRequest(new { success = false, message = "服务不可用" });

            var convertSizes = sizes.Split(',').Select(uint.Parse).ToArray();

            var folderName = $"icons_{Guid.NewGuid():N}";
            var iconFolderPath = Path.Combine(env.WebRootPath, IconFolder, folderName);
            Directory.CreateDirectory(iconFolderPath);

            var sourcePath = Path.Combine(env.WebRootPath, IconFolder, $"{Guid.NewGuid():N}.png");
            await using (var fs = new FileStream(sourcePath, FileMode.Create))
            {
                await sourceImage.CopyToAsync(fs);
            }

            _logger.LogInformation("调用ImageHelper.SeparateGenerateIcon, sourcePath={sourcePath}, iconFolderPath={iconFolderPath}, sizes={sizes}", sourcePath, iconFolderPath, string.Join(",", convertSizes));
            await ImageHelper.SeparateGenerateIcon(sourcePath, iconFolderPath, convertSizes);

            System.IO.File.Delete(sourcePath);

            var zipFileName = $"{folderName}.zip";
            var zipFilePath = Path.Combine(env.WebRootPath, IconFolder, zipFileName);

            if (System.IO.File.Exists(zipFilePath))
                System.IO.File.Delete(zipFilePath);

            await ZipFile.CreateFromDirectoryAsync(iconFolderPath, zipFilePath);
            Directory.Delete(iconFolderPath, true);

            var zipUrl = $"/{IconFolder}/{zipFileName}";
            return Ok(new { success = true, url = zipUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SeparateGenerateIcon错误: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("nuoche")]
    public async Task<IActionResult> NuoChe([FromBody] NuoCheRequest request)
    {
        try
        {
            var env = HttpContext.RequestServices.GetService<IWebHostEnvironment>();
            if (env == null) return BadRequest(new { success = false, message = "服务不可用" });

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.PhoneNumber))
                return BadRequest(new { success = false, message = "标题和手机号码不能为空" });

            if (!long.TryParse(request.PhoneNumber, out _))
                return BadRequest(new { success = false, message = "无效的手机号码" });

            var encodedPhone = new Hashids("codewf").EncodeLong(long.Parse(request.PhoneNumber));
            var domain = "http://localhost:5224";
            var generatedUrl = $"{domain}/nuoche?p={encodedPhone}";

            var folderPath = Path.Combine(env.WebRootPath, IconFolder);
            Directory.CreateDirectory(folderPath);

            var fileName = $"qrcode_{Guid.NewGuid():N}.png";
            var qrCodePath = Path.Combine(folderPath, fileName);

            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(generatedUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(10);
            await System.IO.File.WriteAllBytesAsync(qrCodePath, qrCodeBytes);

            var qrCodeUrl = $"/{IconFolder}/{fileName}";
            return Ok(new { success = true, qrCodeUrl, generatedUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NuoChe错误: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public class NuoCheRequest
{
    public string Title { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
}
