using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace WebApp.Services;

public sealed record LanguageInfo(
    string Code,
    string DotNetCulture,
    string HtmlLang,
    string OgLocale,
    string NativeName,
    string EnglishName);

public static class RequestLanguage
{
    public const string DefaultLanguage = "zh-cn";
    public const string CookieName = "codewf.lang";

    public static readonly IReadOnlyList<LanguageInfo> SupportedLanguages =
    [
        new(DefaultLanguage, "zh-CN", "zh-CN", "zh_CN", "简体中文", "Simplified Chinese"),
        new("zh-tw", "zh-TW", "zh-TW", "zh_TW", "繁體中文", "Traditional Chinese"),
        new("en", "en-US", "en", "en_US", "English", "English"),
        new("ja", "ja-JP", "ja", "ja_JP", "日本語", "Japanese")
    ];

    private static readonly AsyncLocal<string?> CurrentLanguageHolder = new();
    private static readonly HashSet<string> SupportedCodes = SupportedLanguages
        .Select(static language => language.Code)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static string CurrentLanguage
    {
        get => Normalize(CurrentLanguageHolder.Value) ?? DefaultLanguage;
        set => CurrentLanguageHolder.Value = Normalize(value) ?? DefaultLanguage;
    }

    public static LanguageInfo CurrentLanguageInfo => GetLanguage(CurrentLanguage);

    public static void Clear() => CurrentLanguageHolder.Value = null;

    public static LanguageInfo GetLanguage(string? language)
    {
        var normalized = Normalize(language) ?? DefaultLanguage;
        return SupportedLanguages.FirstOrDefault(item =>
            string.Equals(item.Code, normalized, StringComparison.OrdinalIgnoreCase))
            ?? SupportedLanguages[0];
    }

