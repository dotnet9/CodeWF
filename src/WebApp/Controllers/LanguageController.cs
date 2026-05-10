using System.Diagnostics;
using CodeWfLogger = CodeWF.Log.Core.Logger;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services;

namespace WebApp.Controllers;

[ApiController]
[Route("api/language")]
public sealed class LanguageController : ControllerBase
{
    private readonly I18nService _i18nService;
    private readonly LanguagePreparationService _languagePreparationService;
    private readonly ILogger<LanguageController> _logger;

    public LanguageController(
        I18nService i18nService,
        LanguagePreparationService languagePreparationService,
        ILogger<LanguageController> logger)
    {
        _i18nService = i18nService;
        _languagePreparationService = languagePreparationService;
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetStatus([FromQuery] string? language)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language);
        if (normalizedLanguage is null)
        {
            return BadRequest(new { success = false, message = "Unsupported language." });
        }

        var status = _i18nService.GetLanguageResourceStatus(normalizedLanguage);
        return Ok(new
        {
            success = true,
            language = status.Code,
            languageName = status.NativeName,
            englishName = status.EnglishName,
            isDefaultLanguage = status.IsDefaultLanguage,
            hasLanguageResource = status.HasResourceFile
        });
    }

    [HttpPost("prepare")]
    public async Task<IActionResult> PrepareAsync(
        [FromBody] LanguagePrepareRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedLanguage = RequestLanguage.Normalize(request.Language);
        if (normalizedLanguage is null)
        {
            return BadRequest(new { success = false, message = "Unsupported language." });
        }

        var targetUri = BuildLocalTargetUri(request.Url, normalizedLanguage);
        if (targetUri is null)
        {
            return BadRequest(new { success = false, message = "Invalid target URL." });
        }

        var stopwatch = Stopwatch.StartNew();
        CodeWfLogger.Info(
            $"语言跳转内容预热开始。language={normalizedLanguage}; url={targetUri}.",
            log2UI: false,
            log2File: false,
            log2Console: true);

        try
        {
            if (request.Background)
            {
                var queuedJob = _languagePreparationService.Queue(normalizedLanguage, targetUri);
                stopwatch.Stop();

                CodeWfLogger.Info(
                    $"语言跳转内容后台预热已入队。language={normalizedLanguage}; jobId={queuedJob.JobId}; url={targetUri}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);

                return Ok(ToJobResponse(queuedJob, success: true, background: true));
            }

            var job = await _languagePreparationService.PrepareNowAsync(normalizedLanguage, targetUri, cancellationToken);
            stopwatch.Stop();

            CodeWfLogger.Info(
                $"语言跳转内容预热完成。language={normalizedLanguage}; url={targetUri}; jobId={job.JobId}; state={job.State}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                log2UI: false,
                log2File: false,
                log2Console: true);

            return Ok(ToJobResponse(job, success: !job.IsFailed, background: false));
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            CodeWfLogger.Warn(
                $"语言跳转内容预热取消。language={normalizedLanguage}; url={targetUri}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            CodeWfLogger.Error(
                $"语言跳转内容预热失败。language={normalizedLanguage}; url={targetUri}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogError(ex, "Failed to prepare language content for {Language} {Url}.", normalizedLanguage, targetUri);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                language = normalizedLanguage,
                message = "Failed to prepare language content."
            });
        }
    }

    [HttpGet("prepare/{jobId}")]
    public IActionResult GetPrepareJob(string jobId)
    {
        var job = _languagePreparationService.GetJob(jobId);
        return job is null
            ? NotFound(new { success = false, message = "Language preparation job was not found." })
            : Ok(ToJobResponse(job, success: !job.IsFailed, background: true));
    }

    private Uri? BuildLocalTargetUri(string? url, string language)
    {
        var origin = $"{GetExternalRequestScheme()}://{Request.Host}";
        var baseUri = new Uri($"{origin}/");
        var targetText = string.IsNullOrWhiteSpace(url)
            ? RequestLanguage.LocalizePath("/", language)
            : url.Trim();

        if (!Uri.TryCreate(targetText, UriKind.Absolute, out var targetUri)
            && !Uri.TryCreate(baseUri, targetText, out targetUri))
        {
            return null;
        }

        if (!IsSameRequestHost(targetUri)
            || !IsHttpScheme(targetUri.Scheme))
        {
            return null;
        }

        targetUri = new Uri(baseUri, $"{targetUri.AbsolutePath}{targetUri.Query}{targetUri.Fragment}");

        var path = new PathString(targetUri.AbsolutePath);
        var hasPathLanguage = RequestLanguage.TryGetPathLanguage(path, out var pathLanguage, out var remainingPath);
        if (!hasPathLanguage)
        {
            path = remainingPath;
        }

        var routePath = hasPathLanguage ? remainingPath : path;
        if (RequestLanguage.ShouldSkipLocalization(routePath)
            || (routePath.Value ?? string.Empty).StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!hasPathLanguage || !string.Equals(pathLanguage, language, StringComparison.OrdinalIgnoreCase))
        {
            var localizedPath = RequestLanguage.LocalizePath(
                $"{routePath}{targetUri.Query}{targetUri.Fragment}",
                language);
            targetUri = new Uri(baseUri, localizedPath);
        }

        return targetUri;
    }

    private bool IsSameRequestHost(Uri targetUri) =>
        string.Equals(targetUri.Authority, Request.Host.Value, StringComparison.OrdinalIgnoreCase);

    private string GetExternalRequestScheme()
    {
        var forwardedProto = Request.Headers["X-Forwarded-Proto"]
            .FirstOrDefault()?
            .Split(',', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        return !string.IsNullOrWhiteSpace(forwardedProto) && IsHttpScheme(forwardedProto)
            ? forwardedProto
            : Request.Scheme;
    }

    private static bool IsHttpScheme(string scheme) =>
        string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
        || string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

    private static object ToJobResponse(
        LanguagePreparationJobSnapshot job,
        bool success,
        bool background) => new
        {
            success,
            background,
            jobId = job.JobId,
            language = job.Language,
            targetUrl = job.TargetUrl,
            state = job.State,
            isCompleted = job.IsCompleted,
            isFailed = job.IsFailed,
            hadLanguageResource = job.HadLanguageResource,
            hasLanguageResource = job.HasLanguageResource,
            createdLanguageResource = job.CreatedLanguageResource,
            pagePrepared = job.PagePrepared,
            stringCount = job.StringCount,
            textMapCount = job.TextMapCount,
            errorMessage = job.ErrorMessage,
            updatedAt = job.UpdatedAt
        };
}

public sealed record LanguagePrepareRequest(string? Language, string? Url, bool Background = false);
