using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using CodeWfLogger = CodeWF.Log.Core.Logger;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WebApp.Options;

namespace WebApp.Services;

public sealed class I18nService
{
    private static readonly string[] RequiredDefaultTextMapKeys =
    [
        "正在准备语言内容，请稍候...",
        "正在准备语言内容，请稍后..."
    ];

    private static readonly string[] TextDiscoveryRoots =
    [
        "Pages",
        "Views",
        "Components",
        "Models",
        "wwwroot/js"
    ];

    private static readonly string[] TextDiscoveryExtensions =
    [
        ".cshtml",
        ".cs",
        ".js"
    ];

    private static readonly Regex ChineseTextRegex = new(@"[\u3400-\u9fff]", RegexOptions.Compiled);
    private static readonly Regex HtmlTextRegex = new(@">(?<value>[^<>]*[\u3400-\u9fff][^<>]*)<", RegexOptions.Compiled);
    private static readonly Regex AttributeTextRegex = new(@"(?:placeholder|aria-label|title|alt|value)\s*=\s*""(?<value>[^""]*[\u3400-\u9fff][^""]*)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex DoubleQuotedTextRegex = new(@"""(?<value>(?:[^""\\]|\\.)*[\u3400-\u9fff](?:[^""\\]|\\.)*)""", RegexOptions.Compiled);
    private static readonly Regex SingleQuotedTextRegex = new(@"'(?<value>(?:[^'\\]|\\.)*[\u3400-\u9fff](?:[^'\\]|\\.)*)'", RegexOptions.Compiled);
    private static readonly Regex BacktickTextRegex = new(@"`(?<value>[^`]*[\u3400-\u9fff][^`]*)`", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly JsonSerializerOptions ReadJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static readonly JsonSerializerOptions WriteJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ConcurrentDictionary<string, I18nResource> _resources = new(StringComparer.OrdinalIgnoreCase);
    private readonly IOptions<SiteOption> _siteOption;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IContentTranslationService _translationService;
    private readonly ILogger<I18nService> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _resourceFileLocks = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyCollection<string>? _discoveredDefaultTexts;

    public I18nService(
        IOptions<SiteOption> siteOption,
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        IContentTranslationService translationService,
        ILogger<I18nService>? logger = null)
    {
        _siteOption = siteOption;
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        _translationService = translationService;
        _logger = logger ?? NullLogger<I18nService>.Instance;
    }

    public IReadOnlyList<LanguageInfo> Languages => RequestLanguage.SupportedLanguages;

    public IReadOnlyList<LanguageInfo> SeoLanguages => RequestLanguage.GetSeoLanguages(CurrentLanguage);

    public string CurrentLanguage => RequestLanguage.CurrentLanguage;

    public LanguageInfo CurrentLanguageInfo => RequestLanguage.CurrentLanguageInfo;

    public LanguageResourceStatus GetLanguageResourceStatus(string? language)
    {
        var normalized = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        var languageInfo = RequestLanguage.GetLanguage(normalized);
        var hasResourceFile = RequestLanguage.IsDefaultLanguage(normalized) || HasResourceFile(normalized);
        return new LanguageResourceStatus(
            normalized,
            languageInfo.NativeName,
            languageInfo.EnglishName,
            RequestLanguage.IsDefaultLanguage(normalized),
            hasResourceFile);
    }

    public async Task<LanguageResourcePrepareResult> PrepareLanguageResourceAsync(
        string? language,
        CancellationToken cancellationToken = default)
    {
        var before = GetLanguageResourceStatus(language);
        var stopwatch = Stopwatch.StartNew();
        CodeWfLogger.Info(
            $"i18n 语言资源预热开始。language={before.Code}; hasResourceFile={before.HasResourceFile}.",
            log2UI: false,
            log2File: false,
            log2Console: true);

        _resources.TryRemove(before.Code, out _);
        var resource = await GetResourceAsync(before.Code, cancellationToken);
        var after = GetLanguageResourceStatus(before.Code);

        stopwatch.Stop();
        if (!after.IsDefaultLanguage && !after.HasResourceFile)
        {
            CodeWfLogger.Warn(
                $"i18n 语言资源预热未生成文件，当前返回内容为默认资源回退。language={after.Code}; target={GetResourceFilePath(after.Code)}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogWarning(
                "I18n language resource file was not generated and fallback resource is being returned. Language={Language}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                after.Code,
                GetResourceFilePath(after.Code),
                stopwatch.ElapsedMilliseconds);
        }

        CodeWfLogger.Info(
            $"i18n 语言资源预热完成。language={after.Code}; hadResourceFile={before.HasResourceFile}; hasResourceFile={after.HasResourceFile}; strings={resource.Strings.Count}; textMap={resource.TextMap.Count}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
            log2UI: false,
            log2File: false,
            log2Console: true);

        return new LanguageResourcePrepareResult(
            after.Code,
            after.NativeName,
            after.EnglishName,
            after.IsDefaultLanguage,
            before.HasResourceFile,
            after.HasResourceFile,
            !before.HasResourceFile && after.HasResourceFile,
            resource.Strings.Count,
            resource.TextMap.Count);
    }

    public string T(string key) => T(key, key);

    public string T(string key, string fallback)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return fallback;
        }

        var resource = GetResource(CurrentLanguage);
        return resource.Strings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    public string Format(string key, string fallback, params object[] args)
    {
        var template = T(key, fallback);
        return args.Length == 0 ? template : string.Format(template, args);
    }

    public string Text(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text ?? string.Empty;
        }

        var resource = GetResource(CurrentLanguage);
        var start = 0;
        var end = text.Length - 1;
        while (start <= end && char.IsWhiteSpace(text[start]))
        {
            start++;
        }

        while (end >= start && char.IsWhiteSpace(text[end]))
        {
            end--;
        }

        var trimmed = text[start..(end + 1)];
        return resource.TextMap.TryGetValue(trimmed, out var value) && !string.IsNullOrWhiteSpace(value)
            ? $"{text[..start]}{value}{text[(end + 1)..]}"
            : text;
    }

    public string Url(string? url, string? language = null) => RequestLanguage.LocalizePath(url, language);

    public string SwitchUrl(string language)
    {
        var context = _httpContextAccessor.HttpContext;
        return context is null
            ? RequestLanguage.LocalizePath("/", language)
            : RequestLanguage.BuildSwitchUrl(context, language);
    }

    public string SwitchPath(string language)
    {
        var context = _httpContextAccessor.HttpContext;
        return context is null
            ? RequestLanguage.LocalizePath("/", language)
            : RequestLanguage.BuildSwitchPath(context, language);
    }

    public string GetClientResourceJson()
    {
        var resource = GetResource(CurrentLanguage);
        var payload = new ClientI18nResource(
            CurrentLanguage,
            RequestLanguage.SupportedLanguages.Select(static language => language.Code).ToArray(),
            resource.TextMap,
            resource.Strings,
            resource.Patterns);
        return JsonSerializer.Serialize(payload, WriteJsonOptions);
    }

    private I18nResource GetResource(string language)
    {
        var normalized = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        return _resources.GetOrAdd(normalized, LoadResource);
    }

    private async Task<I18nResource> GetResourceAsync(string language, CancellationToken cancellationToken)
    {
        var normalized = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        if (_resources.TryGetValue(normalized, out var cached))
        {
            return cached;
        }

        var resource = await LoadResourceAsync(normalized, cancellationToken);
        _resources[normalized] = resource;
        return resource;
    }

    private I18nResource LoadResource(string language)
    {
        if (string.Equals(language, RequestLanguage.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
        {
            return LoadDefaultResource();
        }

        var fallback = LoadDefaultResource();
        var parentLanguage = GetParentLanguage(language);
        var parent = parentLanguage is null
            ? new I18nResource()
            : LoadResourceFile(parentLanguage) ?? new I18nResource();
        var current = LoadResourceFile(language) ?? new I18nResource();

        return new I18nResource
        {
            Strings = Merge(Merge(fallback.Strings, parent.Strings), current.Strings),
            TextMap = Merge(Merge(fallback.TextMap, parent.TextMap), current.TextMap),
            Patterns = current.Patterns.Count > 0
                ? current.Patterns
                : parent.Patterns.Count > 0
                    ? parent.Patterns
                    : fallback.Patterns
        };
    }

    private async Task<I18nResource> LoadResourceAsync(string language, CancellationToken cancellationToken)
    {
        if (string.Equals(language, RequestLanguage.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
        {
            return await LoadDefaultResourceAsync(cancellationToken);
        }

        var fallback = await LoadDefaultResourceAsync(cancellationToken);
        var parentLanguage = GetParentLanguage(language);
        var parent = parentLanguage is null
            ? new I18nResource()
            : await LoadResourceFileAsync(parentLanguage, cancellationToken) ?? new I18nResource();
        var current = await LoadResourceFileAsync(language, cancellationToken)
            ?? await CreateResourceFileFromDefaultAsync(language, fallback, cancellationToken)
            ?? new I18nResource();
        current = await CompleteResourceFromDefaultAsync(language, fallback, parent, current, cancellationToken);

        return new I18nResource
        {
            Strings = Merge(Merge(fallback.Strings, parent.Strings), current.Strings),
            TextMap = Merge(Merge(fallback.TextMap, parent.TextMap), current.TextMap),
            Patterns = current.Patterns.Count > 0
                ? current.Patterns
                : parent.Patterns.Count > 0
                    ? parent.Patterns
                    : fallback.Patterns
        };
    }

    private I18nResource LoadDefaultResource()
    {
        var resource = LoadResourceFile(RequestLanguage.DefaultLanguage) ?? new I18nResource();
        EnrichDefaultResource(resource);
        return resource;
    }

    private async Task<I18nResource> LoadDefaultResourceAsync(CancellationToken cancellationToken)
    {
        var resource = await LoadResourceFileAsync(RequestLanguage.DefaultLanguage, cancellationToken) ?? new I18nResource();
        await EnrichDefaultResourceAsync(resource, cancellationToken);
        return resource;
    }

    private I18nResource? LoadResourceFile(string language)
    {
        var filePath = GetResourceFilePath(language);
        if (filePath is null || !File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<I18nResource>(json, ReadJsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load i18n resource {language}: {ex.Message}");
            return null;
        }
    }

    private async Task<I18nResource?> LoadResourceFileAsync(string language, CancellationToken cancellationToken)
    {
        var filePath = GetResourceFilePath(language);
        if (filePath is null || !File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<I18nResource>(json, ReadJsonOptions);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"Failed to load i18n resource {language}: {ex.Message}");
            return null;
        }
    }

    private bool HasResourceFile(string language)
    {
        var filePath = GetResourceFilePath(language);
        return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath);
    }

    private string? GetResourceFilePath(string language)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        return RequestLanguage.IsDefaultLanguage(normalizedLanguage)
            ? GetDefaultResourceFilePath()
            : GetI18nResourceFilePath(normalizedLanguage);
    }

    private string? GetDefaultResourceFilePath()
    {
        var primaryPath = GetAssetPath("site", "lang.json");
        if (!string.IsNullOrWhiteSpace(primaryPath) && File.Exists(primaryPath))
        {
            return primaryPath;
        }

        var legacyPath = GetAssetPath("site", "i18n", $"{RequestLanguage.DefaultLanguage}.json");
        return !string.IsNullOrWhiteSpace(legacyPath) && File.Exists(legacyPath)
            ? legacyPath
            : primaryPath;
    }

    private string? GetI18nResourceFilePath(string language, bool createCultureDirectory = false) =>
        GetI18nAssetPath(language, createCultureDirectory, "site", "lang.json");

    private string? GetI18nAssetPath(string language, bool createCultureDirectory, params string[] paths)
    {
        var cultureDir = GetI18nCultureDir(language, createCultureDirectory);
        if (cultureDir is null)
        {
            return null;
        }

        var segments = new string[paths.Length + 1];
        segments[0] = cultureDir;
        Array.Copy(paths, 0, segments, 1, paths.Length);

        return Path.Combine(segments);
    }

    private string? GetI18nCultureDir(string language, bool createDirectory = false)
    {
        var i18nResourcesDir = GetI18nResourcesDir();
        if (i18nResourcesDir is null)
        {
            return null;
        }

        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        var cultureDir = Path.Combine(i18nResourcesDir, normalizedLanguage);
        if (createDirectory)
        {
            Directory.CreateDirectory(cultureDir);
        }

        return cultureDir;
    }

    private string? GetAssetPath(params string[] paths)
    {
        var root = GetLocalAssetsDir();
        if (root is null)
        {
            return null;
        }

        var segments = new string[paths.Length + 1];
        segments[0] = root;
        Array.Copy(paths, 0, segments, 1, paths.Length);

        return Path.Combine(segments);
    }

    private string? GetLocalAssetsDir() => ResolveConfiguredDirectory(_siteOption.Value.LocalAssetsDir);

    private string? GetI18nResourcesDir() => ResolveConfiguredDirectory(_siteOption.Value.I18nResourcesDir);

    private string? ResolveConfiguredDirectory(string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return null;
        }

        var expandedPath = Environment.ExpandEnvironmentVariables(configuredPath.Trim());
        var path = Path.IsPathRooted(expandedPath)
            ? expandedPath
            : Path.Combine(_environment.ContentRootPath, expandedPath);

        return Path.GetFullPath(path);
    }

    private static Dictionary<string, string> Merge(
        IReadOnlyDictionary<string, string> fallback,
        IReadOnlyDictionary<string, string> current)
    {
        var merged = new Dictionary<string, string>(fallback, StringComparer.Ordinal);
        foreach (var item in current)
        {
            merged[item.Key] = item.Value;
        }

        return merged;
    }

    private I18nResource? CreateResourceFileFromDefault(string language, I18nResource defaultResource)
    {
        var defaultFilePath = GetDefaultResourceFilePath();
        var targetFilePath = GetI18nResourceFilePath(language, createCultureDirectory: false);
        if (defaultFilePath is null || !File.Exists(defaultFilePath))
        {
            CodeWfLogger.Warn(
                $"i18n 语言资源未生成，默认资源文件不存在。language={language}; source={defaultFilePath ?? "(null)"}; target={targetFilePath ?? "(null)"}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return null;
        }

        if (targetFilePath is null)
        {
            CodeWfLogger.Warn(
                $"i18n 语言资源未生成，目标资源路径为空。language={language}; source={defaultFilePath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return null;
        }

        if (File.Exists(targetFilePath))
        {
            return null;
        }

        var gate = _resourceFileLocks.GetOrAdd(targetFilePath, _ => new SemaphoreSlim(1, 1));
        var stopwatch = Stopwatch.StartNew();
        CodeWfLogger.Info(
            $"i18n 语言资源准备开始。language={language}; source={defaultFilePath}; target={targetFilePath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        _logger.LogInformation(
            "I18n language resource translation started. Language={Language}; Source={SourcePath}; Target={TargetPath}.",
            language,
            defaultFilePath,
            targetFilePath);
        gate.Wait();
        try
        {
            if (File.Exists(targetFilePath))
            {
                stopwatch.Stop();
                CodeWfLogger.Info(
                    $"i18n 语言资源已由其他请求生成。language={language}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogInformation(
                    "I18n language resource already exists after waiting. Language={Language}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    language,
                    targetFilePath,
                    stopwatch.ElapsedMilliseconds);
                return LoadResourceFile(language);
            }

            var source = JsonSerializer.Serialize(defaultResource, WriteJsonOptions);
            var translated = _translationService.TranslateAsync(
                    source,
                    RequestLanguage.GetLanguage(language),
                    ContentTranslationKind.JsonResource,
                    resourceName: targetFilePath)
                .GetAwaiter()
                .GetResult();

            if (string.IsNullOrWhiteSpace(translated))
            {
                stopwatch.Stop();
                CodeWfLogger.Warn(
                    $"i18n 语言资源未生成，翻译结果为空。language={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; source={defaultFilePath}; target={targetFilePath}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogWarning(
                    "I18n language resource translation returned empty result. Language={Language}; Source={SourcePath}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    language,
                    defaultFilePath,
                    targetFilePath,
                    stopwatch.ElapsedMilliseconds);
                return null;
            }

            var resource = JsonSerializer.Deserialize<I18nResource>(translated, ReadJsonOptions);
            if (resource is null)
            {
                stopwatch.Stop();
                CodeWfLogger.Warn(
                    $"i18n 语言资源未生成，翻译结果无法反序列化。language={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; source={defaultFilePath}; target={targetFilePath}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                return null;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
            File.WriteAllText(targetFilePath, translated);
            stopwatch.Stop();
            CodeWfLogger.Info(
                $"i18n 语言资源生成完成。language={language}; outputChars={translated.Length}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "I18n language resource saved. Language={Language}; Source={SourcePath}; Target={TargetPath}; OutputChars={OutputChars}; ElapsedMs={ElapsedMs}.",
                language,
                defaultFilePath,
                targetFilePath,
                translated.Length,
                stopwatch.ElapsedMilliseconds);
            return resource;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            CodeWfLogger.Error(
                $"i18n 语言资源生成失败。language={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogError(
                ex,
                "I18n language resource translation failed. Language={Language}; Source={SourcePath}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                language,
                defaultFilePath,
                targetFilePath,
                stopwatch.ElapsedMilliseconds);
            Console.WriteLine($"Failed to create i18n resource {language}: {ex.Message}");
            return null;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<I18nResource?> CreateResourceFileFromDefaultAsync(
        string language,
        I18nResource defaultResource,
        CancellationToken cancellationToken)
    {
        var defaultFilePath = GetDefaultResourceFilePath();
        var targetFilePath = GetI18nResourceFilePath(language, createCultureDirectory: false);
        if (defaultFilePath is null || !File.Exists(defaultFilePath))
        {
            CodeWfLogger.Warn(
                $"i18n 璇█璧勬簮鏈敓鎴愶紝榛樿璧勬簮鏂囦欢涓嶅瓨鍦ㄣ€俵anguage={language}; source={defaultFilePath ?? "(null)"}; target={targetFilePath ?? "(null)"}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return null;
        }

        if (targetFilePath is null)
        {
            CodeWfLogger.Warn(
                $"i18n 璇█璧勬簮鏈敓鎴愶紝鐩爣璧勬簮璺緞涓虹┖銆俵anguage={language}; source={defaultFilePath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return null;
        }

        if (File.Exists(targetFilePath))
        {
            return null;
        }

        var gate = _resourceFileLocks.GetOrAdd(targetFilePath, _ => new SemaphoreSlim(1, 1));
        var stopwatch = Stopwatch.StartNew();
        CodeWfLogger.Info(
            $"i18n 璇█璧勬簮鍑嗗寮€濮嬨€俵anguage={language}; source={defaultFilePath}; target={targetFilePath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        _logger.LogInformation(
            "I18n language resource translation started. Language={Language}; Source={SourcePath}; Target={TargetPath}.",
            language,
            defaultFilePath,
            targetFilePath);
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(targetFilePath))
            {
                stopwatch.Stop();
                CodeWfLogger.Info(
                    $"i18n 璇█璧勬簮宸茬敱鍏朵粬璇锋眰鐢熸垚銆俵anguage={language}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogInformation(
                    "I18n language resource already exists after waiting. Language={Language}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    language,
                    targetFilePath,
                    stopwatch.ElapsedMilliseconds);
                return await LoadResourceFileAsync(language, cancellationToken);
            }

            var source = JsonSerializer.Serialize(defaultResource, WriteJsonOptions);
            var translated = await _translationService.TranslateAsync(
                source,
                RequestLanguage.GetLanguage(language),
                ContentTranslationKind.JsonResource,
                resourceName: targetFilePath,
                cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(translated))
            {
                stopwatch.Stop();
                CodeWfLogger.Warn(
                    $"i18n 璇█璧勬簮鏈敓鎴愶紝缈昏瘧缁撴灉涓虹┖銆俵anguage={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; source={defaultFilePath}; target={targetFilePath}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogWarning(
                    "I18n language resource translation returned empty result. Language={Language}; Source={SourcePath}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    language,
                    defaultFilePath,
                    targetFilePath,
                    stopwatch.ElapsedMilliseconds);
                return null;
            }

            var resource = JsonSerializer.Deserialize<I18nResource>(translated, ReadJsonOptions);
            if (resource is null)
            {
                stopwatch.Stop();
                CodeWfLogger.Warn(
                    $"i18n 璇█璧勬簮鏈敓鎴愶紝缈昏瘧缁撴灉鏃犳硶鍙嶅簭鍒楀寲銆俵anguage={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; source={defaultFilePath}; target={targetFilePath}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                return null;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
            await File.WriteAllTextAsync(targetFilePath, translated, cancellationToken);
            stopwatch.Stop();
            CodeWfLogger.Info(
                $"i18n 璇█璧勬簮鐢熸垚瀹屾垚銆俵anguage={language}; outputChars={translated.Length}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "I18n language resource saved. Language={Language}; Source={SourcePath}; Target={TargetPath}; OutputChars={OutputChars}; ElapsedMs={ElapsedMs}.",
                language,
                defaultFilePath,
                targetFilePath,
                translated.Length,
                stopwatch.ElapsedMilliseconds);
            return resource;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            CodeWfLogger.Error(
                $"i18n 璇█璧勬簮鐢熸垚澶辫触銆俵anguage={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogError(
                ex,
                "I18n language resource translation failed. Language={Language}; Source={SourcePath}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                language,
                defaultFilePath,
                targetFilePath,
                stopwatch.ElapsedMilliseconds);
            Console.WriteLine($"Failed to create i18n resource {language}: {ex.Message}");
            return null;
        }
        finally
        {
            gate.Release();
        }
    }

    private I18nResource CompleteResourceFromDefault(
        string language,
        I18nResource fallback,
        I18nResource parent,
        I18nResource current)
    {
        var mergedExistingStrings = Merge(parent.Strings, current.Strings);
        var mergedExistingTextMap = Merge(parent.TextMap, current.TextMap);
        var missingStrings = fallback.Strings
            .Where(item => !mergedExistingStrings.ContainsKey(item.Key))
            .ToDictionary(static item => item.Key, static item => item.Value, StringComparer.Ordinal);
        var missingTextMap = fallback.TextMap
            .Where(item => !mergedExistingTextMap.ContainsKey(item.Key))
            .ToDictionary(static item => item.Key, static item => item.Value, StringComparer.Ordinal);

        if (missingStrings.Count == 0 && missingTextMap.Count == 0)
        {
            return current;
        }

        var targetFilePath = GetI18nResourceFilePath(language, createCultureDirectory: false);
        if (targetFilePath is null)
        {
            return current;
        }

        var patch = new I18nResource
        {
            Strings = missingStrings,
            TextMap = missingTextMap
        };
        var source = JsonSerializer.Serialize(patch, WriteJsonOptions);
        var gate = _resourceFileLocks.GetOrAdd(targetFilePath, _ => new SemaphoreSlim(1, 1));
        var stopwatch = Stopwatch.StartNew();
        CodeWfLogger.Info(
            $"i18n 语言资源补译开始。language={language}; missingStrings={missingStrings.Count}; missingTextMap={missingTextMap.Count}; target={targetFilePath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        _logger.LogInformation(
            "I18n language resource backfill started. Language={Language}; MissingStrings={MissingStrings}; MissingTextMap={MissingTextMap}; Target={TargetPath}.",
            language,
            missingStrings.Count,
            missingTextMap.Count,
            targetFilePath);
        gate.Wait();
        try
        {
            var latest = LoadResourceFile(language) ?? current;
            var latestWithParentStrings = Merge(parent.Strings, latest.Strings);
            var latestWithParentTextMap = Merge(parent.TextMap, latest.TextMap);
            missingStrings = fallback.Strings
                .Where(item => !latestWithParentStrings.ContainsKey(item.Key))
                .ToDictionary(static item => item.Key, static item => item.Value, StringComparer.Ordinal);
            missingTextMap = fallback.TextMap
                .Where(item => !latestWithParentTextMap.ContainsKey(item.Key))
                .ToDictionary(static item => item.Key, static item => item.Value, StringComparer.Ordinal);
            if (missingStrings.Count == 0 && missingTextMap.Count == 0)
            {
                stopwatch.Stop();
                CodeWfLogger.Info(
                    $"i18n 语言资源补译跳过，缺失项已由其他请求补齐。language={language}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogInformation(
                    "I18n language resource backfill skipped because another request completed it. Language={Language}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    language,
                    targetFilePath,
                    stopwatch.ElapsedMilliseconds);
                return latest;
            }

            patch = new I18nResource
            {
                Strings = missingStrings,
                TextMap = missingTextMap
            };
            source = JsonSerializer.Serialize(patch, WriteJsonOptions);
            var translated = _translationService.TranslateAsync(
                    source,
                    RequestLanguage.GetLanguage(language),
                    ContentTranslationKind.JsonResource,
                    resourceName: targetFilePath)
                .GetAwaiter()
                .GetResult();
            if (string.IsNullOrWhiteSpace(translated))
            {
                stopwatch.Stop();
                CodeWfLogger.Warn(
                    $"i18n 语言资源补译未完成，翻译结果为空。language={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogWarning(
                    "I18n language resource backfill returned empty result. Language={Language}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    language,
                    targetFilePath,
                    stopwatch.ElapsedMilliseconds);
                return latest;
            }

            var translatedPatch = JsonSerializer.Deserialize<I18nResource>(translated, ReadJsonOptions);
            if (translatedPatch is null)
            {
                stopwatch.Stop();
                CodeWfLogger.Warn(
                    $"i18n 语言资源补译未完成，翻译结果无法反序列化。language={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                return latest;
            }

            foreach (var item in translatedPatch.Strings)
            {
                latest.Strings[item.Key] = item.Value;
            }

            foreach (var item in translatedPatch.TextMap)
            {
                latest.TextMap[item.Key] = item.Value;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
            File.WriteAllText(targetFilePath, JsonSerializer.Serialize(latest, WriteJsonOptions));
            stopwatch.Stop();
            CodeWfLogger.Info(
                $"i18n 语言资源补译完成。language={language}; strings={translatedPatch.Strings.Count}; textMap={translatedPatch.TextMap.Count}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "I18n language resource backfill saved. Language={Language}; Target={TargetPath}; Strings={Strings}; TextMap={TextMap}; ElapsedMs={ElapsedMs}.",
                language,
                targetFilePath,
                translatedPatch.Strings.Count,
                translatedPatch.TextMap.Count,
                stopwatch.ElapsedMilliseconds);
            return latest;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            CodeWfLogger.Error(
                $"i18n 语言资源补译失败。language={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogError(
                ex,
                "I18n language resource backfill failed. Language={Language}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                language,
                targetFilePath,
                stopwatch.ElapsedMilliseconds);
            return current;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<I18nResource> CompleteResourceFromDefaultAsync(
        string language,
        I18nResource fallback,
        I18nResource parent,
        I18nResource current,
        CancellationToken cancellationToken)
    {
        var mergedExistingStrings = Merge(parent.Strings, current.Strings);
        var mergedExistingTextMap = Merge(parent.TextMap, current.TextMap);
        var missingStrings = fallback.Strings
            .Where(item => !mergedExistingStrings.ContainsKey(item.Key))
            .ToDictionary(static item => item.Key, static item => item.Value, StringComparer.Ordinal);
        var missingTextMap = fallback.TextMap
            .Where(item => !mergedExistingTextMap.ContainsKey(item.Key))
            .ToDictionary(static item => item.Key, static item => item.Value, StringComparer.Ordinal);

        if (missingStrings.Count == 0 && missingTextMap.Count == 0)
        {
            return current;
        }

        var targetFilePath = GetI18nResourceFilePath(language, createCultureDirectory: false);
        if (targetFilePath is null)
        {
            return current;
        }

        var patch = new I18nResource
        {
            Strings = missingStrings,
            TextMap = missingTextMap
        };
        var source = JsonSerializer.Serialize(patch, WriteJsonOptions);
        var gate = _resourceFileLocks.GetOrAdd(targetFilePath, _ => new SemaphoreSlim(1, 1));
        var stopwatch = Stopwatch.StartNew();
        CodeWfLogger.Info(
            $"i18n 璇█璧勬簮琛ヨ瘧寮€濮嬨€俵anguage={language}; missingStrings={missingStrings.Count}; missingTextMap={missingTextMap.Count}; target={targetFilePath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        _logger.LogInformation(
            "I18n language resource backfill started. Language={Language}; MissingStrings={MissingStrings}; MissingTextMap={MissingTextMap}; Target={TargetPath}.",
            language,
            missingStrings.Count,
            missingTextMap.Count,
            targetFilePath);
        await gate.WaitAsync(cancellationToken);
        try
        {
            var latest = await LoadResourceFileAsync(language, cancellationToken) ?? current;
            var latestWithParentStrings = Merge(parent.Strings, latest.Strings);
            var latestWithParentTextMap = Merge(parent.TextMap, latest.TextMap);
            missingStrings = fallback.Strings
                .Where(item => !latestWithParentStrings.ContainsKey(item.Key))
                .ToDictionary(static item => item.Key, static item => item.Value, StringComparer.Ordinal);
            missingTextMap = fallback.TextMap
                .Where(item => !latestWithParentTextMap.ContainsKey(item.Key))
                .ToDictionary(static item => item.Key, static item => item.Value, StringComparer.Ordinal);
            if (missingStrings.Count == 0 && missingTextMap.Count == 0)
            {
                stopwatch.Stop();
                CodeWfLogger.Info(
                    $"i18n 璇█璧勬簮琛ヨ瘧璺宠繃锛岀己澶遍」宸茬敱鍏朵粬璇锋眰琛ラ綈銆俵anguage={language}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogInformation(
                    "I18n language resource backfill skipped because another request completed it. Language={Language}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    language,
                    targetFilePath,
                    stopwatch.ElapsedMilliseconds);
                return latest;
            }

            patch = new I18nResource
            {
                Strings = missingStrings,
                TextMap = missingTextMap
            };
            source = JsonSerializer.Serialize(patch, WriteJsonOptions);
            var translated = await _translationService.TranslateAsync(
                source,
                RequestLanguage.GetLanguage(language),
                ContentTranslationKind.JsonResource,
                resourceName: targetFilePath,
                cancellationToken: cancellationToken);
            if (string.IsNullOrWhiteSpace(translated))
            {
                stopwatch.Stop();
                CodeWfLogger.Warn(
                    $"i18n 璇█璧勬簮琛ヨ瘧鏈畬鎴愶紝缈昏瘧缁撴灉涓虹┖銆俵anguage={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                _logger.LogWarning(
                    "I18n language resource backfill returned empty result. Language={Language}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    language,
                    targetFilePath,
                    stopwatch.ElapsedMilliseconds);
                return latest;
            }

            var translatedPatch = JsonSerializer.Deserialize<I18nResource>(translated, ReadJsonOptions);
            if (translatedPatch is null)
            {
                stopwatch.Stop();
                CodeWfLogger.Warn(
                    $"i18n 璇█璧勬簮琛ヨ瘧鏈畬鎴愶紝缈昏瘧缁撴灉鏃犳硶鍙嶅簭鍒楀寲銆俵anguage={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                return latest;
            }

            foreach (var item in translatedPatch.Strings)
            {
                latest.Strings[item.Key] = item.Value;
            }

            foreach (var item in translatedPatch.TextMap)
            {
                latest.TextMap[item.Key] = item.Value;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
            await File.WriteAllTextAsync(targetFilePath, JsonSerializer.Serialize(latest, WriteJsonOptions), cancellationToken);
            stopwatch.Stop();
            CodeWfLogger.Info(
                $"i18n 璇█璧勬簮琛ヨ瘧瀹屾垚銆俵anguage={language}; strings={translatedPatch.Strings.Count}; textMap={translatedPatch.TextMap.Count}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "I18n language resource backfill saved. Language={Language}; Target={TargetPath}; Strings={Strings}; TextMap={TextMap}; ElapsedMs={ElapsedMs}.",
                language,
                targetFilePath,
                translatedPatch.Strings.Count,
                translatedPatch.TextMap.Count,
                stopwatch.ElapsedMilliseconds);
            return latest;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            CodeWfLogger.Error(
                $"i18n 璇█璧勬簮琛ヨ瘧澶辫触銆俵anguage={language}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetFilePath}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogError(
                ex,
                "I18n language resource backfill failed. Language={Language}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                language,
                targetFilePath,
                stopwatch.ElapsedMilliseconds);
            return current;
        }
        finally
        {
            gate.Release();
        }
    }

    private void EnrichDefaultResource(I18nResource resource)
    {
        foreach (var item in resource.Strings.Values.Where(ShouldIncludeDiscoveredText))
        {
            resource.TextMap.TryAdd(item, item);
        }

        foreach (var item in RequiredDefaultTextMapKeys)
        {
            resource.TextMap.TryAdd(item, item);
        }

        foreach (var text in GetDiscoveredDefaultTexts())
        {
            resource.TextMap.TryAdd(text, text);
        }
    }

    private async Task EnrichDefaultResourceAsync(I18nResource resource, CancellationToken cancellationToken)
    {
        foreach (var item in resource.Strings.Values.Where(ShouldIncludeDiscoveredText))
        {
            resource.TextMap.TryAdd(item, item);
        }

        foreach (var item in RequiredDefaultTextMapKeys)
        {
            resource.TextMap.TryAdd(item, item);
        }

        foreach (var text in await GetDiscoveredDefaultTextsAsync(cancellationToken))
        {
            resource.TextMap.TryAdd(text, text);
        }
    }

    private IReadOnlyCollection<string> GetDiscoveredDefaultTexts()
    {
        if (_discoveredDefaultTexts is not null)
        {
            return _discoveredDefaultTexts;
        }

        var texts = new HashSet<string>(StringComparer.Ordinal);
        var scannedRoots = new List<string>();
        foreach (var root in TextDiscoveryRoots)
        {
            var rootPath = Path.Combine(
                new[] { _environment.ContentRootPath }
                    .Concat(root.Split('/', StringSplitOptions.RemoveEmptyEntries))
                    .ToArray());
            if (!Directory.Exists(rootPath))
            {
                continue;
            }

            scannedRoots.Add(rootPath);
            foreach (var filePath in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
                         .Where(static path => TextDiscoveryExtensions.Contains(
                             Path.GetExtension(path),
                             StringComparer.OrdinalIgnoreCase)))
            {
                try
                {
                    var content = File.ReadAllText(filePath);
                    AddMatches(texts, HtmlTextRegex, content);
                    AddMatches(texts, AttributeTextRegex, content);
                    AddMatches(texts, DoubleQuotedTextRegex, content);
                    AddMatches(texts, SingleQuotedTextRegex, content);
                    AddMatches(texts, BacktickTextRegex, content);
                }
                catch (Exception ex)
                {
                    CodeWfLogger.Warn(
                        $"i18n 页面文案扫描跳过文件。file={filePath}; error={ex.Message}.",
                        log2UI: false,
                        log2File: false,
                        log2Console: true);
                }
            }
        }

        _discoveredDefaultTexts = texts;
        CodeWfLogger.Info(
            $"i18n 页面文案扫描完成。count={texts.Count}; roots={string.Join('|', scannedRoots)}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        return _discoveredDefaultTexts;
    }

    private async Task<IReadOnlyCollection<string>> GetDiscoveredDefaultTextsAsync(CancellationToken cancellationToken)
    {
        if (_discoveredDefaultTexts is not null)
        {
            return _discoveredDefaultTexts;
        }

        var texts = new HashSet<string>(StringComparer.Ordinal);
        var scannedRoots = new List<string>();
        foreach (var root in TextDiscoveryRoots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rootPath = Path.Combine(
                new[] { _environment.ContentRootPath }
                    .Concat(root.Split('/', StringSplitOptions.RemoveEmptyEntries))
                    .ToArray());
            if (!Directory.Exists(rootPath))
            {
                continue;
            }

            scannedRoots.Add(rootPath);
            foreach (var filePath in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
                         .Where(static path => TextDiscoveryExtensions.Contains(
                             Path.GetExtension(path),
                             StringComparer.OrdinalIgnoreCase)))
            {
                try
                {
                    var content = await File.ReadAllTextAsync(filePath, cancellationToken);
                    AddMatches(texts, HtmlTextRegex, content);
                    AddMatches(texts, AttributeTextRegex, content);
                    AddMatches(texts, DoubleQuotedTextRegex, content);
                    AddMatches(texts, SingleQuotedTextRegex, content);
                    AddMatches(texts, BacktickTextRegex, content);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    CodeWfLogger.Warn(
                        $"i18n 椤甸潰鏂囨鎵弿璺宠繃鏂囦欢銆俧ile={filePath}; error={ex.Message}.",
                        log2UI: false,
                        log2File: false,
                        log2Console: true);
                }
            }
        }

        _discoveredDefaultTexts = texts;
        CodeWfLogger.Info(
            $"i18n 椤甸潰鏂囨鎵弿瀹屾垚銆俢ount={texts.Count}; roots={string.Join('|', scannedRoots)}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        return _discoveredDefaultTexts;
    }

    private static void AddMatches(HashSet<string> texts, Regex regex, string content)
    {
        foreach (Match match in regex.Matches(content))
        {
            var value = NormalizeDiscoveredText(match.Groups["value"].Value);
            if (ShouldIncludeDiscoveredText(value))
            {
                texts.Add(value);
            }
        }
    }

    private static string NormalizeDiscoveredText(string value)
    {
        var text = System.Net.WebUtility.HtmlDecode(value)
            .Replace("\\\"", "\"", StringComparison.Ordinal)
            .Replace("\\'", "'", StringComparison.Ordinal)
            .Replace("\\n", " ", StringComparison.Ordinal)
            .Replace("\\r", " ", StringComparison.Ordinal)
            .Replace("\\t", " ", StringComparison.Ordinal)
            .Trim();
        return WhitespaceRegex.Replace(text, " ");
    }

    private static bool ShouldIncludeDiscoveredText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var text = value.Trim();
        return text.Length is >= 2 and <= 240
            && ChineseTextRegex.IsMatch(text)
            && !text.Contains('@', StringComparison.Ordinal)
            && !text.Contains("${", StringComparison.Ordinal)
            && !text.Contains("=>", StringComparison.Ordinal)
            && !text.StartsWith("//", StringComparison.Ordinal)
            && !text.StartsWith("/*", StringComparison.Ordinal);
    }

    private static string? GetParentLanguage(string language)
    {
        var separatorIndex = language.IndexOf('-', StringComparison.Ordinal);
        return separatorIndex > 0 ? language[..separatorIndex] : null;
    }
}

public sealed class I18nResource
{
    public Dictionary<string, string> Strings { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> TextMap { get; set; } = new(StringComparer.Ordinal);
    public List<I18nPattern> Patterns { get; set; } = [];
}

public sealed record I18nPattern(string Pattern, string Replacement);

public sealed record LanguageResourceStatus(
    string Code,
    string NativeName,
    string EnglishName,
    bool IsDefaultLanguage,
    bool HasResourceFile);

public sealed record LanguageResourcePrepareResult(
    string Code,
    string NativeName,
    string EnglishName,
    bool IsDefaultLanguage,
    bool HadResourceFile,
    bool HasResourceFile,
    bool CreatedResourceFile,
    int StringCount,
    int TextMapCount);

public sealed record ClientI18nResource(
    string Language,
    IReadOnlyList<string> Languages,
    IReadOnlyDictionary<string, string> TextMap,
    IReadOnlyDictionary<string, string> Strings,
    IReadOnlyList<I18nPattern> Patterns);
