using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.Extensions.Options;
using WebApp.Options;

namespace WebApp.Services;

public sealed class I18nService
{
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

    public I18nService(
        IOptions<SiteOption> siteOption,
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        _siteOption = siteOption;
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public IReadOnlyList<LanguageInfo> Languages => RequestLanguage.SupportedLanguages;

    public string CurrentLanguage => RequestLanguage.CurrentLanguage;

    public LanguageInfo CurrentLanguageInfo => RequestLanguage.CurrentLanguageInfo;

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

    private I18nResource LoadResource(string language)
    {
        if (string.Equals(language, RequestLanguage.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
        {
            return LoadResourceFile(language) ?? new I18nResource();
        }

        var fallback = LoadResourceFile(RequestLanguage.DefaultLanguage) ?? new I18nResource();
        var current = LoadResourceFile(language) ?? new I18nResource();

        return new I18nResource
        {
            Strings = Merge(fallback.Strings, current.Strings),
            TextMap = Merge(fallback.TextMap, current.TextMap),
            Patterns = current.Patterns.Count > 0 ? current.Patterns : fallback.Patterns
        };
    }

    private I18nResource? LoadResourceFile(string language)
    {
        var filePath = GetAssetPath("site", "i18n", $"{language}.json");
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

    private string? GetAssetPath(params string[] paths)
    {
        var localAssetsDir = _siteOption.Value.LocalAssetsDir;
        if (string.IsNullOrWhiteSpace(localAssetsDir))
        {
            return null;
        }

        var expandedPath = Environment.ExpandEnvironmentVariables(localAssetsDir.Trim());
        var root = Path.IsPathRooted(expandedPath)
            ? expandedPath
            : Path.Combine(_environment.ContentRootPath, expandedPath);

        var segments = new string[paths.Length + 1];
        segments[0] = Path.GetFullPath(root);
        Array.Copy(paths, 0, segments, 1, paths.Length);

        return Path.Combine(segments);
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
}

public sealed class I18nResource
{
    public Dictionary<string, string> Strings { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> TextMap { get; set; } = new(StringComparer.Ordinal);
    public List<I18nPattern> Patterns { get; set; } = [];
}

public sealed record I18nPattern(string Pattern, string Replacement);

public sealed record ClientI18nResource(
    string Language,
    IReadOnlyDictionary<string, string> TextMap,
    IReadOnlyDictionary<string, string> Strings,
    IReadOnlyList<I18nPattern> Patterns);
