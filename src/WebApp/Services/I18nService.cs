using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
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
    private static readonly Regex I18nFallbackRegex = new(
        @"(?:I18n|i18nService|_i18nService)\.(?:T|Format)\(\s*""(?<key>[^""]+)""\s*,\s*""(?<value>(?:[^""\\]|\\.)*[\u3400-\u9fff](?:[^""\\]|\\.)*)""",
        RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex InterpolatedExpressionRegex = new(@"\{[A-Za-z_]\w*(?:\.[^}]*)+\}", RegexOptions.Compiled);
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
    private readonly ILogger<I18nService> _logger;
    private IReadOnlyDictionary<string, string>? _discoveredDefaultStrings;
    private IReadOnlyCollection<string>? _discoveredDefaultTexts;

    public I18nService(
        IOptions<SiteOption> siteOption,
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        ILogger<I18nService>? logger = null)
    {
        _siteOption = siteOption;
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
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
        var resource = GetResource(CurrentLanguage);
        if (string.IsNullOrWhiteSpace(key))
        {
            return TranslateText(resource, fallback);
        }

        return resource.Strings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? TranslateText(resource, value)
            : TranslateText(resource, fallback);
    }

    public string Format(string key, string fallback, params object[] args)
    {
        var resource = GetResource(CurrentLanguage);
        var template = !string.IsNullOrWhiteSpace(key)
                       && resource.Strings.TryGetValue(key, out var value)
                       && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
        var formatted = args.Length == 0 ? template : string.Format(template, args);
        return TranslateText(resource, formatted);
    }

    public string Text(string? text)
    {
        var resource = GetResource(CurrentLanguage);
        return TranslateText(resource, text);
    }

    private static string TranslateText(I18nResource resource, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text ?? string.Empty;
        }

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
        if (resource.TextMap.TryGetValue(trimmed, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return $"{text[..start]}{value}{text[(end + 1)..]}";
        }

        foreach (var pattern in resource.Patterns)
        {
            try
            {
                var regex = new Regex(pattern.Pattern);
                if (regex.IsMatch(trimmed))
                {
                    return $"{text[..start]}{regex.Replace(trimmed, pattern.Replacement)}{text[(end + 1)..]}";
                }
            }
            catch (ArgumentException)
            {
                continue;
            }
        }

        return text;
    }

    public string Url(string? url, string? language = null) => RequestLanguage.LocalizePath(url, language);

    public string FormatDate(DateTime? value) =>
        value.HasValue ? value.Value.ToString("d", CultureInfo.CurrentCulture) : string.Empty;

    public string FormatDateTime(DateTime? value) =>
        value.HasValue ? value.Value.ToString("g", CultureInfo.CurrentCulture) : string.Empty;

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
        var current = await LoadResourceFileAsync(language, cancellationToken) ?? new I18nResource();

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
            : GetLocalizedResourceFilePath(normalizedLanguage);
    }

    private string? GetDefaultResourceFilePath() => GetAssetPath("site", "lang.json");

    private string? GetLocalizedResourceFilePath(string language) => GetAssetPath("site", $"lang.{language}.json");

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

    private void EnrichDefaultResource(I18nResource resource)
    {
        foreach (var item in GetDiscoveredDefaultStrings())
        {
            resource.Strings.TryAdd(item.Key, item.Value);
        }

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
        foreach (var item in await GetDiscoveredDefaultStringsAsync(cancellationToken))
        {
            resource.Strings.TryAdd(item.Key, item.Value);
        }

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

    private IReadOnlyDictionary<string, string> GetDiscoveredDefaultStrings()
    {
        if (_discoveredDefaultStrings is not null)
        {
            return _discoveredDefaultStrings;
        }

        var strings = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var filePath in EnumerateDiscoveryFiles())
        {
            try
            {
                AddI18nFallbackMatches(strings, File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                CodeWfLogger.Warn(
                    $"i18n 页面键值扫描跳过文件。file={filePath}; error={ex.Message}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
            }
        }

        _discoveredDefaultStrings = strings;
        CodeWfLogger.Info(
            $"i18n 页面键值扫描完成。count={strings.Count}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        return _discoveredDefaultStrings;
    }

    private async Task<IReadOnlyDictionary<string, string>> GetDiscoveredDefaultStringsAsync(CancellationToken cancellationToken)
    {
        if (_discoveredDefaultStrings is not null)
        {
            return _discoveredDefaultStrings;
        }

        var strings = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var filePath in EnumerateDiscoveryFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                AddI18nFallbackMatches(strings, await File.ReadAllTextAsync(filePath, cancellationToken));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                CodeWfLogger.Warn(
                    $"i18n 页面键值扫描跳过文件。file={filePath}; error={ex.Message}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
            }
        }

        _discoveredDefaultStrings = strings;
        CodeWfLogger.Info(
            $"i18n 页面键值扫描完成。count={strings.Count}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        return _discoveredDefaultStrings;
    }

    private IReadOnlyCollection<string> GetDiscoveredDefaultTexts()
    {
        if (_discoveredDefaultTexts is not null)
        {
            return _discoveredDefaultTexts;
        }

        var texts = new HashSet<string>(StringComparer.Ordinal);
        var scannedRoots = new List<string>();
        foreach (var rootPath in EnumerateDiscoveryRoots())
        {
            scannedRoots.Add(rootPath);
            foreach (var filePath in EnumerateDiscoveryFiles(rootPath))
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
        foreach (var rootPath in EnumerateDiscoveryRoots())
        {
            cancellationToken.ThrowIfCancellationRequested();
            scannedRoots.Add(rootPath);
            foreach (var filePath in EnumerateDiscoveryFiles(rootPath))
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

    private IEnumerable<string> EnumerateDiscoveryRoots()
    {
        foreach (var root in TextDiscoveryRoots)
        {
            var rootPath = Path.Combine(
                new[] { _environment.ContentRootPath }
                    .Concat(root.Split('/', StringSplitOptions.RemoveEmptyEntries))
                    .ToArray());
            if (Directory.Exists(rootPath))
            {
                yield return rootPath;
            }
        }
    }

    private IEnumerable<string> EnumerateDiscoveryFiles() =>
        EnumerateDiscoveryRoots().SelectMany(EnumerateDiscoveryFiles);

    private static IEnumerable<string> EnumerateDiscoveryFiles(string rootPath) =>
        Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(static path => TextDiscoveryExtensions.Contains(
                Path.GetExtension(path),
                StringComparer.OrdinalIgnoreCase));

    private static void AddI18nFallbackMatches(Dictionary<string, string> strings, string content)
    {
        foreach (Match match in I18nFallbackRegex.Matches(content))
        {
            var key = match.Groups["key"].Value.Trim();
            var value = NormalizeDiscoveredText(match.Groups["value"].Value);
            if (!string.IsNullOrWhiteSpace(key) && ShouldIncludeDiscoveredText(value))
            {
                strings.TryAdd(key, value);
            }
        }
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
            && !ContainsMarkupFragment(text)
            && !ContainsCodeFragment(text)
            && !text.Contains('@', StringComparison.Ordinal)
            && !text.Contains("${", StringComparison.Ordinal)
            && !text.Contains("=>", StringComparison.Ordinal)
            && !text.StartsWith("//", StringComparison.Ordinal)
            && !text.StartsWith("/*", StringComparison.Ordinal);
    }

    private static bool ContainsMarkupFragment(string text) =>
        text.Contains('<', StringComparison.Ordinal)
        || text.Contains('>', StringComparison.Ordinal)
        || text.Contains("class=", StringComparison.OrdinalIgnoreCase)
        || text.Contains("href=", StringComparison.OrdinalIgnoreCase)
        || text.Contains("src=", StringComparison.OrdinalIgnoreCase)
        || text.Contains("</", StringComparison.Ordinal)
        || text.Contains("/>", StringComparison.Ordinal);

    private static bool ContainsCodeFragment(string text) =>
        text.Contains(';', StringComparison.Ordinal)
        || text.Contains("throw ", StringComparison.OrdinalIgnoreCase)
        || text.Contains("function ", StringComparison.OrdinalIgnoreCase)
        || text.Contains("return ", StringComparison.OrdinalIgnoreCase)
        || text.Contains("const ", StringComparison.OrdinalIgnoreCase)
        || text.Contains("let ", StringComparison.OrdinalIgnoreCase)
        || text.Contains("var ", StringComparison.OrdinalIgnoreCase)
        || text.Contains("while(", StringComparison.OrdinalIgnoreCase)
        || text.Contains("while (", StringComparison.OrdinalIgnoreCase)
        || text.Contains("new Error", StringComparison.OrdinalIgnoreCase)
        || InterpolatedExpressionRegex.IsMatch(text);

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
