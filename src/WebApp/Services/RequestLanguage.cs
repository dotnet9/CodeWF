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

    public static readonly IReadOnlyList<LanguageInfo> SupportedLanguages = BuildSupportedLanguages();

    private static readonly AsyncLocal<string?> CurrentLanguageHolder = new();
    private static readonly HashSet<string> SupportedCodes = SupportedLanguages
        .Select(static language => language.Code)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ReservedRouteSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "about",
        "album",
        "api",
        "cat",
        "doc",
        "donation",
        "favicon.ico",
        "favicon.png",
        "img",
        "lib",
        "post",
        "privacy",
        "project",
        "robots.txt",
        "rss",
        "s",
        "search",
        "sitemap",
        "sitemap.xml",
        "tag",
        "timeline",
        "tool"
    };

    public static string CurrentLanguage
    {
        get => Normalize(CurrentLanguageHolder.Value) ?? DefaultLanguage;
        set => CurrentLanguageHolder.Value = Normalize(value) ?? DefaultLanguage;
    }

    public static LanguageInfo CurrentLanguageInfo => GetLanguage(CurrentLanguage);

    public static void Clear() => CurrentLanguageHolder.Value = null;

    public static bool IsDefaultLanguage(string? language) =>
        string.Equals(Normalize(language), DefaultLanguage, StringComparison.OrdinalIgnoreCase);

    public static LanguageInfo GetLanguage(string? language)
    {
        var normalized = Normalize(language) ?? DefaultLanguage;
        return SupportedLanguages.FirstOrDefault(item =>
            string.Equals(item.Code, normalized, StringComparison.OrdinalIgnoreCase))
            ?? SupportedLanguages[0];
    }

    public static IReadOnlyList<LanguageInfo> GetSeoLanguages(string? currentLanguage)
    {
        string[] preferredCodes =
        [
            DefaultLanguage,
            "zh-tw",
            "en",
            "ja",
            Normalize(currentLanguage) ?? DefaultLanguage
        ];

        return preferredCodes
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(GetLanguage)
            .GroupBy(static language => language.Code, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToList();
    }

    public static string? Normalize(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        var value = language.Trim().Replace('_', '-').ToLowerInvariant();

        if (value.Equals("zh-tw", StringComparison.OrdinalIgnoreCase)
            || value.Equals("zh-hk", StringComparison.OrdinalIgnoreCase)
            || value.Equals("zh-mo", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("zh-hant", StringComparison.OrdinalIgnoreCase))
        {
            return "zh-tw";
        }

        if (value.Equals("zh", StringComparison.OrdinalIgnoreCase)
            || value.Equals("zh-cn", StringComparison.OrdinalIgnoreCase)
            || value.Equals("zh-sg", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("zh-hans", StringComparison.OrdinalIgnoreCase))
        {
            return DefaultLanguage;
        }

        if (value.Equals("en", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("en-", StringComparison.OrdinalIgnoreCase))
        {
            return "en";
        }

        if (value.Equals("ja", StringComparison.OrdinalIgnoreCase)
            || value.Equals("jp", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("ja-", StringComparison.OrdinalIgnoreCase))
        {
            return "ja";
        }

        if (value.StartsWith("zh-", StringComparison.OrdinalIgnoreCase))
        {
            return DefaultLanguage;
        }

        return SupportedCodes.Contains(value) ? value : null;
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
        if (ReservedRouteSegments.Contains(firstSegment))
        {
            return false;
        }

        var normalized = Normalize(firstSegment);
        if (normalized is null)
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

    private static IReadOnlyList<LanguageInfo> BuildSupportedLanguages()
    {
        return
        [
            CreateLanguageInfo(CultureInfo.GetCultureInfo("zh-CN")),
            CreateLanguageInfo(CultureInfo.GetCultureInfo("zh-TW")),
            CreateLanguageInfo(CultureInfo.GetCultureInfo("en")),
            CreateLanguageInfo(CultureInfo.GetCultureInfo("ja"))
        ];
    }

    private static LanguageInfo CreateLanguageInfo(CultureInfo culture)
    {
        var code = NormalizeCultureName(culture.Name);
        var displayCulture = culture;
        if (culture.IsNeutralCulture)
        {
            displayCulture = culture;
        }

        return new LanguageInfo(
            code,
            displayCulture.Name,
            displayCulture.Name,
            displayCulture.Name.Replace('-', '_'),
            displayCulture.NativeName,
            displayCulture.EnglishName);
    }

    private static string NormalizeCultureName(string cultureName) =>
        cultureName.Replace('_', '-').ToLowerInvariant();
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

        if (originalPath == "/")
        {
            await InvokeWithLanguageAsync(
                context,
                RequestLanguage.DefaultLanguage,
                originalPath,
                originalPath,
                persistCookie: false);
            return;
        }

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

        if (persistCookie && ShouldWriteLanguageCookie(context, language))
        {
            // 语言已写入 Cookie 时不重复 Set-Cookie，减少页面切换后的响应头体积，也避免影响浏览器缓存判断。
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

    private static bool ShouldWriteLanguageCookie(HttpContext context, string language)
    {
        if (!context.Request.Cookies.TryGetValue(RequestLanguage.CookieName, out var existingLanguage))
        {
            return true;
        }

        return !string.Equals(
            RequestLanguage.Normalize(existingLanguage),
            RequestLanguage.Normalize(language),
            StringComparison.OrdinalIgnoreCase);
    }
}
