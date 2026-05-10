using System.Collections.Concurrent;
using System.Diagnostics;
using CodeWfLogger = CodeWF.Log.Core.Logger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Tmt.V20180321;
using TencentCloud.Tmt.V20180321.Models;
using WebApp.Options;

namespace WebApp.Services;

public sealed class TencentFanyiContentTranslationService : IContentTranslationService
{
    // 腾讯按请求计费且有频率限制：这里同时做内存缓存、并发合并和全局限速。
    // 相同目标语言+相同文本只请求一次；并发遇到同一文本时共用同一个 Lazy<Task>。
    private static readonly SemaphoreSlim RequestRateLimitLock = new(1, 1);
    private static readonly ConcurrentDictionary<TencentTranslationCacheKey, string> TranslationCache = new();
    private static readonly ConcurrentDictionary<TencentTranslationCacheKey, Lazy<Task<string?>>> PendingTranslations = new();
    private static readonly ConcurrentQueue<TencentTranslationCacheKey> TranslationCacheOrder = new();
    private static DateTimeOffset NextRequestTime = DateTimeOffset.MinValue;

    private readonly record struct TencentTranslationCacheKey(string TargetCode, string Source);

    private static readonly Dictionary<string, string> TencentLanguageMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["zh-cn"] = "zh",
        ["zh-tw"] = "zh-TW",
        ["en"] = "en",
        ["ja"] = "ja",
        ["ko"] = "ko",
        ["fr"] = "fr",
        ["de"] = "de",
        ["es"] = "es",
        ["pt"] = "pt",
        ["it"] = "it",
        ["ru"] = "ru",
        ["ar"] = "ar",
        ["th"] = "th",
        ["vi"] = "vi",
        ["id"] = "id",
        ["ms"] = "ms",
        ["tr"] = "tr"
    };

    private readonly IOptions<TencentFanyiOption> _options;
    private readonly ILogger<TencentFanyiContentTranslationService> _logger;

    public TencentFanyiContentTranslationService(
        IOptions<TencentFanyiOption> options,
        ILogger<TencentFanyiContentTranslationService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<string?> TranslateAsync(
        string source,
        LanguageInfo targetLanguage,
        ContentTranslationKind kind,
        string? resourceName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return source;
        }

        var resource = NormalizeResourceName(resourceName);
        var option = _options.Value;
        var appId = GetAppId(option);
        var key = GetKey(option);
        if (string.IsNullOrWhiteSpace(appId)
            || string.IsNullOrWhiteSpace(key)
            || IsPlaceholder(appId)
            || IsPlaceholder(key))
        {
            CodeWfLogger.Warn(
                $"TencentFanyi translation is skipped because TencentFanyi:AppID and TencentFanyi:Key are not configured correctly. language={targetLanguage.Code}; kind={kind}; resource={resource}; inputChars={source.Length}; hasAppID={!string.IsNullOrWhiteSpace(appId)}; hasKey={!string.IsNullOrWhiteSpace(key)}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogWarning(
                "TencentFanyi translation is skipped because AppID/Key are not configured correctly. Language={Language}; Kind={Kind}; Resource={Resource}; InputChars={InputChars}; HasAppID={HasAppID}; HasKey={HasKey}.",
                targetLanguage.Code,
                kind,
                resource,
                source.Length,
                !string.IsNullOrWhiteSpace(appId),
                !string.IsNullOrWhiteSpace(key));
            LogTencentCredentialHint(appId, key);
            return null;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            LogTencentCredentialHint(appId, key);
            var endpoint = GetEndpoint(option);
            var protocol = GetProtocol(option);
            var region = GetRegion(option);
            var projectId = GetProjectId(option);
            var maxRetries = GetMaxRetries(option);
            var maxChars = GetMaxCharsPerRequest(option);
            var maxRequestsPerSecond = GetMaxRequestsPerSecond(option);
            var memoryCacheLimit = GetMemoryCacheLimit(option);
            var client = CreateClient(option, appId, key);
            CodeWfLogger.Info(
                $"TencentFanyi translation started. language={targetLanguage.Code}; tencentTarget={GetTencentLanguageCode(targetLanguage)}; kind={kind}; resource={resource}; inputChars={source.Length}; endpoint={endpoint}; protocol={protocol}; region={region}; projectId={projectId}; maxCharsPerRequest={maxChars}; maxRetries={maxRetries}; maxRequestsPerSecond={maxRequestsPerSecond}; memoryCacheLimit={memoryCacheLimit}; appIdLength={appId.Length}; keyLength={key.Length}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "TencentFanyi translation started. Language={Language}; TencentTarget={TencentTarget}; Kind={Kind}; Resource={Resource}; InputChars={InputChars}; Endpoint={Endpoint}; Protocol={Protocol}; Region={Region}; ProjectId={ProjectId}; MaxCharsPerRequest={MaxCharsPerRequest}; MaxRetries={MaxRetries}; MaxRequestsPerSecond={MaxRequestsPerSecond}; MemoryCacheLimit={MemoryCacheLimit}; AppIdLength={AppIdLength}; KeyLength={KeyLength}.",
                targetLanguage.Code,
                GetTencentLanguageCode(targetLanguage),
                kind,
                resource,
                source.Length,
                endpoint,
                protocol,
                region,
                projectId,
                maxChars,
                maxRetries,
                maxRequestsPerSecond,
                memoryCacheLimit,
                appId.Length,
                key.Length);

            var translated = await StructuredContentTranslation.TranslateAsync(
                source,
                targetLanguage,
                kind,
                resource,
                maxChars,
                (chunk, language, chunkResource, chunkIndex, chunkCount, token) => TranslateChunkAsync(
                    client,
                    option,
                    chunk,
                    language,
                    chunkResource,
                    chunkIndex,
                    chunkCount,
                    token),
                cancellationToken);

            stopwatch.Stop();
            CodeWfLogger.Info(
                $"TencentFanyi translation completed. language={targetLanguage.Code}; kind={kind}; resource={resource}; outputChars={translated?.Length ?? 0}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "TencentFanyi translation completed. Language={Language}; Kind={Kind}; Resource={Resource}; OutputChars={OutputChars}; ElapsedMs={ElapsedMs}.",
                targetLanguage.Code,
                kind,
                resource,
                translated?.Length ?? 0,
                stopwatch.ElapsedMilliseconds);
            return string.IsNullOrWhiteSpace(translated) ? null : translated;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            CodeWfLogger.Error(
                $"TencentFanyi translation failed. language={targetLanguage.Code}; kind={kind}; resource={resource}; inputChars={source.Length}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogError(
                ex,
                "Failed to translate content with TencentFanyi. Language={Language}; Kind={Kind}; Resource={Resource}; InputChars={InputChars}; ElapsedMs={ElapsedMs}.",
                targetLanguage.Code,
                kind,
                resource,
                source.Length,
                stopwatch.ElapsedMilliseconds);
            return null;
        }
    }

    private async Task<string?> TranslateChunkAsync(
        TmtClient client,
        TencentFanyiOption option,
        string source,
        LanguageInfo targetLanguage,
        string resource,
        int chunkIndex,
        int chunkCount,
        CancellationToken cancellationToken)
    {
        var targetCode = GetTencentLanguageCode(targetLanguage);
        var cacheKey = new TencentTranslationCacheKey(targetCode, source);
        var allowPersistentCache = ShouldUsePersistentCache(source);
        if (allowPersistentCache && TryGetCachedTranslation(option, cacheKey, out var cachedTranslation))
        {
            CodeWfLogger.Info(
                $"TencentFanyi API cache hit. language={targetLanguage.Code}; tencentTarget={targetCode}; resource={resource}; inputChars={source.Length}; outputChars={cachedTranslation.Length}; cacheSize={TranslationCache.Count}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "TencentFanyi API cache hit. Language={Language}; TencentTarget={TencentTarget}; Resource={Resource}; InputChars={InputChars}; OutputChars={OutputChars}; CacheSize={CacheSize}.",
                targetLanguage.Code,
                targetCode,
                resource,
                source.Length,
                cachedTranslation.Length,
                TranslationCache.Count);
            return cachedTranslation;
        }

        // 同一批资源翻译时可能出现重复文本。GetOrAdd + Lazy<Task> 可以把并发请求折叠成一次真实 API 调用。
        var lazy = PendingTranslations.GetOrAdd(
            cacheKey,
            _ => new Lazy<Task<string?>>(
                async () =>
                {
                    var translated = await TranslateChunkFromApiAsync(
                        client,
                        option,
                        source,
                        targetLanguage,
                        resource,
                        chunkIndex,
                        chunkCount,
                        targetCode,
                        cancellationToken);
                    if (allowPersistentCache && !string.IsNullOrWhiteSpace(translated))
                    {
                        AddCachedTranslation(option, cacheKey, translated);
                    }

                    return translated;
                },
                LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            return await lazy.Value;
        }
        finally
        {
            if (lazy.IsValueCreated && lazy.Value.IsCompleted)
            {
                PendingTranslations.TryRemove(cacheKey, out _);
            }
        }
    }

    private async Task<string?> TranslateChunkFromApiAsync(
        TmtClient client,
        TencentFanyiOption option,
        string source,
        LanguageInfo targetLanguage,
        string resource,
        int chunkIndex,
        int chunkCount,
        string targetCode,
        CancellationToken cancellationToken)
    {
        var maxRetries = GetMaxRetries(option);
        var networkTimeout = GetNetworkTimeout(option);
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            var requestStopwatch = Stopwatch.StartNew();
            try
            {
                await WaitForRateLimitAsync(option, targetLanguage, resource, chunkIndex, chunkCount, cancellationToken);
                requestStopwatch.Restart();
                CodeWfLogger.Info(
                    $"TencentFanyi API request started. language={targetLanguage.Code}; tencentTarget={targetCode}; resource={resource}; chunk={chunkIndex}/{chunkCount}; attempt={attempt + 1}/{maxRetries + 1}; inputChars={source.Length}; endpoint={GetEndpoint(option)}; protocol={GetProtocol(option)}; region={GetRegion(option)}; projectId={GetProjectId(option)}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogInformation(
                    "TencentFanyi API request started. Language={Language}; TencentTarget={TencentTarget}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; Attempt={Attempt}/{AttemptCount}; InputChars={InputChars}; Endpoint={Endpoint}; Protocol={Protocol}; Region={Region}; ProjectId={ProjectId}.",
                    targetLanguage.Code,
                    targetCode,
                    resource,
                    chunkIndex,
                    chunkCount,
                    attempt + 1,
                    maxRetries + 1,
                    source.Length,
                    GetEndpoint(option),
                    GetProtocol(option),
                    GetRegion(option),
                    GetProjectId(option));

                var request = new TextTranslateRequest
                {
                    SourceText = source,
                    Source = "zh",
                    Target = targetCode,
                    ProjectId = GetProjectId(option)
                };
                var response = await client.TextTranslate(request).WaitAsync(networkTimeout, cancellationToken);
                requestStopwatch.Stop();
                var translated = response.TargetText;
                if (!string.IsNullOrWhiteSpace(translated))
                {
                    CodeWfLogger.Info(
                        $"TencentFanyi API request completed. language={targetLanguage.Code}; tencentTarget={targetCode}; resource={resource}; chunk={chunkIndex}/{chunkCount}; attempt={attempt + 1}/{maxRetries + 1}; outputChars={translated.Length}; usedAmount={response.UsedAmount?.ToString() ?? "unknown"}; requestId={response.RequestId}; elapsedMs={requestStopwatch.ElapsedMilliseconds}.",
                        log2UI: false,
                        log2File: false,
                        log2Console: true);
                    _logger.LogInformation(
                        "TencentFanyi API request completed. Language={Language}; TencentTarget={TencentTarget}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; Attempt={Attempt}/{AttemptCount}; OutputChars={OutputChars}; UsedAmount={UsedAmount}; RequestId={RequestId}; ElapsedMs={ElapsedMs}.",
                        targetLanguage.Code,
                        targetCode,
                        resource,
                        chunkIndex,
                        chunkCount,
                        attempt + 1,
                        maxRetries + 1,
                        translated.Length,
                        response.UsedAmount,
                        response.RequestId,
                        requestStopwatch.ElapsedMilliseconds);
                    return translated;
                }

                if (attempt >= maxRetries)
                {
                    _logger.LogWarning(
                        "TencentFanyi returned no translation result. Language={Language}; TencentTarget={TencentTarget}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; RequestId={RequestId}",
                        targetLanguage.Code,
                        targetCode,
                        resource,
                        chunkIndex,
                        chunkCount,
                        response.RequestId);
                    return null;
                }
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                requestStopwatch.Stop();
                CodeWfLogger.Warn(
                    $"TencentFanyi API request failed. language={targetLanguage.Code}; tencentTarget={targetCode}; resource={resource}; chunk={chunkIndex}/{chunkCount}; attempt={attempt + 1}/{maxRetries + 1}; exceptionType={ex.GetType().Name}; message={ex.Message}; elapsedMs={requestStopwatch.ElapsedMilliseconds}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogWarning(
                    ex,
                    "TencentFanyi API request failed. Language={Language}; TencentTarget={TencentTarget}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; Attempt={Attempt}/{AttemptCount}; ElapsedMs={ElapsedMs}.",
                    targetLanguage.Code,
                    targetCode,
                    resource,
                    chunkIndex,
                    chunkCount,
                    attempt + 1,
                    maxRetries + 1,
                    requestStopwatch.ElapsedMilliseconds);

                if (attempt >= maxRetries)
                {
                    throw new InvalidOperationException(
                        $"TencentFanyi API request failed. language={targetLanguage.Code}; tencentTarget={targetCode}; resource={resource}; chunk={chunkIndex}/{chunkCount}; exceptionType={ex.GetType().Name}; message={ex.Message}.",
                        ex);
                }

                var retryDelay = GetRetryDelay(ex, attempt);
                CodeWfLogger.Warn(
                    $"TencentFanyi API request will retry. language={targetLanguage.Code}; tencentTarget={targetCode}; resource={resource}; chunk={chunkIndex}/{chunkCount}; nextAttempt={attempt + 2}/{maxRetries + 1}; retryDelayMs={retryDelay.TotalMilliseconds:0}; reason={ex.Message}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogWarning(
                    "TencentFanyi API request will retry. Language={Language}; TencentTarget={TencentTarget}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; NextAttempt={NextAttempt}/{AttemptCount}; RetryDelayMs={RetryDelayMs}; Reason={Reason}.",
                    targetLanguage.Code,
                    targetCode,
                    resource,
                    chunkIndex,
                    chunkCount,
                    attempt + 2,
                    maxRetries + 1,
                    retryDelay.TotalMilliseconds,
                    ex.Message);
                await Task.Delay(retryDelay, cancellationToken);
            }
        }

        return null;
    }

    private static bool ShouldUsePersistentCache(string source) =>
        !LooksLikeIndexedJsonBatchPayload(source);

    private static bool LooksLikeIndexedJsonBatchPayload(string source)
    {
        if (source.Length <= 6)
        {
            return false;
        }

        for (var i = 0; i < 6; i++)
        {
            if (!char.IsDigit(source[i]))
            {
                return false;
            }
        }

        return char.IsWhiteSpace(source[6]);
    }

    private static bool TryGetCachedTranslation(
        TencentFanyiOption option,
        TencentTranslationCacheKey cacheKey,
        out string translated)
    {
        translated = string.Empty;
        return GetMemoryCacheLimit(option) > 0
            && TranslationCache.TryGetValue(cacheKey, out translated!);
    }

    private static void AddCachedTranslation(
        TencentFanyiOption option,
        TencentTranslationCacheKey cacheKey,
        string translated)
    {
        var cacheLimit = GetMemoryCacheLimit(option);
        if (cacheLimit <= 0)
        {
            return;
        }

        if (TranslationCache.TryAdd(cacheKey, translated))
        {
            TranslationCacheOrder.Enqueue(cacheKey);
            TrimTranslationCache(cacheLimit);
            return;
        }

        TranslationCache[cacheKey] = translated;
    }

    private static void TrimTranslationCache(int cacheLimit)
    {
        while (TranslationCache.Count > cacheLimit
               && TranslationCacheOrder.TryDequeue(out var oldKey))
        {
            TranslationCache.TryRemove(oldKey, out _);
        }
    }

    private static async Task WaitForRateLimitAsync(
        TencentFanyiOption option,
        LanguageInfo targetLanguage,
        string resource,
        int chunkIndex,
        int chunkCount,
        CancellationToken cancellationToken)
    {
        // SDK 本身只负责发送请求，不会替业务做全局 QPS 控制；这里串行维护下一次允许请求的时间。
        var maxRequestsPerSecond = GetMaxRequestsPerSecond(option);
        var interval = TimeSpan.FromMilliseconds(Math.Ceiling(1000d / maxRequestsPerSecond));

        await RequestRateLimitLock.WaitAsync(cancellationToken);
        try
        {
            var now = DateTimeOffset.UtcNow;
            if (NextRequestTime > now)
            {
                var delay = NextRequestTime - now;
                if (delay.TotalMilliseconds >= 1000)
                {
                    CodeWfLogger.Info(
                        $"TencentFanyi API request throttled. language={targetLanguage.Code}; resource={resource}; chunk={chunkIndex}/{chunkCount}; delayMs={delay.TotalMilliseconds:0}; maxRequestsPerSecond={maxRequestsPerSecond}.",
                        log2UI: false,
                        log2File: false,
                        log2Console: true);
                }

                await Task.Delay(delay, cancellationToken);
            }

            NextRequestTime = DateTimeOffset.UtcNow + interval;
        }
        finally
        {
            RequestRateLimitLock.Release();
        }
    }

    private static TmtClient CreateClient(TencentFanyiOption option, string appId, string key)
    {
        var httpProfile = new HttpProfile
        {
            Endpoint = GetEndpoint(option),
            Protocol = GetProtocol(option),
            Timeout = (int)GetNetworkTimeout(option).TotalSeconds
        };
        var clientProfile = new ClientProfile
        {
            HttpProfile = httpProfile
        };
        var credential = new Credential
        {
            SecretId = appId,
            SecretKey = key
        };
        return new TmtClient(credential, GetRegion(option), clientProfile);
    }

    private static string? GetAppId(TencentFanyiOption option)
    {
        if (IsConfiguredValue(option.AppID))
        {
            return option.AppID!.Trim();
        }

        if (IsConfiguredValue(option.SecretId))
        {
            return option.SecretId!.Trim();
        }

        return null;
    }

    private static string? GetKey(TencentFanyiOption option)
    {
        if (IsConfiguredValue(option.Key))
        {
            return option.Key!.Trim();
        }

        if (IsConfiguredValue(option.SecretKey))
        {
            return option.SecretKey!.Trim();
        }

        return null;
    }

    private void LogTencentCredentialHint(string? appId, string? key)
    {
        if (!LooksLikeTencentNumericAppId(appId) && !LooksLikeTencentSecretId(key))
        {
            return;
        }

        CodeWfLogger.Warn(
            "TencentFanyi credentials may be misconfigured. Configure TencentFanyi:AppID with the Tencent Cloud API credential ID and TencentFanyi:Key with the API credential key.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        _logger.LogWarning(
            "TencentFanyi credentials may be misconfigured. Configure AppID with the Tencent Cloud API credential ID and Key with the API credential key. HasAppID={HasAppID}; HasKey={HasKey}; AppIDIsNumeric={AppIDIsNumeric}; KeyLooksLikeCredentialId={KeyLooksLikeCredentialId}.",
            !string.IsNullOrWhiteSpace(appId),
            !string.IsNullOrWhiteSpace(key),
            LooksLikeTencentNumericAppId(appId),
            LooksLikeTencentSecretId(key));
    }

    private static bool IsConfiguredValue(string? value) =>
        !string.IsNullOrWhiteSpace(value) && !IsPlaceholder(value);

    private static bool LooksLikeTencentNumericAppId(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().All(char.IsDigit);

    private static bool LooksLikeTencentSecretId(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().StartsWith("AKID", StringComparison.OrdinalIgnoreCase);

    private static bool IsPlaceholder(string? value) =>
        string.Equals(value?.Trim(), "your app id", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value?.Trim(), "your tencent app id", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value?.Trim(), "your secret id", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value?.Trim(), "your tencent secret id", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value?.Trim(), "your key", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value?.Trim(), "your tencent key", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value?.Trim(), "your secret key", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value?.Trim(), "your tencent secret key", StringComparison.OrdinalIgnoreCase);

    private static string GetEndpoint(TencentFanyiOption option)
    {
        var endpoint = string.IsNullOrWhiteSpace(option.Endpoint)
            ? TencentFanyiOption.DefaultEndpoint
            : option.Endpoint.Trim();
        return Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
            ? uri.Host
            : endpoint.TrimEnd('/');
    }

    private static string GetProtocol(TencentFanyiOption option)
    {
        if (!string.IsNullOrWhiteSpace(option.Endpoint)
            && Uri.TryCreate(option.Endpoint.Trim(), UriKind.Absolute, out var uri)
            && string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            return HttpProfile.REQ_HTTP;
        }

        return HttpProfile.REQ_HTTPS;
    }

    private static string GetRegion(TencentFanyiOption option) =>
        string.IsNullOrWhiteSpace(option.Region) ? TencentFanyiOption.DefaultRegion : option.Region.Trim();

    private static long GetProjectId(TencentFanyiOption option) =>
        option.ProjectId.GetValueOrDefault(TencentFanyiOption.DefaultProjectId);

    private static TimeSpan GetNetworkTimeout(TencentFanyiOption option)
    {
        var seconds = option.NetworkTimeoutSeconds.GetValueOrDefault(TencentFanyiOption.DefaultNetworkTimeoutSeconds);
        if (seconds <= 0)
        {
            seconds = TencentFanyiOption.DefaultNetworkTimeoutSeconds;
        }

        return TimeSpan.FromSeconds(seconds);
    }

    private static int GetMaxRetries(TencentFanyiOption option)
    {
        var maxRetries = option.MaxRetries.GetValueOrDefault(TencentFanyiOption.DefaultMaxRetries);
        return maxRetries < 0 ? TencentFanyiOption.DefaultMaxRetries : maxRetries;
    }

    private static int GetMaxCharsPerRequest(TencentFanyiOption option)
    {
        var maxChars = option.MaxCharsPerRequest.GetValueOrDefault(TencentFanyiOption.DefaultMaxCharsPerRequest);
        return Math.Clamp(maxChars, 500, TencentFanyiOption.DefaultMaxCharsPerRequest);
    }

    private static int GetMaxRequestsPerSecond(TencentFanyiOption option)
    {
        var maxRequestsPerSecond = option.MaxRequestsPerSecond.GetValueOrDefault(TencentFanyiOption.DefaultMaxRequestsPerSecond);
        return Math.Clamp(maxRequestsPerSecond, 1, 100);
    }

    private static int GetMemoryCacheLimit(TencentFanyiOption option)
    {
        var cacheLimit = option.MemoryCacheLimit.GetValueOrDefault(TencentFanyiOption.DefaultMemoryCacheLimit);
        return Math.Clamp(cacheLimit, 0, 100000);
    }

    private static TimeSpan GetRetryDelay(Exception exception, int attempt) =>
        IsRateLimitException(exception)
            ? TimeSpan.FromSeconds(Math.Min(10, 2 * (attempt + 1)))
            : TimeSpan.FromMilliseconds(200 * (attempt + 1));

    private static bool IsRateLimitException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains("超过了每秒频率上限", StringComparison.Ordinal)
                || current.Message.Contains("RequestLimitExceeded", StringComparison.OrdinalIgnoreCase)
                || current.Message.Contains("RateLimit", StringComparison.OrdinalIgnoreCase)
                || current.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetTencentLanguageCode(LanguageInfo targetLanguage)
    {
        var code = targetLanguage.Code;
        if (TencentLanguageMap.TryGetValue(code, out var mapped))
        {
            return mapped;
        }

        var dashIndex = code.IndexOf('-', StringComparison.Ordinal);
        var neutralCode = dashIndex > 0 ? code[..dashIndex] : code;
        return TencentLanguageMap.TryGetValue(neutralCode, out mapped) ? mapped : neutralCode;
    }

    private static string NormalizeResourceName(string? resourceName) =>
        string.IsNullOrWhiteSpace(resourceName)
            ? "unknown"
            : resourceName.Trim().Replace('\r', ' ').Replace('\n', ' ');
}
