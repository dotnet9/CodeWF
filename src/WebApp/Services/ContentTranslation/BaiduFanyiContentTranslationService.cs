using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CodeWfLogger = CodeWF.Log.Core.Logger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Options;

namespace WebApp.Services;

public sealed class BaiduFanyiContentTranslationService : IContentTranslationService
{
    private static readonly Dictionary<string, string> BaiduLanguageMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["zh-cn"] = "zh",
        ["zh-tw"] = "cht",
        ["en"] = "en",
        ["ja"] = "jp",
        ["ko"] = "kor",
        ["fr"] = "fra",
        ["de"] = "de",
        ["es"] = "spa",
        ["pt"] = "pt",
        ["it"] = "it",
        ["ru"] = "ru",
        ["ar"] = "ara",
        ["th"] = "th",
        ["vi"] = "vie",
        ["id"] = "id",
        ["ms"] = "may",
        ["el"] = "el",
        ["nl"] = "nl",
        ["pl"] = "pl",
        ["tr"] = "tr",
        ["cs"] = "cs",
        ["da"] = "dan",
        ["fi"] = "fin",
        ["sv"] = "swe",
        ["hu"] = "hu"
    };

    private readonly IOptions<BaiduFanyiOption> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BaiduFanyiContentTranslationService> _logger;

    public BaiduFanyiContentTranslationService(
        IOptions<BaiduFanyiOption> options,
        IHttpClientFactory httpClientFactory,
        ILogger<BaiduFanyiContentTranslationService> logger)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
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
        if (string.IsNullOrWhiteSpace(option.AppID)
            || string.IsNullOrWhiteSpace(option.Key)
            || string.Equals(option.AppID.Trim(), "your app id", StringComparison.OrdinalIgnoreCase)
            || string.Equals(option.Key.Trim(), "your key", StringComparison.OrdinalIgnoreCase))
        {
            CodeWfLogger.Warn(
                $"BaiduFanyi translation is skipped because BaiduFanyi:AppID or BaiduFanyi:Key is not configured. language={targetLanguage.Code}; kind={kind}; resource={resource}; inputChars={source.Length}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return null;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var endpoint = string.IsNullOrWhiteSpace(option.Endpoint)
                ? "https://fanyi-api.baidu.com/api/trans/vip/translate"
                : option.Endpoint.Trim();
            var maxRetries = GetMaxRetries(option);
            var maxChars = GetMaxCharsPerRequest(option);
            CodeWfLogger.Info(
                $"BaiduFanyi translation started. language={targetLanguage.Code}; baiduTo={GetBaiduLanguageCode(targetLanguage)}; kind={kind}; resource={resource}; inputChars={source.Length}; endpoint={endpoint}; maxCharsPerRequest={maxChars}; maxRetries={maxRetries}; appIdLength={option.AppID.Trim().Length}; keyLength={option.Key.Trim().Length}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "BaiduFanyi translation started. Language={Language}; BaiduTo={BaiduTo}; Kind={Kind}; Resource={Resource}; InputChars={InputChars}; Endpoint={Endpoint}; MaxCharsPerRequest={MaxCharsPerRequest}; MaxRetries={MaxRetries}; AppIdLength={AppIdLength}; KeyLength={KeyLength}.",
                targetLanguage.Code,
                GetBaiduLanguageCode(targetLanguage),
                kind,
                resource,
                source.Length,
                endpoint,
                maxChars,
                maxRetries,
                option.AppID.Trim().Length,
                option.Key.Trim().Length);

            var translated = await StructuredContentTranslation.TranslateAsync(
                source,
                targetLanguage,
                kind,
                resource,
                maxChars,
                TranslateChunkAsync,
                cancellationToken);

            stopwatch.Stop();
            CodeWfLogger.Info(
                $"BaiduFanyi translation completed. language={targetLanguage.Code}; kind={kind}; resource={resource}; outputChars={translated?.Length ?? 0}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "BaiduFanyi translation completed. Language={Language}; Kind={Kind}; Resource={Resource}; OutputChars={OutputChars}; ElapsedMs={ElapsedMs}.",
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
                $"BaiduFanyi translation failed. language={targetLanguage.Code}; kind={kind}; resource={resource}; inputChars={source.Length}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogError(
                ex,
                "Failed to translate content with BaiduFanyi. Language={Language}; Kind={Kind}; Resource={Resource}; InputChars={InputChars}; ElapsedMs={ElapsedMs}.",
                targetLanguage.Code,
                kind,
                resource,
                source.Length,
                stopwatch.ElapsedMilliseconds);
            return null;
        }
    }

    private async Task<string?> TranslateChunkAsync(
        string source,
        LanguageInfo targetLanguage,
        string resource,
        int chunkIndex,
        int chunkCount,
        CancellationToken cancellationToken)
    {
        var option = _options.Value;
        var endpoint = string.IsNullOrWhiteSpace(option.Endpoint)
            ? "https://fanyi-api.baidu.com/api/trans/vip/translate"
            : option.Endpoint.Trim();
        var salt = Random.Shared.Next(100000, 999999999).ToString();
        var appId = option.AppID!.Trim();
        var key = option.Key!.Trim();
        var targetCode = GetBaiduLanguageCode(targetLanguage);
        var sign = CreateBaiduSign(appId, source, salt, key);
        var form = new Dictionary<string, string>
        {
            ["q"] = source,
            ["from"] = "zh",
            ["to"] = targetCode,
            ["appid"] = appId,
            ["salt"] = salt,
            ["sign"] = sign
        };

        var maxRetries = GetMaxRetries(option);
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            CodeWfLogger.Info(
                $"BaiduFanyi API request started. language={targetLanguage.Code}; baiduTo={targetCode}; resource={resource}; chunk={chunkIndex}/{chunkCount}; attempt={attempt + 1}/{maxRetries + 1}; inputChars={source.Length}; endpoint={endpoint}; salt={salt}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "BaiduFanyi API request started. Language={Language}; BaiduTo={BaiduTo}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; Attempt={Attempt}/{AttemptCount}; InputChars={InputChars}; Endpoint={Endpoint}; Salt={Salt}.",
                targetLanguage.Code,
                targetCode,
                resource,
                chunkIndex,
                chunkCount,
                attempt + 1,
                maxRetries + 1,
                source.Length,
                endpoint,
                salt);

            var requestStopwatch = Stopwatch.StartNew();
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new FormUrlEncodedContent(form)
            };

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(GetNetworkTimeout(option));
            var client = _httpClientFactory.CreateClient(nameof(BaiduFanyiContentTranslationService));
            using var response = await client.SendAsync(request, timeoutCts.Token);
            var responseText = await response.Content.ReadAsStringAsync(timeoutCts.Token);
            requestStopwatch.Stop();
            if (!response.IsSuccessStatusCode)
            {
                if (attempt < maxRetries)
                {
                    _logger.LogWarning(
                        "BaiduFanyi API HTTP failure. Language={Language}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; Attempt={Attempt}/{AttemptCount}; StatusCode={StatusCode}; ElapsedMs={ElapsedMs}; Body={Body}",
                        targetLanguage.Code,
                        resource,
                        chunkIndex,
                        chunkCount,
                        attempt + 1,
                        maxRetries + 1,
                        (int)response.StatusCode,
                        requestStopwatch.ElapsedMilliseconds,
                        responseText);
                    continue;
                }

                _logger.LogWarning(
                    "BaiduFanyi API HTTP failure. Language={Language}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; Attempt={Attempt}/{AttemptCount}; StatusCode={StatusCode}; ElapsedMs={ElapsedMs}; Body={Body}",
                    targetLanguage.Code,
                    resource,
                    chunkIndex,
                    chunkCount,
                    attempt + 1,
                    maxRetries + 1,
                    (int)response.StatusCode,
                    requestStopwatch.ElapsedMilliseconds,
                    responseText);
                return null;
            }

            var translated = ParseBaiduResponse(responseText);
            if (!string.IsNullOrWhiteSpace(translated))
            {
                CodeWfLogger.Info(
                    $"BaiduFanyi API request completed. language={targetLanguage.Code}; baiduTo={targetCode}; resource={resource}; chunk={chunkIndex}/{chunkCount}; attempt={attempt + 1}/{maxRetries + 1}; outputChars={translated.Length}; elapsedMs={requestStopwatch.ElapsedMilliseconds}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogInformation(
                    "BaiduFanyi API request completed. Language={Language}; BaiduTo={BaiduTo}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; Attempt={Attempt}/{AttemptCount}; OutputChars={OutputChars}; ElapsedMs={ElapsedMs}.",
                    targetLanguage.Code,
                    targetCode,
                    resource,
                    chunkIndex,
                    chunkCount,
                    attempt + 1,
                    maxRetries + 1,
                    translated.Length,
                    requestStopwatch.ElapsedMilliseconds);
                return translated;
            }

            var error = ReadBaiduError(responseText);
            if (error is not null)
            {
                CodeWfLogger.Warn(
                    $"BaiduFanyi API returned error. language={targetLanguage.Code}; baiduTo={targetCode}; resource={resource}; chunk={chunkIndex}/{chunkCount}; attempt={attempt + 1}/{maxRetries + 1}; errorCode={error.Value.Code}; errorMessage={error.Value.Message}; elapsedMs={requestStopwatch.ElapsedMilliseconds}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogWarning(
                    "BaiduFanyi API returned error. Language={Language}; BaiduTo={BaiduTo}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; Attempt={Attempt}/{AttemptCount}; ErrorCode={ErrorCode}; ErrorMessage={ErrorMessage}; ElapsedMs={ElapsedMs}.",
                    targetLanguage.Code,
                    targetCode,
                    resource,
                    chunkIndex,
                    chunkCount,
                    attempt + 1,
                    maxRetries + 1,
                    error.Value.Code,
                    error.Value.Message,
                    requestStopwatch.ElapsedMilliseconds);
                throw new InvalidOperationException(
                    $"BaiduFanyi API returned error. language={targetLanguage.Code}; baiduTo={targetCode}; resource={resource}; chunk={chunkIndex}/{chunkCount}; errorCode={error.Value.Code}; errorMessage={error.Value.Message}.");
            }

            if (attempt >= maxRetries)
            {
                _logger.LogWarning(
                    "BaiduFanyi returned no translation result. Language={Language}; To={To}; Resource={Resource}; Chunk={ChunkIndex}/{ChunkCount}; Body={Body}",
                    targetLanguage.Code,
                    targetCode,
                    resource,
                    chunkIndex,
                    chunkCount,
                    responseText);
                return null;
            }
        }

        return null;
    }

    private static string? ParseBaiduResponse(string responseText)
    {
        using var document = JsonDocument.Parse(responseText, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        var root = document.RootElement;
        if (TryGetProperty(root, "error_code", out _) || TryGetProperty(root, "error_msg", out _))
        {
            return null;
        }

        if (!TryGetProperty(root, "trans_result", out var transResult)
            && TryGetProperty(root, "result", out var result))
        {
            TryGetProperty(result, "trans_result", out transResult);
        }

        if (transResult.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var values = transResult.EnumerateArray()
            .Select(static item => TryGetProperty(item, "dst", out var dst) ? dst.GetString() : null)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .ToList();
        return values.Count == 0 ? null : string.Join('\n', values);
    }

    private static (string Code, string Message)? ReadBaiduError(string responseText)
    {
        try
        {
            using var document = JsonDocument.Parse(responseText, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });

            var root = document.RootElement;
            var hasCode = TryGetProperty(root, "error_code", out var code);
            var hasMessage = TryGetProperty(root, "error_msg", out var message);
            if (!hasCode && !hasMessage)
            {
                return null;
            }

            return (
                code.ValueKind == JsonValueKind.String ? code.GetString() ?? string.Empty : code.ToString(),
                message.ValueKind == JsonValueKind.String ? message.GetString() ?? string.Empty : message.ToString());
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static string CreateBaiduSign(string appId, string query, string salt, string key)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes($"{appId}{query}{salt}{key}"));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string GetBaiduLanguageCode(LanguageInfo targetLanguage)
    {
        var code = targetLanguage.Code;
        if (BaiduLanguageMap.TryGetValue(code, out var mapped))
        {
            return mapped;
        }

        var dashIndex = code.IndexOf('-', StringComparison.Ordinal);
        var neutralCode = dashIndex > 0 ? code[..dashIndex] : code;
        return BaiduLanguageMap.TryGetValue(neutralCode, out mapped) ? mapped : neutralCode;
    }

    private static TimeSpan GetNetworkTimeout(BaiduFanyiOption option)
    {
        var seconds = option.NetworkTimeoutSeconds.GetValueOrDefault(BaiduFanyiOption.DefaultNetworkTimeoutSeconds);
        if (seconds <= 0)
        {
            seconds = BaiduFanyiOption.DefaultNetworkTimeoutSeconds;
        }

        return TimeSpan.FromSeconds(seconds);
    }

    private static int GetMaxRetries(BaiduFanyiOption option)
    {
        var maxRetries = option.MaxRetries.GetValueOrDefault(BaiduFanyiOption.DefaultMaxRetries);
        return maxRetries < 0 ? BaiduFanyiOption.DefaultMaxRetries : maxRetries;
    }

    private static int GetMaxCharsPerRequest(BaiduFanyiOption option)
    {
        var maxChars = option.MaxCharsPerRequest.GetValueOrDefault(BaiduFanyiOption.DefaultMaxCharsPerRequest);
        return Math.Clamp(maxChars, 500, 6000);
    }

    private static string NormalizeResourceName(string? resourceName) =>
        string.IsNullOrWhiteSpace(resourceName)
            ? "unknown"
            : resourceName.Trim().Replace('\r', ' ').Replace('\n', ' ');
}
