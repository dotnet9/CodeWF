using System.Collections.Concurrent;
using System.Diagnostics;
using CodeWfLogger = CodeWF.Log.Core.Logger;

namespace WebApp.Services;

public sealed class LanguagePreparationService
{
    private readonly I18nService _i18nService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LanguagePreparationService> _logger;
    private readonly ConcurrentDictionary<string, LanguagePreparationJob> _jobs = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _jobIdsByKey = new(StringComparer.OrdinalIgnoreCase);

    public LanguagePreparationService(
        I18nService i18nService,
        IHttpClientFactory httpClientFactory,
        ILogger<LanguagePreparationService> logger)
    {
        _i18nService = i18nService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public LanguagePreparationJobSnapshot Queue(string language, Uri targetUri)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        var key = BuildJobKey(normalizedLanguage, targetUri);
        if (_jobIdsByKey.TryGetValue(key, out var existingJobId)
            && _jobs.TryGetValue(existingJobId, out var existingJob)
            && !existingJob.IsExpired)
        {
            var existingSnapshot = existingJob.ToSnapshot();
            if (!existingSnapshot.IsFailed)
            {
                return existingSnapshot;
            }
        }

        var job = new LanguagePreparationJob(Guid.NewGuid().ToString("N"), normalizedLanguage, targetUri, key);
        _jobs[job.JobId] = job;
        _jobIdsByKey[key] = job.JobId;
        _ = Task.Run(() => RunJobAsync(job, CancellationToken.None));
        return job.ToSnapshot();
    }

    public LanguagePreparationJobSnapshot? GetJob(string? jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return null;
        }

        return _jobs.TryGetValue(jobId.Trim(), out var job)
            ? job.ToSnapshot()
            : null;
    }

    public async Task<LanguagePreparationJobSnapshot> PrepareNowAsync(
        string language,
        Uri targetUri,
        CancellationToken cancellationToken)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        var job = new LanguagePreparationJob(Guid.NewGuid().ToString("N"), normalizedLanguage, targetUri, BuildJobKey(normalizedLanguage, targetUri));
        await RunJobAsync(job, cancellationToken);
        return job.ToSnapshot();
    }

    private async Task RunJobAsync(LanguagePreparationJob job, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        job.MarkRunning();
        CodeWfLogger.Info(
            $"语言跳转后台准备开始。jobId={job.JobId}; language={job.Language}; url={job.TargetUri}.",
            log2UI: false,
            log2File: false,
            log2Console: true);

        try
        {
            var resourceResult = _i18nService.PrepareLanguageResource(job.Language);
            if (!resourceResult.IsDefaultLanguage && !resourceResult.HasResourceFile)
            {
                throw new InvalidOperationException($"Language resource was not generated. language={job.Language}.");
            }

            var pagePrepared = await PrepareTargetPageAsync(job.TargetUri, cancellationToken);
            if (!pagePrepared)
            {
                throw new InvalidOperationException($"Target language page was not prepared. language={job.Language}; url={job.TargetUri}.");
            }

            stopwatch.Stop();
            job.MarkCompleted(resourceResult, pagePrepared);

            CodeWfLogger.Info(
                $"语言跳转后台准备完成。jobId={job.JobId}; language={job.Language}; createdResource={resourceResult.CreatedResourceFile}; pagePrepared={pagePrepared}; elapsedMs={stopwatch.ElapsedMilliseconds}; url={job.TargetUri}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            job.MarkFailed("Language preparation was canceled.");
            CodeWfLogger.Warn(
                $"语言跳转后台准备取消。jobId={job.JobId}; language={job.Language}; elapsedMs={stopwatch.ElapsedMilliseconds}; url={job.TargetUri}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            job.MarkFailed(ex.Message);
            CodeWfLogger.Error(
                $"语言跳转后台准备失败。jobId={job.JobId}; language={job.Language}; elapsedMs={stopwatch.ElapsedMilliseconds}; url={job.TargetUri}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogError(ex, "Failed to prepare language content in background. jobId={JobId}", job.JobId);
        }
    }

    private async Task<bool> PrepareTargetPageAsync(Uri targetUri, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(30);

        using var request = new HttpRequestMessage(HttpMethod.Get, targetUri);
        request.Headers.TryAddWithoutValidation("Accept", "text/html");
        request.Headers.TryAddWithoutValidation("X-CodeWF-Language-Prepare", "1");

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private static string BuildJobKey(string language, Uri targetUri) =>
        $"{language}|{targetUri.GetLeftPart(UriPartial.Path)}{targetUri.Query}";

    private sealed class LanguagePreparationJob
    {
        private readonly object _gate = new();
        private readonly DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
        private DateTimeOffset _updatedAt = DateTimeOffset.UtcNow;
        private string _state = "queued";
        private string? _errorMessage;
        private bool _pagePrepared;
        private bool _hadLanguageResource;
        private bool _hasLanguageResource;
        private bool _createdLanguageResource;
        private int _stringCount;
        private int _textMapCount;

        public LanguagePreparationJob(string jobId, string language, Uri targetUri, string key)
        {
            JobId = jobId;
            Language = language;
            TargetUri = targetUri;
            Key = key;
        }

        public string JobId { get; }
        public string Language { get; }
        public Uri TargetUri { get; }
        public string Key { get; }
        public bool IsExpired => DateTimeOffset.UtcNow - _updatedAt > TimeSpan.FromMinutes(30);

        public void MarkRunning()
        {
            lock (_gate)
            {
                _state = "running";
                _updatedAt = DateTimeOffset.UtcNow;
            }
        }

        public void MarkCompleted(LanguageResourcePrepareResult resourceResult, bool pagePrepared)
        {
            lock (_gate)
            {
                _state = "completed";
                _pagePrepared = pagePrepared;
                _hadLanguageResource = resourceResult.HadResourceFile;
                _hasLanguageResource = resourceResult.HasResourceFile;
                _createdLanguageResource = resourceResult.CreatedResourceFile;
                _stringCount = resourceResult.StringCount;
                _textMapCount = resourceResult.TextMapCount;
                _updatedAt = DateTimeOffset.UtcNow;
            }
        }

        public void MarkFailed(string errorMessage)
        {
            lock (_gate)
            {
                _state = "failed";
                _errorMessage = errorMessage;
                _updatedAt = DateTimeOffset.UtcNow;
            }
        }

        public LanguagePreparationJobSnapshot ToSnapshot()
        {
            lock (_gate)
            {
                return new LanguagePreparationJobSnapshot(
                    JobId,
                    Language,
                    TargetUri.ToString(),
                    _state,
                    string.Equals(_state, "completed", StringComparison.OrdinalIgnoreCase),
                    string.Equals(_state, "failed", StringComparison.OrdinalIgnoreCase),
                    _hadLanguageResource,
                    _hasLanguageResource,
                    _createdLanguageResource,
                    _pagePrepared,
                    _stringCount,
                    _textMapCount,
                    _errorMessage,
                    _createdAt,
                    _updatedAt);
            }
        }
    }
}

public sealed record LanguagePreparationJobSnapshot(
    string JobId,
    string Language,
    string TargetUrl,
    string State,
    bool IsCompleted,
    bool IsFailed,
    bool HadLanguageResource,
    bool HasLanguageResource,
    bool CreatedLanguageResource,
    bool PagePrepared,
    int StringCount,
    int TextMapCount,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