    public static string? Normalize(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        var value = language.Trim().Replace('_', '-').ToLowerInvariant();
        if (SupportedCodes.Contains(value))
        {
            return value;
        }

        if (value.StartsWith("zh-hant", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("zh-tw", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("zh-hk", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("zh-mo", StringComparison.OrdinalIgnoreCase))
        {
            return "zh-tw";
        }

        if (value.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            return DefaultLanguage;
        }

        if (value.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            return "en";
        }

        if (value.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
        {
            return "ja";
        }

        return null;
    }

    public static bool TryGetPathLanguage(PathString path, out string language, out PathString remainingPath)
    {
        language = DefaultLanguage;
        remainingPath = path;

        var value = path.Value;
        if (string.IsNullOrWhiteSpace(value) || value == "/")
        {
            return false;
        }

        var span = value.AsSpan(1);
        var slashIndex = span.IndexOf('/');
        var firstSegment = slashIndex >= 0
            ? span[..slashIndex].ToString()
            : span.ToString();

        var normalized = Normalize(firstSegment);
        if (normalized is null || !SupportedCodes.Contains(normalized))
        {
            return false;
        }

        language = normalized;
        remainingPath = slashIndex < 0
            ? new PathString("/")
            : new PathString(value[(firstSegment.Length + 1)..]);

        return true;
    }

    public static string DetectPreferredLanguage(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(CookieName, out var cookieLanguage)
            && Normalize(cookieLanguage) is { } normalizedCookie)
        {
            return normalizedCookie;
        }

        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        if (!string.IsNullOrWhiteSpace(acceptLanguage))
        {
            foreach (var item in acceptLanguage.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var token = item.Split(';', 2)[0];
                if (Normalize(token) is { } normalized)
                {
                    return normalized;
                }
            }
        }

        return DefaultLanguage;
    }

    public static bool ShouldSkipLocalization(PathString path)
    {
        var value = path.Value ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/favicon.png", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/robots.txt", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string[] skippedPrefixes =
        [
            "/api",
            "/css",
            "/js",
            "/lib",
            "/img",
            "/webfonts",
            "/UploadIcons"
        ];

        return skippedPrefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    public static void ApplyCulture(string language)
    {
        var cultureName = GetLanguage(language).DotNetCulture;
        var culture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public static string LocalizePath(string? url, string? language = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return $"/{Normalize(language) ?? CurrentLanguage}";
        }

        var value = url.Trim();
        if (value.StartsWith('#')
            || value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        if (!value.StartsWith('/'))
        {
            return url;
        }

        var hashIndex = value.IndexOf('#', StringComparison.Ordinal);
        var hash = hashIndex >= 0 ? value[hashIndex..] : string.Empty;
        var beforeHash = hashIndex >= 0 ? value[..hashIndex] : value;
        var queryIndex = beforeHash.IndexOf('?', StringComparison.Ordinal);
        var query = queryIndex >= 0 ? beforeHash[queryIndex..] : string.Empty;
        var pathOnly = queryIndex >= 0 ? beforeHash[..queryIndex] : beforeHash;

        if (ShouldSkipLocalization(new PathString(pathOnly)))
        {
            return url;
        }

        var targetLanguage = Normalize(language) ?? CurrentLanguage;
        if (TryGetPathLanguage(new PathString(pathOnly), out _, out var remainingPath))
        {
            pathOnly = remainingPath.Value ?? "/";
        }

        var localizedPath = pathOnly == "/"
            ? $"/{targetLanguage}"
            : $"/{targetLanguage}{pathOnly}";

        return $"{localizedPath}{query}{hash}";
    }

    public static string BuildSwitchUrl(HttpContext context, string language)
    {
        var originalPath = context.Items[RequestLanguageMiddleware.OriginalPathItemKey] as string
            ?? context.Request.Path.ToString();
        return LocalizePath($"{originalPath}{context.Request.QueryString}", language);
    }

    public static string BuildSwitchPath(HttpContext context, string language)
    {
        var originalPath = context.Items[RequestLanguageMiddleware.OriginalPathItemKey] as string
            ?? context.Request.Path.ToString();
        return LocalizePath(originalPath, language);
    }
}

public sealed class RequestLanguageMiddleware
{
    public const string LanguageItemKey = "CodeWF.Language";
    public const string OriginalPathItemKey = "CodeWF.OriginalPath";

    private readonly RequestDelegate _next;

    public RequestLanguageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalPath = context.Request.Path;

        if (RequestLanguage.TryGetPathLanguage(originalPath, out var routeLanguage, out var remainingPath))
        {
            await InvokeWithLanguageAsync(context, routeLanguage, originalPath, remainingPath);
            return;
        }

        if (RequestLanguage.ShouldSkipLocalization(originalPath))
        {
            var fallbackLanguage = RequestLanguage.DetectPreferredLanguage(context);
            await InvokeWithLanguageAsync(context, fallbackLanguage, originalPath, originalPath, persistCookie: false);
            return;
        }

        var preferredLanguage = RequestLanguage.DetectPreferredLanguage(context);
        context.Response.Redirect(RequestLanguage.LocalizePath($"{originalPath}{context.Request.QueryString}", preferredLanguage));
    }

    private async Task InvokeWithLanguageAsync(
        HttpContext context,
        string language,
        PathString originalPath,
        PathString remainingPath,
        bool persistCookie = true)
    {
        var previousPath = context.Request.Path;
        context.Items[LanguageItemKey] = language;
        context.Items[OriginalPathItemKey] = originalPath.ToString();
        RequestLanguage.CurrentLanguage = language;
        RequestLanguage.ApplyCulture(language);

        if (persistCookie)
        {
            context.Response.Cookies.Append(
                RequestLanguage.CookieName,
                language,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = false,
                    IsEssential = true,
                    Path = "/",
                    SameSite = SameSiteMode.Lax,
                    Secure = context.Request.IsHttps
                });
        }

        try
        {
            context.Request.Path = remainingPath;
            await _next(context);
        }
        finally
        {
            context.Request.Path = previousPath;
            RequestLanguage.Clear();
        }
    }
}
