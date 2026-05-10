using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using WebApp.Extensions;
using WebApp.Models;
using WebApp.Options;
using CodeWfLogger = CodeWF.Log.Core.Logger;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;

namespace WebApp.Services;

public class AppService : IDisposable
{
    private const int SearchCacheLimit = 20;
    private const int SearchQueryStatsLimit = 50;
    private const string SearchKeywordsFileName = "search-keywords.json";
    private const string BlockedSearchNotice = "这个搜索词不适合展示，请换一个技术关键词。";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static readonly JsonSerializerOptions WriteJsonOptions = new(JsonOptions)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private static readonly Regex LocalizedBlogPostFileNameRegex = new(
        @"^(?<slug>.+)\.(?<timestamp>\d{14})\.md$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex LegacyLocalizedBlogPostFileNameRegex = new(
        @"^(?<slug>.+)\.(?<timestamp>\d{14})\.(?<language>[a-z]{2,3}(?:-[a-z0-9]+)*)\.md$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private List<DocItem>? _docItems;
    private List<ToolItem>? _toolItems;
    private readonly ConcurrentDictionary<string, List<DocItem>> _docItemsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<ToolItem>> _toolItemsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, (string? Markdown, string? HtmlContent)> _aboutByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, (string? Markdown, string? HtmlContent)> _donationByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private List<AlbumItem>? _albumItems;
    private List<CategoryItem>? _categoryItems;
    private readonly ConcurrentDictionary<string, List<AlbumItem>> _albumItemsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<CategoryItem>> _categoryItemsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private List<SearchBlockedKeywordGroup>? _searchBlockedKeywordGroups;
    private IReadOnlyList<string>? _searchBlockedKeywords;
    private readonly ConcurrentDictionary<string, List<SearchBlockedKeywordGroup>> _searchBlockedKeywordGroupsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IReadOnlyList<string>> _searchBlockedKeywordsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private List<BlogPost>? _blogPosts;
    private readonly ConcurrentDictionary<string, List<BlogPost>> _blogPostsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private List<FriendLinkItem>? _friendLinkItems;
    private List<TimeLineItem>? _timeLineItems;
    private readonly ConcurrentDictionary<string, List<FriendLinkItem>> _friendLinkItemsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<TimeLineItem>> _timeLineItemsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string>? _webSiteCountInfos;
    private string? _donationMarkdown;
    private string? _donationHtmlContent;
    private string? _aboutMarkdown;
    private string? _aboutHtmlContent;
    private string? _rss;
    private string? _siteMap;
    private List<SearchableEntry>? _searchIndex;
    private readonly ConcurrentDictionary<string, List<SearchableEntry>> _searchIndexByLanguage = new(StringComparer.OrdinalIgnoreCase);

    private sealed record SearchField(string? Text, int ExactScore, int PrefixScore, int ContainsScore);
    private sealed record SearchableToolNode(ToolItem Item, string? GroupName);
    private sealed record SearchableDocNode(DocItem Item, string? ContextLabel, string? RelativeDir);
    private sealed record SearchableEntry(
        SearchResultKind Kind,
        string Title,
        string Url,
        string? Context,
        string? Slug,
        DateTime? UpdatedAt,
        IReadOnlyList<SearchField> Fields,
        IReadOnlyList<string?> SummaryCandidates,
        IReadOnlyList<string?> SnippetCandidates);

    private sealed record SearchSnapshot(
        List<SearchResultItem> OrderedResults,
        int ToolCount,
        int DocCount,
        int PostCount);

    private sealed record SearchCacheEntry(SearchSnapshot Snapshot, int HitCount, DateTimeOffset LastUsedAt);
    private sealed record SearchQueryStats(string Query, int Count, DateTimeOffset LastSearchedAt);
    private sealed record BlogPostMetadataTranslation(
        string? Title,
        string? Description,
        List<string>? Albums,
        List<string>? Categories,
        List<string>? Tags);

    private readonly SemaphoreSlim _searchIndexLock = new(1, 1);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _searchQueryStatsFileLocks = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SearchCacheEntry> _searchCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, SearchQueryStats>> _searchQueryStatsByLanguage = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, byte> _searchQueryStatsLoadedLanguages = new(StringComparer.OrdinalIgnoreCase);
    private readonly IOptions<SiteOption> siteOption;
    private readonly IWebHostEnvironment environment;
    private readonly IContentTranslationService translationService;
    private readonly ILogger<AppService> logger;
    private readonly object _assetWatcherGate = new();
    private FileSystemWatcher? _assetWatcher;
    private Timer? _assetWatcherDebounceTimer;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _localizedAssetLocks = new(StringComparer.OrdinalIgnoreCase);

    public AppService(
        IOptions<SiteOption> siteOption,
        IWebHostEnvironment environment,
        IContentTranslationService? translationService = null,
        ILogger<AppService>? logger = null)
    {
        this.siteOption = siteOption;
        this.environment = environment;
        this.translationService = translationService ?? NullContentTranslationService.Instance;
        this.logger = logger ?? NullLogger<AppService>.Instance;

        // 仅在开发环境监听资源仓库，便于改 Markdown/JSON 后即时刷新站点内容。
        InitializeAssetWatcher();
    }

    private string? GetLocalAssetsDir()
    {
        return ResolveConfiguredDirectory(siteOption.Value.LocalAssetsDir);
    }

    private string? GetI18nResourcesDir()
    {
        return ResolveConfiguredDirectory(siteOption.Value.I18nResourcesDir);
    }

    private string? ResolveConfiguredDirectory(string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return null;
        }

        var expandedPath = System.Environment.ExpandEnvironmentVariables(configuredPath.Trim());
        var path = Path.IsPathRooted(expandedPath)
            ? expandedPath
            : Path.Combine(environment.ContentRootPath, expandedPath);

        return Path.GetFullPath(path);
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

    private string? GetI18nAssetPath(string language, params string[] paths) =>
        GetI18nAssetPath(language, false, paths);

    private void InitializeAssetWatcher()
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        var localAssetsDir = GetLocalAssetsDir();
        if (string.IsNullOrWhiteSpace(localAssetsDir) || !Directory.Exists(localAssetsDir))
        {
            return;
        }

        _assetWatcher = new FileSystemWatcher(localAssetsDir)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
                           | NotifyFilters.DirectoryName
                           | NotifyFilters.LastWrite
                           | NotifyFilters.CreationTime
                           | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        _assetWatcher.Changed += OnAssetChanged;
        _assetWatcher.Created += OnAssetChanged;
        _assetWatcher.Deleted += OnAssetChanged;
        _assetWatcher.Renamed += OnAssetChanged;
    }

    private void OnAssetChanged(object sender, FileSystemEventArgs args)
    {
        if (!ShouldInvalidateContentCaches(args.FullPath))
        {
            return;
        }

        lock (_assetWatcherGate)
        {
            // 文件保存时往往会触发多次事件，这里做一次轻量去抖，避免反复清空缓存。
            _assetWatcherDebounceTimer ??= new Timer(_ => InvalidateContentCaches(), null, Timeout.Infinite, Timeout.Infinite);
            _assetWatcherDebounceTimer.Change(300, Timeout.Infinite);
        }
    }

    private bool ShouldInvalidateContentCaches(string? fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return false;
        }

        var extension = Path.GetExtension(fullPath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        var interestingExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".md",
            ".yml",
            ".yaml",
            ".json",
            ".png",
            ".jpg",
            ".jpeg",
            ".webp",
            ".svg"
        };

        if (!interestingExtensions.Contains(extension))
        {
            return false;
        }

        // 搜索热词文件会在正常搜索时持续更新，不应该因此把整站内容缓存全部打掉。
        return !IsSearchKeywordsFileName(Path.GetFileName(fullPath));
    }

    private static bool IsSearchKeywordsFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        if (string.Equals(fileName, SearchKeywordsFileName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return fileName.StartsWith("search-keywords.", StringComparison.OrdinalIgnoreCase)
            && fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    private void InvalidateContentCaches()
    {
        _docItems = null;
        _toolItems = null;
        _docItemsByLanguage.Clear();
        _toolItemsByLanguage.Clear();
        _aboutByLanguage.Clear();
        _donationByLanguage.Clear();
        _albumItems = null;
        _categoryItems = null;
        _albumItemsByLanguage.Clear();
        _categoryItemsByLanguage.Clear();
        _searchBlockedKeywordGroups = null;
        _searchBlockedKeywords = null;
        _blogPosts = null;
        _blogPostsByLanguage.Clear();
        _friendLinkItems = null;
        _timeLineItems = null;
        _friendLinkItemsByLanguage.Clear();
        _timeLineItemsByLanguage.Clear();
        _webSiteCountInfos = null;
        _donationMarkdown = null;
        _donationHtmlContent = null;
        _aboutMarkdown = null;
        _aboutHtmlContent = null;
        _rss = null;
        _siteMap = null;
        _searchIndex = null;
        _searchIndexByLanguage.Clear();
        _searchCache.Clear();
        _searchBlockedKeywordGroupsByLanguage.Clear();
        _searchBlockedKeywordsByLanguage.Clear();
    }

    private string? GetAssetPath(params string[] paths)
    {
        var localAssetsDir = GetLocalAssetsDir();
        if (localAssetsDir is null)
        {
            return null;
        }

        var segments = new string[paths.Length + 1];
        segments[0] = localAssetsDir;
        Array.Copy(paths, 0, segments, 1, paths.Length);

        return Path.Combine(segments);
    }

    private Task<string?> GetLocalizedDocAssetPathAsync(string language, params string[] paths) =>
        GetLocalizedDocAssetPathAsync(language, true, paths);

    private Task<string?> GetLocalizedDocAssetPathAsync(string language, bool createMissingTranslation, params string[] paths)
    {
        var sourceSegments = new string[paths.Length + 2];
        sourceSegments[0] = "site";
        sourceSegments[1] = "doc";
        Array.Copy(paths, 0, sourceSegments, 2, paths.Length);

        return GetOrCreateLocalizedAssetPathAsync(language, sourceSegments, createMissingTranslation);
    }

    private Task<string?> GetLocalizedSiteAssetPathAsync(string language, params string[] paths)
    {
        var sourceSegments = new string[paths.Length + 1];
        sourceSegments[0] = "site";
        Array.Copy(paths, 0, sourceSegments, 1, paths.Length);

        return GetOrCreateLocalizedAssetPathAsync(language, sourceSegments);
    }

    private Task<string?> GetLocalizedPayAssetPathAsync(string language, string fileName) =>
        GetOrCreateLocalizedAssetPathAsync(language, ["site", "pays", fileName]);

    private Task<string?> GetLocalizedToolAssetPathAsync(string language, bool createMissingTranslation = true) =>
        GetOrCreateLocalizedAssetPathAsync(language, ["site", "tools", "tools.json"], createMissingTranslation);

    private async Task<string?> GetOrCreateLocalizedAssetPathAsync(
        string language,
        string[] sourceSegments,
        bool createMissingTranslation = true)
    {
        var sourcePath = GetAssetPath(sourceSegments);
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            return null;
        }

        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        if (RequestLanguage.IsDefaultLanguage(normalizedLanguage))
        {
            return sourcePath;
        }

        var targetPath = GetI18nAssetPath(normalizedLanguage, createCultureDirectory: false, sourceSegments);
        if (targetPath is null)
        {
            return sourcePath;
        }

        if (File.Exists(targetPath))
        {
            return targetPath;
        }

        var kind = GetContentTranslationKind(sourcePath);
        if (kind is null)
        {
            return sourcePath;
        }

        if (!createMissingTranslation)
        {
            return sourcePath;
        }

        var gate = _localizedAssetLocks.GetOrAdd(targetPath, _ => new SemaphoreSlim(1, 1));
        var stopwatch = Stopwatch.StartNew();
        CodeWfLogger.Info(
            $"语言资源文件准备开始。language={normalizedLanguage}; kind={kind}; source={sourcePath}; target={targetPath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        logger.LogInformation(
            "Localized asset translation started. Language={Language}; Kind={Kind}; Source={SourcePath}; Target={TargetPath}.",
            normalizedLanguage,
            kind,
            sourcePath,
            targetPath);
        await gate.WaitAsync();
        try
        {
            if (File.Exists(targetPath))
            {
                stopwatch.Stop();
                CodeWfLogger.Info(
                    $"语言资源文件已由其他请求生成。language={normalizedLanguage}; kind={kind}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetPath}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                logger.LogInformation(
                    "Localized asset already exists after waiting. Language={Language}; Kind={Kind}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    normalizedLanguage,
                    kind,
                    targetPath,
                    stopwatch.ElapsedMilliseconds);
                return targetPath;
            }

            var source = await File.ReadAllTextAsync(sourcePath);
            var translated = await translationService.TranslateAsync(
                source,
                RequestLanguage.GetLanguage(normalizedLanguage),
                kind.Value,
                resourceName: targetPath);

            if (string.IsNullOrWhiteSpace(translated))
            {
                stopwatch.Stop();
                var message = $"语言资源文件未生成。language={normalizedLanguage}; kind={kind}; elapsedMs={stopwatch.ElapsedMilliseconds}; source={sourcePath}; target={targetPath}.";
                CodeWfLogger.Warn(
                    message,
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                logger.LogWarning(
                    "Localized asset translation returned empty result. Language={Language}; Kind={Kind}; Source={SourcePath}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    normalizedLanguage,
                    kind,
                    sourcePath,
                    targetPath,
                    stopwatch.ElapsedMilliseconds);
                throw new InvalidOperationException(message);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            await File.WriteAllTextAsync(targetPath, translated);
            stopwatch.Stop();
            CodeWfLogger.Info(
                $"语言资源文件生成完成。language={normalizedLanguage}; kind={kind}; outputChars={translated.Length}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetPath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            logger.LogInformation(
                "Localized asset saved. Language={Language}; Kind={Kind}; Source={SourcePath}; Target={TargetPath}; OutputChars={OutputChars}; ElapsedMs={ElapsedMs}.",
                normalizedLanguage,
                kind,
                sourcePath,
                targetPath,
                translated.Length,
                stopwatch.ElapsedMilliseconds);
            return targetPath;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            CodeWfLogger.Error(
                $"语言资源文件生成失败。language={normalizedLanguage}; kind={kind}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetPath}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            logger.LogError(
                ex,
                "Localized asset translation failed. Language={Language}; Kind={Kind}; Source={SourcePath}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                normalizedLanguage,
                kind,
                sourcePath,
                targetPath,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            gate.Release();
        }
    }

    private static ContentTranslationKind? GetContentTranslationKind(string sourcePath) =>
        Path.GetExtension(sourcePath).ToLowerInvariant() switch
        {
            ".md" => ContentTranslationKind.MarkdownPage,
            ".json" => ContentTranslationKind.JsonResource,
            _ => null
        };

    public async Task SeedAsync()
    {
        // 统一在启动阶段把高频数据源读入内存，后续页面请求尽量只做拼装。
        await GetAllAlbumItemsAsync();
        await GetAllCategoryItemsAsync();
        await GetSearchBlockedKeywordGroupsAsync();
        await LoadSearchQueryStatsAsync();
        await GetAllBlogPostsAsync();
        await GetAllFriendLinkItemsAsync();
        await GetTimeLineItemsAsync();
        await GetAllDocItemsAsync();
        await GetAllToolItemsAsync();
        await GetWebSiteCountAsync();
        await ReadAboutAsync();
        await ReadDonationAsync();
        await GetRssAsync();
        await GetSiteMapAsync();
    }

    public Task<List<DocItem>?> GetAllDocItemsAsync() =>
        GetDocItemsAsync(RequestLanguage.CurrentLanguage, createMissingTranslation: true);

    public async Task<int> GetDefaultDocNodeCountAsync()
    {
        var docItems = await GetDocItemsAsync(RequestLanguage.DefaultLanguage, createMissingTranslation: false) ?? [];
        return docItems.Count + docItems.Sum(static item => item.Children?.Count ?? 0);
    }

    private async Task<List<DocItem>?> GetDocItemsAsync(string language, bool createMissingTranslation)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        if (RequestLanguage.IsDefaultLanguage(normalizedLanguage) && _docItems?.Any() == true)
        {
            return _docItems;
        }

        _docItemsByLanguage.TryGetValue(normalizedLanguage, out var localizedItems);
        if (createMissingTranslation && localizedItems?.Any() == true)
        {
            return localizedItems;
        }

        var sourcePath = GetAssetPath("site", "doc", "navigation.json");
        var filePath = await GetLocalizedDocAssetPathAsync(normalizedLanguage, createMissingTranslation, "navigation.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return RequestLanguage.IsDefaultLanguage(normalizedLanguage)
                ? _docItems
                : localizedItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            var items = JsonSerializer.Deserialize<List<DocItem>>(fileContent, JsonOptions) ?? [];
            if (RequestLanguage.IsDefaultLanguage(normalizedLanguage))
            {
                _docItems = items;
            }
            else if (!string.Equals(filePath, sourcePath, StringComparison.OrdinalIgnoreCase))
            {
                _docItemsByLanguage[normalizedLanguage] = items;
            }

            return items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize localized doc/navigation.json: {ex.Message}");
        }

        return RequestLanguage.IsDefaultLanguage(normalizedLanguage)
            ? _docItems
            : localizedItems;
    }

    public Task<List<ToolItem>?> GetAllToolItemsAsync() =>
        GetToolItemsAsync(RequestLanguage.CurrentLanguage, createMissingTranslation: true);

    public async Task<int> GetDefaultToolEntryCountAsync()
    {
        var toolItems = await GetToolItemsAsync(RequestLanguage.DefaultLanguage, createMissingTranslation: false) ?? [];
        return toolItems.Sum(static item => Math.Max(1, item.Children?.Count ?? 0));
    }

    private async Task<List<ToolItem>?> GetToolItemsAsync(string language, bool createMissingTranslation)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        if (RequestLanguage.IsDefaultLanguage(normalizedLanguage) && _toolItems?.Any() == true)
        {
            return _toolItems;
        }

        _toolItemsByLanguage.TryGetValue(normalizedLanguage, out var localizedItems);
        if (createMissingTranslation && localizedItems?.Any() == true)
        {
            return localizedItems;
        }

        var sourcePath = GetAssetPath("site", "tools", "tools.json");
        var filePath = await GetLocalizedToolAssetPathAsync(normalizedLanguage, createMissingTranslation);
        if (filePath is null || !File.Exists(filePath))
        {
            return RequestLanguage.IsDefaultLanguage(normalizedLanguage)
                ? _toolItems
                : localizedItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            var items = JsonSerializer.Deserialize<List<ToolItem>>(fileContent, JsonOptions) ?? [];
            if (RequestLanguage.IsDefaultLanguage(normalizedLanguage))
            {
                _toolItems = items;
            }
            else if (!string.Equals(filePath, sourcePath, StringComparison.OrdinalIgnoreCase))
            {
                _toolItemsByLanguage[normalizedLanguage] = items;
            }

            return items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize tools.json: {ex.Message}");
        }

        return RequestLanguage.IsDefaultLanguage(normalizedLanguage)
            ? _toolItems
            : localizedItems;
    }

    public async Task<DocItem?> GetDocItemAsync(string slug)
    {
        var docItems = await GetAllDocItemsAsync();
        if (docItems?.Any() != true)
        {
            return default;
        }

        foreach (var item in docItems)
        {
            if (!string.IsNullOrWhiteSpace(item.Slug) && item.Slug == slug)
            {
                await LoadDocContentAsync(item);
                return item;
            }

            if (item.Children?.Any() != true) continue;

            foreach (var itemChild in item.Children.Where(itemChild => itemChild.Slug == slug))
            {
                await LoadDocContentAsync(itemChild, item.Slug);
                return itemChild;
            }
        }

        var first = docItems.FirstOrDefault()?.Children?.FirstOrDefault();
        if (first != null)
        {
            await LoadDocContentAsync(first);
        }

        return first;
    }

    public ToolItem? GetToolItem(string slug)
    {
        if (_toolItems?.Any() != true)
        {
            return default;
        }

        foreach (var item in _toolItems)
        {
            if (item.Children?.Any() != true) continue;

            foreach (var child in item.Children.Where(child =>
                         string.Equals(child.Slug, slug, StringComparison.InvariantCultureIgnoreCase)))
            {
                return child;
            }
        }

        return default;
    }

    public async Task<SearchResultPageData> SearchAsync(string? query, int pageIndex, int pageSize, SearchResultKind? kind = null)
    {
        var normalizedQuery = NormalizeSearchQuery(query);
        var safePageIndex = Math.Max(1, pageIndex);
        var safePageSize = pageSize <= 0 ? 10 : pageSize;

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return new SearchResultPageData(safePageIndex, safePageSize, 0, [], 0, 0, 0);
        }

        if (await IsBlockedSearchQueryAsync(normalizedQuery))
        {
            return new SearchResultPageData(
                safePageIndex,
                safePageSize,
                0,
                [],
                0,
                0,
                0,
                true,
                BlockedSearchNotice);
        }

        await TrackSearchQueryAsync(normalizedQuery);

        var cacheKey = $"{RequestLanguage.CurrentLanguage}:{normalizedQuery}";
        if (_searchCache.TryGetValue(cacheKey, out var cacheEntry))
        {
            // 站内搜索的查询模式很容易重复，命中缓存时直接复用排序后的快照。
            TouchSearchCacheEntry(cacheKey, cacheEntry);
            return CreateSearchPageData(cacheEntry.Snapshot, safePageIndex, safePageSize, kind);
        }

        var snapshot = await BuildSearchSnapshotAsync(normalizedQuery);
        _searchCache[cacheKey] = new SearchCacheEntry(snapshot, 1, DateTimeOffset.UtcNow);
        TrimSearchCache();

        return CreateSearchPageData(snapshot, safePageIndex, safePageSize, kind);
    }

    public async Task<List<SearchSuggestionItem>> GetSearchSuggestionsAsync(string? query, int size = 10)
    {
        var normalizedQuery = NormalizeSearchQuery(query);
        var safeSize = Math.Clamp(size, 1, 20);

        if (await IsBlockedSearchQueryAsync(normalizedQuery))
        {
            return [];
        }

        var suggestions = new List<SearchSuggestionItem>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 优先返回用户真实搜索过的热词，建议列表会比纯标题匹配更贴近日常使用。
        var language = RequestLanguage.Normalize(RequestLanguage.CurrentLanguage) ?? RequestLanguage.DefaultLanguage;
        await LoadSearchQueryStatsAsync(language);
        foreach (var item in GetSearchQueryStats(language).Values
                     .Where(item => string.IsNullOrWhiteSpace(normalizedQuery)
                         || item.Query.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                     .OrderByDescending(item => item.Count)
                     .ThenByDescending(item => item.LastSearchedAt)
            .Take(safeSize))
        {
            if (seen.Add(item.Query))
            {
                suggestions.Add(new SearchSuggestionItem(item.Query, GetHotSearchLabel(), item.Count));
            }
        }

        if (suggestions.Count >= safeSize || normalizedQuery.Length < 1)
        {
            return suggestions;
        }

        // 热词不足时再从内存索引补齐，避免输入建议完全依赖历史查询数据。
        var index = await GetSearchIndexAsync();
        foreach (var entry in index
                     .Where(entry => entry.Title.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                         || (!string.IsNullOrWhiteSpace(entry.Slug)
                             && entry.Slug.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)))
                     .OrderBy(entry => entry.Kind)
                     .ThenBy(entry => entry.Title, StringComparer.CurrentCultureIgnoreCase))
        {
            if (!seen.Add(entry.Title))
            {
                continue;
            }

            suggestions.Add(new SearchSuggestionItem(entry.Title, GetSuggestionLabel(entry.Kind), 0));
            if (suggestions.Count >= safeSize)
            {
                break;
            }
        }

        return suggestions;
    }

    private async Task<SearchSnapshot> BuildSearchSnapshotAsync(string normalizedQuery)
    {
        var tokens = SplitSearchTokens(normalizedQuery);
        if (tokens.Count == 0)
        {
            return new SearchSnapshot([], 0, 0, 0);
        }

        var results = new List<SearchResultItem>();
        var index = await GetSearchIndexAsync();

        // 搜索结果先统一打分，再在同一种实体内按相关度和更新时间排序。
        foreach (var entry in index)
        {
            var score = CalculateSearchScore(normalizedQuery, tokens, entry.Fields);
            if (score <= 0)
            {
                continue;
            }

            var summary = SelectSummary(normalizedQuery, entry.SummaryCandidates);
            var matchedSnippet = SelectSnippet(normalizedQuery, entry.SnippetCandidates);

            results.Add(new SearchResultItem
            {
                Kind = entry.Kind,
                Title = entry.Title,
                Url = entry.Url,
                Summary = summary,
                MatchedSnippet = matchedSnippet,
                Context = entry.Context,
                Slug = entry.Slug,
                UpdatedAt = entry.UpdatedAt,
                Score = score
            });
        }

        var ordered = results
            .OrderBy(result => result.Kind)
            .ThenByDescending(result => result.Score)
            .ThenByDescending(result => result.UpdatedAt ?? DateTime.MinValue)
            .ThenBy(result => result.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new SearchSnapshot(
            ordered,
            ordered.Count(result => result.Kind == SearchResultKind.Tool),
            ordered.Count(result => result.Kind == SearchResultKind.Doc),
            ordered.Count(result => result.Kind == SearchResultKind.Post));
    }

    private async Task<List<SearchableEntry>> GetSearchIndexAsync()
    {
        var language = RequestLanguage.CurrentLanguage;
        if (_searchIndexByLanguage.TryGetValue(language, out var localizedIndex))
        {
            return localizedIndex;
        }

        await _searchIndexLock.WaitAsync();
        try
        {
            if (_searchIndexByLanguage.TryGetValue(language, out localizedIndex))
            {
                return localizedIndex;
            }

            var blogPosts = await GetAllBlogPostsAsync() ?? [];
            var docItems = await GetAllDocItemsAsync();
            var toolItems = await GetAllToolItemsAsync();

            // 把文章、文档、工具统一拉平成一套可搜索条目，后续搜索只面对内存索引。
            var index = new List<SearchableEntry>();
            foreach (var tool in GetSearchableToolNodes(toolItems))
            {
                index.Add(new SearchableEntry(
                    SearchResultKind.Tool,
                    tool.Item.Name?.Trim() ?? "未命名工具",
                    ConstantUtil.GetToolUrl(tool.Item.Slug),
                    tool.GroupName,
                    tool.Item.Slug,
                    null,
                    [
                        new SearchField(tool.Item.Name, 220, 170, 130),
                        new SearchField(tool.Item.Slug, 180, 140, 100),
                        new SearchField(tool.Item.Memo, 90, 60, 36),
                        new SearchField(tool.GroupName, 70, 45, 24)
                    ],
                    [tool.Item.Memo, tool.GroupName],
                    [tool.Item.Memo, tool.GroupName]));
            }

            var searchableDocs = await GetSearchableDocNodesAsync(docItems);
            foreach (var doc in searchableDocs)
            {
                var plainContent = CleanSearchText(doc.Item.Content);
                index.Add(new SearchableEntry(
                    SearchResultKind.Doc,
                    doc.Item.Name?.Trim() ?? "未命名项目",
                    ConstantUtil.GetDocUrl(doc.Item.Slug),
                    doc.ContextLabel,
                    doc.Item.Slug,
                    null,
                    [
                        new SearchField(doc.Item.Name, 220, 170, 130),
                        new SearchField(doc.Item.Slug, 180, 140, 100),
                        new SearchField(doc.Item.Memo, 90, 60, 36),
                        new SearchField(doc.ContextLabel, 70, 45, 24),
                        new SearchField(plainContent, 26, 18, 12)
                    ],
                    [doc.Item.Memo, plainContent],
                    [doc.Item.Memo, plainContent]));
            }

            foreach (var post in blogPosts)
            {
                var plainContent = CleanSearchText(post.Content);
                var categoryText = string.Join(" / ", post.Categories?.Where(static item => !string.IsNullOrWhiteSpace(item)) ?? []);
                var albumText = string.Join(" / ", post.Albums?.Where(static item => !string.IsNullOrWhiteSpace(item)) ?? []);
                var tagText = string.Join(" / ", post.Tags?.Where(static item => !string.IsNullOrWhiteSpace(item)) ?? []);
                var context = !string.IsNullOrWhiteSpace(categoryText)
                    ? categoryText
                    : !string.IsNullOrWhiteSpace(albumText)
                        ? albumText
                        : null;

                index.Add(new SearchableEntry(
                    SearchResultKind.Post,
                    post.Title?.Trim() ?? "未命名文章",
                    ConstantUtil.GetPostUrl(post),
                    context,
                    post.Slug,
                    post.Lastmod ?? post.Date,
                    [
                        new SearchField(post.Title, 230, 180, 135),
                        new SearchField(post.Slug, 185, 145, 105),
                        new SearchField(post.Description, 95, 65, 40),
                        new SearchField(categoryText, 75, 48, 28),
                        new SearchField(albumText, 60, 42, 24),
                        new SearchField(tagText, 60, 42, 24),
                        new SearchField(post.Author, 48, 36, 22),
                        new SearchField(plainContent, 28, 20, 14)
                    ],
                    [post.Description, plainContent],
                    [post.Description, plainContent]));
            }

            _searchIndexByLanguage[language] = index;
            if (string.Equals(language, RequestLanguage.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                _searchIndex = index;
            }

            return index;
        }
        finally
        {
            _searchIndexLock.Release();
        }
    }

    private static SearchResultPageData CreateSearchPageData(
        SearchSnapshot snapshot,
        int pageIndex,
        int pageSize,
        SearchResultKind? kind)
    {
        var filtered = kind.HasValue
            ? snapshot.OrderedResults.Where(result => result.Kind == kind.Value).ToList()
            : snapshot.OrderedResults;
        var totalCount = filtered.Count;
        var totalPages = totalCount <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var currentPageIndex = totalPages <= 0
            ? 1
            : Math.Min(pageIndex, totalPages);
        var data = filtered
            .Skip((currentPageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new SearchResultPageData(
            currentPageIndex,
            pageSize,
            totalCount,
            data,
            snapshot.ToolCount,
            snapshot.DocCount,
            snapshot.PostCount);
    }

    private static string NormalizeSearchQuery(string? query) =>
        Regex.Replace(query?.Trim() ?? string.Empty, @"\s+", " ");

    private async Task<bool> IsBlockedSearchQueryAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return false;
        }

        var keywords = await GetSearchBlockedKeywordsAsync();
        return ContainsBlockedSearchKeyword(query, keywords);
    }

    private static bool ContainsBlockedSearchKeyword(string query, IReadOnlyList<string> keywords) =>
        keywords.Any(keyword => query.Contains(keyword, StringComparison.OrdinalIgnoreCase));

    private async Task<IReadOnlyList<string>> GetSearchBlockedKeywordsAsync()
    {
        return await GetSearchBlockedKeywordsAsync(RequestLanguage.CurrentLanguage);
    }

    private async Task<IReadOnlyList<string>> GetSearchBlockedKeywordsAsync(string language)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        if (_searchBlockedKeywordsByLanguage.TryGetValue(normalizedLanguage, out var localizedKeywords))
        {
            return localizedKeywords;
        }

        var groups = await GetSearchBlockedKeywordGroupsAsync(normalizedLanguage);
        var keywords = groups
            .SelectMany(static group => group.Keywords ?? [])
            .Select(NormalizeSearchQuery)
            .Where(static keyword => !string.IsNullOrWhiteSpace(keyword))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(static keyword => keyword.Length)
            .ThenBy(static keyword => keyword, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        _searchBlockedKeywordsByLanguage[normalizedLanguage] = keywords;
        if (RequestLanguage.IsDefaultLanguage(normalizedLanguage))
        {
            _searchBlockedKeywords = keywords;
        }

        return keywords;
    }

    private async Task TrackSearchQueryAsync(string normalizedQuery)
    {
        var language = RequestLanguage.Normalize(RequestLanguage.CurrentLanguage) ?? RequestLanguage.DefaultLanguage;
        await LoadSearchQueryStatsAsync(language);

        var now = DateTimeOffset.UtcNow;
        var searchQueryStats = GetSearchQueryStats(language);
        CodeWfLogger.Info(
            $"搜索热词记录开始。language={language}; query={normalizedQuery}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        searchQueryStats.AddOrUpdate(
            normalizedQuery,
            new SearchQueryStats(normalizedQuery, 1, now),
            (_, current) => current with
            {
                Count = current.Count + 1,
                LastSearchedAt = now
            });

        TrimSearchQueryStats(searchQueryStats);
        await SaveSearchQueryStatsAsync(language);
    }

    private Task LoadSearchQueryStatsAsync() =>
        LoadSearchQueryStatsAsync(RequestLanguage.CurrentLanguage);

    private async Task LoadSearchQueryStatsAsync(string language)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        if (_searchQueryStatsLoadedLanguages.ContainsKey(normalizedLanguage))
        {
            return;
        }

        var filePath = GetSearchKeywordsFilePath(normalizedLanguage);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _searchQueryStatsLoadedLanguages[normalizedLanguage] = 1;
            CodeWfLogger.Warn(
                $"搜索热词文件路径为空，跳过加载。language={normalizedLanguage}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return;
        }

        var blockedKeywords = await GetSearchBlockedKeywordsAsync(normalizedLanguage);
        var searchQueryStats = GetSearchQueryStats(normalizedLanguage);
        var gate = _searchQueryStatsFileLocks.GetOrAdd(normalizedLanguage, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync();
        try
        {
            if (_searchQueryStatsLoadedLanguages.ContainsKey(normalizedLanguage))
            {
                return;
            }

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            CodeWfLogger.Info(
                $"搜索热词加载开始。language={normalizedLanguage}; file={filePath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);

            var readFilePath = filePath;
            if (!File.Exists(filePath))
            {
                var legacyFilePath = GetLegacySearchKeywordsFilePath(normalizedLanguage);
                if (!string.IsNullOrWhiteSpace(legacyFilePath)
                    && File.Exists(legacyFilePath))
                {
                    readFilePath = legacyFilePath;
                    CodeWfLogger.Info(
                        $"搜索热词使用旧默认文件加载，将在下次保存到语言文件。language={normalizedLanguage}; legacyFile={legacyFilePath}; file={filePath}.",
                        log2UI: false,
                        log2File: false,
                        log2Console: true);
                }
                else
                {
                    await File.WriteAllTextAsync(filePath, "[]", Encoding.UTF8);
                    _searchQueryStatsLoadedLanguages[normalizedLanguage] = 1;
                    CodeWfLogger.Info(
                        $"搜索热词文件不存在，已创建空文件。language={normalizedLanguage}; file={filePath}.",
                        log2UI: false,
                        log2File: false,
                        log2Console: true);
                    return;
                }
            }

            var fileContent = await File.ReadAllTextAsync(readFilePath);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                _searchQueryStatsLoadedLanguages[normalizedLanguage] = 1;
                CodeWfLogger.Info(
                    $"搜索热词文件为空。language={normalizedLanguage}; file={readFilePath}.",
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                return;
            }

            List<SearchQueryStats>? savedStats;
            try
            {
                savedStats = JsonSerializer.Deserialize<List<SearchQueryStats>>(fileContent, JsonOptions);
            }
            catch (Exception ex)
            {
                CodeWfLogger.Error(
                    $"搜索热词反序列化失败。language={normalizedLanguage}; file={readFilePath}.",
                    ex,
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                Console.WriteLine($"Failed to deserialize {Path.GetFileName(readFilePath)}: {ex.Message}");
                _searchQueryStatsLoadedLanguages[normalizedLanguage] = 1;
                return;
            }

            foreach (var item in savedStats ?? [])
            {
                var query = NormalizeSearchQuery(item.Query);
                if (string.IsNullOrWhiteSpace(query) || ContainsBlockedSearchKeyword(query, blockedKeywords))
                {
                    continue;
                }

                var count = Math.Max(1, item.Count);
                var lastSearchedAt = item.LastSearchedAt == default ? DateTimeOffset.UtcNow : item.LastSearchedAt;
                searchQueryStats.AddOrUpdate(
                    query,
                    new SearchQueryStats(query, count, lastSearchedAt),
                    (_, current) => current.Count > count
                                    || (current.Count == count && current.LastSearchedAt >= lastSearchedAt)
                        ? current
                        : new SearchQueryStats(query, count, lastSearchedAt));
            }

            TrimSearchQueryStats(searchQueryStats);
            _searchQueryStatsLoadedLanguages[normalizedLanguage] = 1;
            CodeWfLogger.Info(
                $"搜索热词加载完成。language={normalizedLanguage}; count={searchQueryStats.Count}; file={readFilePath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
        }
        catch (Exception ex)
        {
            CodeWfLogger.Error(
                $"搜索热词加载失败。language={normalizedLanguage}; file={filePath}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            Console.WriteLine($"Failed to load {Path.GetFileName(filePath)}: {ex.Message}");
            _searchQueryStatsLoadedLanguages[normalizedLanguage] = 1;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task SaveSearchQueryStatsAsync(string language)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        var filePath = GetSearchKeywordsFilePath(normalizedLanguage);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            CodeWfLogger.Warn(
                $"搜索热词文件路径为空，跳过保存。language={normalizedLanguage}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return;
        }

        var searchQueryStats = GetSearchQueryStats(normalizedLanguage);
        var gate = _searchQueryStatsFileLocks.GetOrAdd(normalizedLanguage, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync();
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var data = searchQueryStats.Values
                .OrderByDescending(static item => item.Count)
                .ThenByDescending(static item => item.LastSearchedAt)
                .Take(SearchQueryStatsLimit)
                .ToList();
            var json = JsonSerializer.Serialize(data, WriteJsonOptions);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
            CodeWfLogger.Info(
                $"搜索热词保存完成。language={normalizedLanguage}; count={data.Count}; file={filePath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
        }
        catch (Exception ex)
        {
            CodeWfLogger.Error(
                $"搜索热词保存失败。language={normalizedLanguage}; file={filePath}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            Console.WriteLine($"Failed to write {Path.GetFileName(filePath)}: {ex.Message}");
        }
        finally
        {
            gate.Release();
        }
    }

    private ConcurrentDictionary<string, SearchQueryStats> GetSearchQueryStats(string language)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        return _searchQueryStatsByLanguage.GetOrAdd(
            normalizedLanguage,
            _ => new ConcurrentDictionary<string, SearchQueryStats>(StringComparer.OrdinalIgnoreCase));
    }

    private string? GetSearchKeywordsFilePath(string language)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        if (RequestLanguage.IsDefaultLanguage(normalizedLanguage))
        {
            return GetAssetPath("site", SearchKeywordsFileName);
        }

        return GetI18nAssetPath(normalizedLanguage, true, "site", SearchKeywordsFileName);
    }

    private string? GetLegacySearchKeywordsFilePath(string? language = null)
    {
        var defaultPath = GetAssetPath("site", SearchKeywordsFileName);
        if (string.IsNullOrWhiteSpace(defaultPath))
        {
            return null;
        }

        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        if (RequestLanguage.IsDefaultLanguage(normalizedLanguage))
        {
            return defaultPath;
        }

        var directory = Path.GetDirectoryName(defaultPath) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(defaultPath);
        var extension = Path.GetExtension(defaultPath);
        return Path.Combine(directory, $"{fileName}.{normalizedLanguage}{extension}");
    }

    private void TouchSearchCacheEntry(string normalizedQuery, SearchCacheEntry cacheEntry)
    {
        _searchCache[normalizedQuery] = cacheEntry with
        {
            HitCount = cacheEntry.HitCount + 1,
            LastUsedAt = DateTimeOffset.UtcNow
        };
    }

    private void TrimSearchCache()
    {
        if (_searchCache.Count <= SearchCacheLimit)
        {
            return;
        }

        foreach (var item in _searchCache
                     .OrderBy(item => item.Value.HitCount)
                     .ThenBy(item => item.Value.LastUsedAt)
                     .Take(_searchCache.Count - SearchCacheLimit))
        {
            _searchCache.TryRemove(item.Key, out _);
        }
    }

    private static void TrimSearchQueryStats(ConcurrentDictionary<string, SearchQueryStats> searchQueryStats)
    {
        if (searchQueryStats.Count <= SearchQueryStatsLimit)
        {
            return;
        }

        foreach (var item in searchQueryStats
                     .OrderBy(item => item.Value.Count)
                     .ThenBy(item => item.Value.LastSearchedAt)
                     .Take(searchQueryStats.Count - SearchQueryStatsLimit))
        {
            searchQueryStats.TryRemove(item.Key, out _);
        }
    }

    private static string GetSuggestionLabel(SearchResultKind kind)
    {
        var language = RequestLanguage.CurrentLanguage;
        return language switch
        {
            "en" => kind switch
            {
                SearchResultKind.Tool => "Tool",
                SearchResultKind.Doc => "Project",
                SearchResultKind.Post => "Post",
                _ => "Content"
            },
            "ja" => kind switch
            {
                SearchResultKind.Tool => "ツール",
                SearchResultKind.Doc => "プロジェクト",
                SearchResultKind.Post => "記事",
                _ => "コンテンツ"
            },
            "zh-tw" => kind switch
            {
                SearchResultKind.Tool => "工具",
                SearchResultKind.Doc => "專案",
                SearchResultKind.Post => "文章",
                _ => "內容"
            },
            _ => kind switch
            {
                SearchResultKind.Tool => "工具",
                SearchResultKind.Doc => "项目",
                SearchResultKind.Post => "文章",
                _ => "内容"
            }
        };
    }

    private static string GetHotSearchLabel() => RequestLanguage.CurrentLanguage switch
    {
        "en" => "Hot",
        "ja" => "人気検索",
        "zh-tw" => "熱門",
        _ => "热搜"
    };

    private async Task LoadDocContentAsync(DocItem item, string? parentDir = default)
    {
        if (!string.IsNullOrWhiteSpace(item.Content))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(item.Slug))
        {
            return;
        }

        var language = RequestLanguage.CurrentLanguage;
        var contentPath = string.IsNullOrWhiteSpace(parentDir)
            ? await GetLocalizedDocAssetPathAsync(language, $"{item.Slug}.md")
            : await GetLocalizedDocAssetPathAsync(language, parentDir, $"{item.Slug}.md");

        if (string.IsNullOrWhiteSpace(contentPath) || !File.Exists(contentPath))
        {
            return;
        }

        item.Content = await File.ReadAllTextAsync(contentPath);
        item.HtmlContent = item.Content.ToHtml();
    }

    private IEnumerable<SearchableToolNode> GetSearchableToolNodes(IEnumerable<ToolItem>? toolItems = null)
    {
        var source = toolItems ?? _toolItems;
        if (source?.Any() != true)
        {
            yield break;
        }

        foreach (var item in source)
        {
            if (item.Children?.Any() == true)
            {
                foreach (var child in item.Children.Where(static child =>
                             !string.IsNullOrWhiteSpace(child.Name) || !string.IsNullOrWhiteSpace(child.Slug)))
                {
                    yield return new SearchableToolNode(child, item.Name);
                }

                continue;
            }

            if (!string.IsNullOrWhiteSpace(item.Name) || !string.IsNullOrWhiteSpace(item.Slug))
            {
                yield return new SearchableToolNode(item, null);
            }
        }
    }

    private async Task<List<SearchableDocNode>> GetSearchableDocNodesAsync(IEnumerable<DocItem>? docItems = null)
    {
        var nodes = new List<SearchableDocNode>();
        var source = docItems ?? _docItems;
        if (source?.Any() != true)
        {
            return nodes;
        }

        async Task TraverseAsync(IEnumerable<DocItem> items, string? parentLabel = null, string? parentDir = null)
        {
            foreach (var item in items)
            {
                await LoadDocContentAsync(item, parentDir);
                nodes.Add(new SearchableDocNode(item, parentLabel, parentDir));

                if (item.Children?.Any() != true || string.IsNullOrWhiteSpace(item.Slug))
                {
                    continue;
                }

                var nextParentLabel = string.IsNullOrWhiteSpace(parentLabel)
                    ? item.Name
                    : $"{parentLabel} / {item.Name}";
                var nextParentDir = string.IsNullOrWhiteSpace(parentDir)
                    ? item.Slug
                    : Path.Combine(parentDir, item.Slug);

                await TraverseAsync(item.Children, nextParentLabel, nextParentDir);
            }
        }

        await TraverseAsync(source);
        return nodes;
    }

    private static int CalculateSearchScore(string query, IReadOnlyCollection<string> tokens, IReadOnlyCollection<SearchField> fields)
    {
        if (tokens.Count == 0 || fields.Count == 0)
        {
            return 0;
        }

        var totalScore = 0;

        foreach (var token in tokens)
        {
            var tokenScore = 0;
            foreach (var field in fields)
            {
                tokenScore = Math.Max(tokenScore,
                    GetMatchScore(field.Text, token, field.ExactScore, field.PrefixScore, field.ContainsScore));
            }

            if (tokenScore <= 0)
            {
                return 0;
            }

            totalScore += tokenScore;
        }

        foreach (var field in fields)
        {
            totalScore += GetFullQueryBonus(field.Text, query, field.ExactScore, field.PrefixScore, field.ContainsScore);
        }

        return totalScore;
    }

    private static int GetMatchScore(string? source, string token, int exactScore, int prefixScore, int containsScore)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return 0;
        }

        var text = source.Trim();
        if (text.Equals(token, StringComparison.OrdinalIgnoreCase))
        {
            return exactScore;
        }

        if (text.StartsWith(token, StringComparison.OrdinalIgnoreCase))
        {
            return prefixScore;
        }

        return text.Contains(token, StringComparison.OrdinalIgnoreCase) ? containsScore : 0;
    }

    private static int GetFullQueryBonus(string? source, string query, int exactScore, int prefixScore, int containsScore)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(query))
        {
            return 0;
        }

        var text = source.Trim();
        if (text.Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            return exactScore / 2;
        }

        if (text.StartsWith(query, StringComparison.OrdinalIgnoreCase))
        {
            return prefixScore / 2;
        }

        return text.Contains(query, StringComparison.OrdinalIgnoreCase) ? Math.Max(containsScore / 2, 1) : 0;
    }

    private static List<string> SplitSearchTokens(string? query) =>
        (query ?? string.Empty)
        .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    private static string? SelectSummary(string query, IEnumerable<string?> candidates)
    {
        foreach (var candidate in candidates)
        {
            var snippet = SelectSnippet(query, candidate);
            if (!string.IsNullOrWhiteSpace(snippet))
            {
                return snippet;
            }
        }

        return null;
    }

    private static string? SelectSnippet(string query, IEnumerable<string?> candidates)
    {
        foreach (var candidate in candidates)
        {
            var snippet = SelectSnippet(query, candidate, 130);
            if (!string.IsNullOrWhiteSpace(snippet))
            {
                return snippet;
            }
        }

        return null;
    }

    private static string? SelectSnippet(string query, string? source, int maxLength = 130)
    {
        var cleaned = CleanSearchText(source);
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return null;
        }

        if (cleaned.Length <= maxLength)
        {
            return cleaned;
        }

        var index = cleaned.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            foreach (var token in SplitSearchTokens(query))
            {
                index = cleaned.IndexOf(token, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    break;
                }
            }
        }

        if (index < 0)
        {
            return $"{cleaned[..maxLength].Trim()}...";
        }

        var start = Math.Max(0, index - (maxLength / 3));
        var length = Math.Min(maxLength, cleaned.Length - start);
        var snippet = cleaned.Substring(start, length).Trim();

        if (start > 0)
        {
            snippet = $"...{snippet}";
        }

        if (start + length < cleaned.Length)
        {
            snippet = $"{snippet}...";
        }

        return snippet;
    }

    private static string CleanSearchText(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

        var cleaned = source;
        cleaned = Regex.Replace(cleaned, "<[^>]+>", " ");
        cleaned = Regex.Replace(cleaned, @"!\[[^\]]*\]\([^)]+\)", " ");
        cleaned = Regex.Replace(cleaned, @"\[(.*?)\]\([^)]+\)", "$1");
        cleaned = Regex.Replace(cleaned, @"[`*_>#\-]+", " ");
        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        return cleaned.Trim();
    }

    public async Task<List<AlbumItem>?> GetAllAlbumItemsAsync()
    {
        var language = RequestLanguage.CurrentLanguage;
        if (RequestLanguage.IsDefaultLanguage(language) && _albumItems?.Any() == true)
        {
            return _albumItems;
        }

        if (_albumItemsByLanguage.TryGetValue(language, out var localizedItems) && localizedItems.Any())
        {
            return localizedItems;
        }

        var filePath = await GetLocalizedSiteAssetPathAsync(language, "albums.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return RequestLanguage.IsDefaultLanguage(language)
                ? _albumItems
                : localizedItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        List<AlbumItem>? items = null;
        try
        {
            items = JsonSerializer.Deserialize<List<AlbumItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize albums.json: {ex.Message}");
        }

        items ??= [];
        items.Insert(0, new AlbumItem() { Slug = ConstantUtil.DefaultCategory, Name = GetAllItemsLabel(language) });

        if (RequestLanguage.IsDefaultLanguage(language))
        {
            _albumItems = items;
        }

        _albumItemsByLanguage[language] = items;
        return items;
    }

    public async Task<List<CategoryItem>?> GetAllCategoryItemsAsync()
    {
        var language = RequestLanguage.CurrentLanguage;
        if (RequestLanguage.IsDefaultLanguage(language) && _categoryItems?.Any() == true)
        {
            return _categoryItems;
        }

        if (_categoryItemsByLanguage.TryGetValue(language, out var localizedItems) && localizedItems.Any())
        {
            return localizedItems;
        }

        var filePath = await GetLocalizedSiteAssetPathAsync(language, "categories.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return RequestLanguage.IsDefaultLanguage(language)
                ? _categoryItems
                : localizedItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        List<CategoryItem>? items = null;
        try
        {
            items = JsonSerializer.Deserialize<List<CategoryItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize categories.json: {ex.Message}");
        }

        items ??= [];
        items.Insert(0, new CategoryItem() { Slug = ConstantUtil.DefaultCategory, Name = GetAllItemsLabel(language) });

        if (RequestLanguage.IsDefaultLanguage(language))
        {
            _categoryItems = items;
        }

        _categoryItemsByLanguage[language] = items;
        return items;
    }

    private static string GetAllItemsLabel(string language) => RequestLanguage.Normalize(language) switch
    {
        "en" => "All",
        "ja" => "すべて",
        "zh-tw" => "全部",
        _ => RequestLanguage.IsDefaultLanguage(language) ? "所有" : "All"
    };

    public async Task<List<SearchBlockedKeywordGroup>> GetSearchBlockedKeywordGroupsAsync()
    {
        return await GetSearchBlockedKeywordGroupsAsync(RequestLanguage.CurrentLanguage);
    }

    private async Task<List<SearchBlockedKeywordGroup>> GetSearchBlockedKeywordGroupsAsync(string language)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        if (_searchBlockedKeywordGroupsByLanguage.TryGetValue(normalizedLanguage, out var localizedGroups))
        {
            return localizedGroups;
        }

        var groups = new List<SearchBlockedKeywordGroup>();
        var filePath = await GetLocalizedSiteAssetPathAsync(normalizedLanguage, "blocked-search-keywords.json");
        if (filePath is null || !File.Exists(filePath))
        {
            _searchBlockedKeywordGroupsByLanguage[normalizedLanguage] = groups;
            if (RequestLanguage.IsDefaultLanguage(normalizedLanguage))
            {
                _searchBlockedKeywordGroups = groups;
            }

            CodeWfLogger.Warn(
                $"屏蔽搜索词文件不存在。language={normalizedLanguage}; file={filePath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return groups;
        }

        CodeWfLogger.Info(
            $"屏蔽搜索词加载开始。language={normalizedLanguage}; file={filePath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            groups = JsonSerializer.Deserialize<List<SearchBlockedKeywordGroup>>(fileContent, JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            CodeWfLogger.Error(
                $"屏蔽搜索词反序列化失败。language={normalizedLanguage}; file={filePath}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            Console.WriteLine($"Failed to deserialize blocked-search-keywords.json: {ex.Message}");
            groups = [];
        }

        foreach (var group in groups)
        {
            group.Keywords = group.Keywords?
                .Select(NormalizeSearchQuery)
                .Where(static keyword => !string.IsNullOrWhiteSpace(keyword))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? [];
        }

        groups = groups
            .Where(static group => group.Keywords is { Count: > 0 })
            .OrderBy(static group => group.Sort)
            .ThenBy(static group => group.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        _searchBlockedKeywordGroupsByLanguage[normalizedLanguage] = groups;
        if (RequestLanguage.IsDefaultLanguage(normalizedLanguage))
        {
            _searchBlockedKeywordGroups = groups;
        }

        CodeWfLogger.Info(
            $"屏蔽搜索词加载完成。language={normalizedLanguage}; count={groups.Count}; file={filePath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        return groups;
    }


    public async Task<List<BlogPost>?> GetAllBlogPostsAsync()
    {
        var language = RequestLanguage.CurrentLanguage;
        if (RequestLanguage.IsDefaultLanguage(language) && _blogPosts?.Any() == true)
        {
            return _blogPosts;
        }

        if (_blogPostsByLanguage.TryGetValue(language, out var localizedPosts) && localizedPosts.Any())
        {
            return localizedPosts;
        }

        var localAssetsDir = GetLocalAssetsDir();
        if (localAssetsDir is null)
        {
            var emptyPosts = new List<BlogPost>();
            if (RequestLanguage.IsDefaultLanguage(language))
            {
                _blogPosts = emptyPosts;
            }

            _blogPostsByLanguage[language] = emptyPosts;
            return emptyPosts;
        }

        var posts = new List<BlogPost>();
        foreach (var postDir in GetPostYearDirectories(localAssetsDir))
        {
            var postFiles = Directory.GetFiles(postDir, "*.md", SearchOption.AllDirectories);
            foreach (var postFile in postFiles)
            {
                if (IsLocalizedBlogPostFile(postFile))
                {
                    continue;
                }

                try
                {
                    var blogPost = await ReadLocalizedBlogPostAsync(
                        postFile,
                        language,
                        createMissingTranslation: false,
                        renderContent: false);
                    if (!blogPost.Draft)
                    {
                        posts.Add(blogPost);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to read blog post {postFile}: {ex.Message}");
                }
            }
        }

        posts = posts
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue)
            .ToList();

        if (RequestLanguage.IsDefaultLanguage(language))
        {
            _blogPosts = posts;
        }

        _blogPostsByLanguage[language] = posts;
        return posts;
    }

    private static IEnumerable<string> GetPostYearDirectories(string localAssetsDir)
    {
        if (!Directory.Exists(localAssetsDir))
        {
            yield break;
        }

        var currentYear = DateTime.Now.Year;
        foreach (var directory in Directory.EnumerateDirectories(localAssetsDir)
                     .Select(path => new
                     {
                         Path = path,
                         IsYear = int.TryParse(Path.GetFileName(path), out var year),
                         Year = int.TryParse(Path.GetFileName(path), out var parsedYear) ? parsedYear : 0
                     })
                     .Where(item => item.IsYear && item.Year >= 1900 && item.Year <= currentYear + 1)
                     .OrderBy(item => item.Year))
        {
            yield return directory.Path;
        }
    }

    private async Task<BlogPost> ReadLocalizedBlogPostAsync(
        string sourcePath,
        string language,
        bool createMissingTranslation,
        bool renderContent)
    {
        var sourcePost = await ReadBlogPostAsync(sourcePath, renderContent);
        if (RequestLanguage.IsDefaultLanguage(language))
        {
            return sourcePost;
        }

        var localizedPath = await GetOrCreateLocalizedBlogPostPathAsync(
            sourcePath,
            sourcePost,
            language,
            createMissingTranslation);
        if (string.Equals(localizedPath, sourcePath, StringComparison.OrdinalIgnoreCase))
        {
            return sourcePost;
        }

        var localizedPost = await ReadBlogPostAsync(localizedPath, renderContent);
        localizedPost.SourcePath = sourcePath;
        localizedPost.Slug = sourcePost.Slug;
        localizedPost.Date = sourcePost.Date;
        localizedPost.Lastmod = sourcePost.Lastmod;
        localizedPost.Cover = sourcePost.Cover;
        localizedPost.Banner = sourcePost.Banner;
        localizedPost.Draft = sourcePost.Draft;
        localizedPost.Author = sourcePost.Author;
        localizedPost.LastModifyUser = sourcePost.LastModifyUser;
        localizedPost.OriginalTitle = sourcePost.OriginalTitle;
        localizedPost.OriginalLink = sourcePost.OriginalLink;
        localizedPost.Copyright = sourcePost.Copyright;

        return localizedPost;
    }

    private async Task<string> GetOrCreateLocalizedBlogPostPathAsync(
        string sourcePath,
        BlogPost sourcePost,
        string language,
        bool createMissingTranslation)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        var version = GetBlogPostVersion(sourcePost, sourcePath);
        var sourceName = Path.GetFileNameWithoutExtension(sourcePath);
        var targetPath = GetLocalizedBlogPostPath(
            sourcePath,
            normalizedLanguage,
            version,
            createCultureDirectory: false);
        if (targetPath is null)
        {
            return sourcePath;
        }

        var gate = _localizedAssetLocks.GetOrAdd(targetPath, _ => new SemaphoreSlim(1, 1));
        var stopwatch = createMissingTranslation ? Stopwatch.StartNew() : null;
        if (createMissingTranslation)
        {
            CodeWfLogger.Info(
                $"语言文章准备开始。language={normalizedLanguage}; slug={sourcePost.Slug}; source={sourcePath}; target={targetPath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            logger.LogInformation(
                "Localized article translation started. Language={Language}; Slug={Slug}; Source={SourcePath}; Target={TargetPath}.",
                normalizedLanguage,
                sourcePost.Slug,
                sourcePath,
                targetPath);
        }

        await gate.WaitAsync();
        try
        {
            DeleteStaleLocalizedBlogPosts(sourcePath, targetPath, version, normalizedLanguage);
            if (File.Exists(targetPath))
            {
                if (createMissingTranslation)
                {
                    await GetOrCreateLocalizedBlogPostMetadataPathAsync(sourcePath, sourcePost, normalizedLanguage);
                }

                stopwatch?.Stop();
                if (stopwatch is not null)
                {
                    CodeWfLogger.Info(
                        $"语言文章已存在。language={normalizedLanguage}; slug={sourcePost.Slug}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetPath}.",
                        log2UI: false,
                        log2File: false,
                        log2Console: true);
                    logger.LogInformation(
                        "Localized article already exists. Language={Language}; Slug={Slug}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                        normalizedLanguage,
                        sourcePost.Slug,
                        targetPath,
                        stopwatch.ElapsedMilliseconds);
                }

                return targetPath;
            }

            if (!createMissingTranslation)
            {
                return sourcePath;
            }

            var source = await BlogPostFiles.ReadBodyAsync(sourcePath);
            var translated = await translationService.TranslateAsync(
                source,
                RequestLanguage.GetLanguage(normalizedLanguage),
                ContentTranslationKind.MarkdownPage,
                resourceName: targetPath);

            if (string.IsNullOrWhiteSpace(translated))
            {
                stopwatch?.Stop();
                var message = $"语言文章未生成。language={normalizedLanguage}; slug={sourcePost.Slug}; elapsedMs={stopwatch?.ElapsedMilliseconds ?? 0}; source={sourcePath}; target={targetPath}.";
                CodeWfLogger.Warn(
                    message,
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                logger.LogWarning(
                    "Localized article translation returned empty result. Language={Language}; Slug={Slug}; Source={SourcePath}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    normalizedLanguage,
                    sourcePost.Slug,
                    sourcePath,
                    targetPath,
                    stopwatch?.ElapsedMilliseconds ?? 0);
                throw new InvalidOperationException(message);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            await WriteAllTextAtomicallyAsync(targetPath, translated.Trim());
            await GetOrCreateLocalizedBlogPostMetadataPathAsync(sourcePath, sourcePost, normalizedLanguage);
            _blogPostsByLanguage.TryRemove(normalizedLanguage, out _);
            _searchIndexByLanguage.TryRemove(normalizedLanguage, out _);
            stopwatch?.Stop();
            CodeWfLogger.Info(
                $"语言文章生成完成。language={normalizedLanguage}; slug={sourcePost.Slug}; outputChars={translated.Length}; elapsedMs={stopwatch?.ElapsedMilliseconds ?? 0}; target={targetPath}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            logger.LogInformation(
                "Localized article saved. Language={Language}; Slug={Slug}; Source={SourcePath}; Target={TargetPath}; OutputChars={OutputChars}; ElapsedMs={ElapsedMs}.",
                normalizedLanguage,
                sourcePost.Slug,
                sourcePath,
                targetPath,
                translated.Length,
                stopwatch?.ElapsedMilliseconds ?? 0);
            return targetPath;
        }
        catch (Exception ex)
        {
            stopwatch?.Stop();
            if (stopwatch is not null)
            {
                CodeWfLogger.Error(
                    $"语言文章生成失败。language={normalizedLanguage}; slug={sourcePost.Slug}; elapsedMs={stopwatch.ElapsedMilliseconds}; target={targetPath}.",
                    ex,
                    log2UI: false,
                    log2File: false,
                    log2Console: true);
                logger.LogError(
                    ex,
                    "Localized article translation failed. Language={Language}; Slug={Slug}; Source={SourcePath}; Target={TargetPath}; ElapsedMs={ElapsedMs}.",
                    normalizedLanguage,
                    sourcePost.Slug,
                    sourcePath,
                    targetPath,
                    stopwatch.ElapsedMilliseconds);
            }

            throw;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<BlogPost?> LocalizeBlogPostMetadataAsync(BlogPost? post)
    {
        if (post is null)
        {
            return null;
        }

        var language = RequestLanguage.Normalize(RequestLanguage.CurrentLanguage) ?? RequestLanguage.DefaultLanguage;
        if (RequestLanguage.IsDefaultLanguage(language))
        {
            return post;
        }

        var sourcePath = post.SourcePath;
        if (string.IsNullOrWhiteSpace(sourcePath) && !string.IsNullOrWhiteSpace(post.Slug))
        {
            sourcePath = await FindBlogPostSourcePathBySlugAsync(post.Slug);
        }

        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            return post;
        }

        // 详情页的上下篇和相关推荐只需要标题/摘要等 Front Matter 信息。
        // 没有完整本地化文章时，只生成小体积 metadata sidecar，避免为了一个卡片翻译整篇正文。
        var sourcePost = await ReadBlogPostAsync(sourcePath, renderContent: false);
        var localizedArticlePath = await GetOrCreateLocalizedBlogPostPathAsync(
            sourcePath,
            sourcePost,
            language,
            createMissingTranslation: false);
        if (!string.Equals(localizedArticlePath, sourcePath, StringComparison.OrdinalIgnoreCase))
        {
            return await ReadLocalizedBlogPostAsync(
                sourcePath,
                language,
                createMissingTranslation: false,
                renderContent: false);
        }

        var metadataPath = await GetOrCreateLocalizedBlogPostMetadataPathAsync(sourcePath, sourcePost, language);
        if (metadataPath is null || !File.Exists(metadataPath))
        {
            return post;
        }

        try
        {
            var metadataPost = await BlogPostFiles.ReadMetadataAsync(metadataPath);
            var metadata = new BlogPostMetadataTranslation(
                metadataPost.Title,
                metadataPost.Description,
                metadataPost.Albums,
                metadataPost.Categories,
                metadataPost.Tags);
            return ApplyLocalizedMetadata(post, metadata);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Localized article metadata could not be loaded. Language={Language}; Slug={Slug}; MetadataPath={MetadataPath}.",
                language,
                post.Slug,
                metadataPath);
            return post;
        }
    }

    private async Task<string?> GetOrCreateLocalizedBlogPostMetadataPathAsync(
        string sourcePath,
        BlogPost sourcePost,
        string language)
    {
        var normalizedLanguage = RequestLanguage.Normalize(language) ?? RequestLanguage.DefaultLanguage;
        var version = GetBlogPostVersion(sourcePost, sourcePath);
        var targetPath = GetLocalizedBlogPostMetadataPath(
            sourcePath,
            normalizedLanguage,
            version,
            createCultureDirectory: false);
        if (targetPath is null)
        {
            return null;
        }

        var gate = _localizedAssetLocks.GetOrAdd(targetPath, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            DeleteStaleLocalizedBlogPostMetadata(sourcePath, targetPath, version);
            if (File.Exists(targetPath))
            {
                return targetPath;
            }

            var resource = JsonSerializer.Serialize(
                new BlogPostMetadataTranslation(
                    sourcePost.Title,
                    sourcePost.Description,
                    sourcePost.Albums,
                    sourcePost.Categories,
                    sourcePost.Tags),
                WriteJsonOptions);
            var translated = await translationService.TranslateAsync(
                resource,
                RequestLanguage.GetLanguage(normalizedLanguage),
                ContentTranslationKind.JsonResource,
                resourceName: targetPath);
            if (string.IsNullOrWhiteSpace(translated))
            {
                return null;
            }

            var metadata = JsonSerializer.Deserialize<BlogPostMetadataTranslation>(translated, JsonOptions);
            if (metadata is null)
            {
                return null;
            }

            var metadataYaml = BlogPostFiles.SerializeMetadata(ApplyLocalizedMetadata(sourcePost, metadata));
            await WriteAllTextAtomicallyAsync(targetPath, metadataYaml);
            _blogPostsByLanguage.TryRemove(normalizedLanguage, out _);
            _searchIndexByLanguage.TryRemove(normalizedLanguage, out _);
            return targetPath;
        }
        finally
        {
            gate.Release();
        }
    }

    private string? GetLocalizedBlogPostMetadataPath(
        string sourcePath,
        string language,
        string version,
        bool createCultureDirectory)
    {
        var localAssetsDir = GetLocalAssetsDir();
        if (localAssetsDir is null)
        {
            return null;
        }

        var relativePath = Path.GetRelativePath(localAssetsDir, sourcePath);
        if (relativePath.StartsWith("..", StringComparison.Ordinal)
            || Path.IsPathRooted(relativePath))
        {
            return null;
        }

        var relativeDirectory = Path.GetDirectoryName(relativePath);
        var sourceName = Path.GetFileNameWithoutExtension(sourcePath);
        var fileName = $"{sourceName}.{version}{BlogPostFiles.MetadataExtension}";
        return string.IsNullOrWhiteSpace(relativeDirectory)
            ? GetI18nAssetPath(language, createCultureDirectory, fileName)
            : GetI18nAssetPath(
                language,
                createCultureDirectory,
                relativeDirectory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Where(static segment => !string.IsNullOrWhiteSpace(segment))
                    .Concat([fileName])
                    .ToArray());
    }

    private static void DeleteStaleLocalizedBlogPostMetadata(
        string sourcePath,
        string targetPath,
        string expectedVersion)
    {
        var directory = Path.GetDirectoryName(targetPath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return;
        }

        var sourceName = Path.GetFileNameWithoutExtension(sourcePath);
        var expectedFileName = Path.GetFileName(targetPath);
        var candidates = Directory.GetFiles(directory, $"{sourceName}.*{BlogPostFiles.MetadataExtension}")
            .Concat(Directory.GetFiles(directory, $"{sourceName}.*.meta.json"));
        foreach (var candidate in candidates)
        {
            var fileName = Path.GetFileName(candidate);
            var isCurrentSidecar = fileName.EndsWith(BlogPostFiles.MetadataExtension, StringComparison.OrdinalIgnoreCase)
                && fileName.Contains($".{expectedVersion}", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(fileName, expectedFileName, StringComparison.OrdinalIgnoreCase)
                || !fileName.StartsWith($"{sourceName}.", StringComparison.OrdinalIgnoreCase)
                || isCurrentSidecar)
            {
                continue;
            }

            File.Delete(candidate);
            var sidecarPath = BlogPostFiles.GetMetadataPath(candidate);
            if (File.Exists(sidecarPath))
            {
                File.Delete(sidecarPath);
            }
        }
    }

    private static BlogPost ApplyLocalizedMetadata(BlogPost post, BlogPostMetadataTranslation metadata)
    {
        return new BlogPost
        {
            SourcePath = post.SourcePath,
            Title = string.IsNullOrWhiteSpace(metadata.Title) ? post.Title : metadata.Title,
            Slug = post.Slug,
            Description = string.IsNullOrWhiteSpace(metadata.Description) ? post.Description : metadata.Description,
            Date = post.Date,
            Lastmod = post.Lastmod,
            Copyright = post.Copyright,
            Banner = post.Banner,
            Author = post.Author,
            LastModifyUser = post.LastModifyUser,
            OriginalTitle = post.OriginalTitle,
            OriginalLink = post.OriginalLink,
            Draft = post.Draft,
            Cover = post.Cover,
            Albums = metadata.Albums?.Count > 0 ? metadata.Albums : post.Albums?.ToList(),
            Categories = metadata.Categories?.Count > 0 ? metadata.Categories : post.Categories?.ToList(),
            Tags = metadata.Tags?.Count > 0 ? metadata.Tags : post.Tags?.ToList(),
            Content = post.Content,
            HtmlContent = post.HtmlContent
        };
    }

    private string? GetLocalizedBlogPostPath(
        string sourcePath,
        string language,
        string version,
        bool createCultureDirectory)
    {
        var localAssetsDir = GetLocalAssetsDir();
        if (localAssetsDir is null)
        {
            return null;
        }

        var relativePath = Path.GetRelativePath(localAssetsDir, sourcePath);
        if (relativePath.StartsWith("..", StringComparison.Ordinal)
            || Path.IsPathRooted(relativePath))
        {
            return null;
        }

        var relativeDirectory = Path.GetDirectoryName(relativePath);
        var sourceName = Path.GetFileNameWithoutExtension(sourcePath);
        var fileName = $"{sourceName}.{version}.md";
        return string.IsNullOrWhiteSpace(relativeDirectory)
            ? GetI18nAssetPath(language, createCultureDirectory, fileName)
            : GetI18nAssetPath(
                language,
                createCultureDirectory,
                relativeDirectory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Where(static segment => !string.IsNullOrWhiteSpace(segment))
                    .Concat([fileName])
                    .ToArray());
    }

    private static void DeleteStaleLocalizedBlogPosts(
        string sourcePath,
        string targetPath,
        string expectedVersion,
        string language)
    {
        var directory = Path.GetDirectoryName(targetPath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return;
        }

        var sourceName = Path.GetFileNameWithoutExtension(sourcePath);
        foreach (var candidate in Directory.GetFiles(directory, $"{sourceName}.*.md"))
        {
            var fileName = Path.GetFileName(candidate);
            var match = LocalizedBlogPostFileNameRegex.Match(fileName);
            var legacyMatch = LegacyLocalizedBlogPostFileNameRegex.Match(fileName);
            var isExpectedNewFile = match.Success
                && string.Equals(match.Groups["slug"].Value, sourceName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(match.Groups["timestamp"].Value, expectedVersion, StringComparison.OrdinalIgnoreCase);
            var isStaleNewFile = match.Success
                && string.Equals(match.Groups["slug"].Value, sourceName, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(match.Groups["timestamp"].Value, expectedVersion, StringComparison.OrdinalIgnoreCase);
            var isStaleLegacyFile = legacyMatch.Success
                && string.Equals(legacyMatch.Groups["slug"].Value, sourceName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(legacyMatch.Groups["language"].Value, language, StringComparison.OrdinalIgnoreCase);

            if (isExpectedNewFile || (!isStaleNewFile && !isStaleLegacyFile))
            {
                continue;
            }

            File.Delete(candidate);
        }
    }

    private static string GetBlogPostVersion(BlogPost post, string sourcePath)
    {
        var versionTime = post.Lastmod ?? post.Date ?? File.GetLastWriteTime(sourcePath);
        return versionTime.ToString("yyyyMMddHHmmss");
    }

    private static async Task WriteAllTextAtomicallyAsync(string path, string content)
    {
        // 翻译文件写一半会污染后续缓存读取，所以先写同目录临时文件，再原子替换目标文件。
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            await File.WriteAllTextAsync(tempPath, content, Encoding.UTF8);
            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static bool IsLocalizedBlogPostFile(string path) =>
        LocalizedBlogPostFileNameRegex.IsMatch(Path.GetFileName(path))
        || LegacyLocalizedBlogPostFileNameRegex.IsMatch(Path.GetFileName(path))
        || Path.GetFileName(path).EndsWith(".meta.json", StringComparison.OrdinalIgnoreCase);

    public async Task<List<BlogPostBrief>?> GetAllBlogPostBriefsAsync()
    {
        var posts = await GetAllBlogPostsAsync();
        return posts?.Select(ToBlogPostBrief).ToList();
    }

    public async Task<PageData<BlogPostBrief>> GetPostByAlbum(int pageIndex, int pageSize, string albumSlug,
        string? key)
    {
        var allPosts = await GetAllBlogPostsAsync() ?? [];
        var albumNames = await GetAlbumMatchNamesAsync(albumSlug);

        IEnumerable<BlogPost> posts;
        if (!string.IsNullOrWhiteSpace(key))
        {
            posts = allPosts
                .Where(p => p.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Slug?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Author?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.LastModifyUser?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Content?.Contains(key, StringComparison.OrdinalIgnoreCase) == true);
        }
        else
        {
            posts = allPosts
                .Where(post => string.Equals(ConstantUtil.DefaultCategory, albumSlug, StringComparison.OrdinalIgnoreCase)
                               || HasAnyTaxonomyName(post.Albums, albumNames));
        }

        var ordered = posts
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue);

        var total = ordered.Count();
        var postDatas = ordered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(ToBlogPostBrief)
            .ToList();
        return new PageData<BlogPostBrief>(pageIndex, pageSize, total, postDatas);
    }

    public async Task<PageData<BlogPostBrief>> GetPostByCategory(int pageIndex, int pageSize, string categorySlug,
        string? key)
    {
        var allPosts = await GetAllBlogPostsAsync() ?? [];
        var categoryNames = await GetCategoryMatchNamesAsync(categorySlug);

        IEnumerable<BlogPost> posts;
        if (!string.IsNullOrWhiteSpace(key))
        {
            posts = allPosts
                .Where(p => p.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Slug?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Author?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.LastModifyUser?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Content?.Contains(key, StringComparison.OrdinalIgnoreCase) == true);
        }
        else
        {
            posts = allPosts
                .Where(post => string.Equals(ConstantUtil.DefaultCategory, categorySlug, StringComparison.OrdinalIgnoreCase)
                               || HasAnyTaxonomyName(post.Categories, categoryNames));
        }

        var ordered = posts
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue);

        var total = ordered.Count();
        var postDatas = ordered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(ToBlogPostBrief)
            .ToList();
        return new PageData<BlogPostBrief>(pageIndex, pageSize, total, postDatas);
    }

    private async Task<HashSet<string>> GetAlbumMatchNamesAsync(string albumSlug)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddTaxonomyNames(await GetAllAlbumItemsAsync(), albumSlug, names);
        AddTaxonomyNames(await LoadDefaultAlbumItemsForMatchingAsync(), albumSlug, names);
        AddDecodedSlugName(albumSlug, names);
        return names;
    }

    private async Task<HashSet<string>> GetCategoryMatchNamesAsync(string categorySlug)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddTaxonomyNames(await GetAllCategoryItemsAsync(), categorySlug, names);
        AddTaxonomyNames(await LoadDefaultCategoryItemsForMatchingAsync(), categorySlug, names);
        AddDecodedSlugName(categorySlug, names);
        return names;
    }

    private static void AddTaxonomyNames<T>(IEnumerable<T>? items, string slug, HashSet<string> names)
    {
        if (items is null)
        {
            return;
        }

        foreach (var item in items)
        {
            var itemSlug = item switch
            {
                AlbumItem album => album.Slug,
                CategoryItem category => category.Slug,
                _ => null
            };

            if (!string.Equals(itemSlug, slug, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var itemName = item switch
            {
                AlbumItem album => album.Name,
                CategoryItem category => category.Name,
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                names.Add(itemName.Trim());
            }
        }
    }

    private static void AddDecodedSlugName(string slug, HashSet<string> names)
    {
        if (string.Equals(slug, ConstantUtil.DefaultCategory, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var decoded = ConstantUtil.DecodeTagSlug(slug);
        if (!string.IsNullOrWhiteSpace(decoded))
        {
            names.Add(decoded);
        }
    }

    private static bool HasAnyTaxonomyName(IEnumerable<string>? values, HashSet<string> names) =>
        values?.Any(value => !string.IsNullOrWhiteSpace(value) && names.Contains(value.Trim())) == true;

    private async Task<List<AlbumItem>> LoadDefaultAlbumItemsForMatchingAsync()
    {
        if (_albumItems?.Any() == true)
        {
            return _albumItems;
        }

        return await LoadDefaultSiteJsonListAsync<AlbumItem>("albums.json");
    }

    private async Task<List<CategoryItem>> LoadDefaultCategoryItemsForMatchingAsync()
    {
        if (_categoryItems?.Any() == true)
        {
            return _categoryItems;
        }

        return await LoadDefaultSiteJsonListAsync<CategoryItem>("categories.json");
    }

    private async Task<List<T>> LoadDefaultSiteJsonListAsync<T>(string fileName)
    {
        var filePath = GetAssetPath("site", fileName);
        if (filePath is null || !File.Exists(filePath))
        {
            return [];
        }

        try
        {
            var fileContent = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<T>>(fileContent, JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize {fileName}: {ex.Message}");
            return [];
        }
    }

    public async Task<List<TagItem>> GetAllTagItemsAsync()
    {
        var allPosts = await GetAllBlogPostsAsync() ?? [];

        return allPosts
            .SelectMany(static post => post.Tags ?? [])
            .Where(static tag => !string.IsNullOrWhiteSpace(tag))
            .Select(ConstantUtil.NormalizeTagName)
            .Where(static tag => !string.IsNullOrWhiteSpace(tag))
            .GroupBy(static tag => tag, StringComparer.OrdinalIgnoreCase)
            .Select(static group => new TagItem
            {
                Name = group.First(),
                PostCount = group.Count()
            })
            .OrderByDescending(static tag => tag.PostCount)
            .ThenBy(static tag => tag.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public async Task<PageData<BlogPostBrief>> GetPostByTag(int pageIndex, int pageSize, string tag, string? key = null)
    {
        var allPosts = await GetAllBlogPostsAsync() ?? [];

        var normalizedTag = ConstantUtil.NormalizeTagName(tag);
        IEnumerable<BlogPost> posts;
        if (!string.IsNullOrWhiteSpace(key))
        {
            posts = allPosts
                .Where(p => p.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Slug?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Author?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Content?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Tags?.Any(tagItem => tagItem.Contains(key, StringComparison.OrdinalIgnoreCase)) == true);
        }
        else
        {
            posts = allPosts
                .Where(post => post.Tags?.Any(postTag =>
                    string.Equals(
                        ConstantUtil.NormalizeTagName(postTag),
                        normalizedTag,
                        StringComparison.OrdinalIgnoreCase)) == true);
        }

        var ordered = posts
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue);

        var total = ordered.Count();
        var data = ordered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(ToBlogPostBrief)
            .ToList();

        return new PageData<BlogPostBrief>(pageIndex, pageSize, total, data);
    }

    public async Task<List<BlogPostBrief>?> GetBannerPostAsync()
    {
        var posts = await GetAllBlogPostsAsync();
        var bannerPosts = posts
            ?.Where(post => post.Banner)
            .OrderByDescending(post => post.Date)
            .Select(ToBlogPostBrief)
            .ToList();
        return bannerPosts;
    }

    public async Task<PageData<BlogPostBrief>> GetPagedBlogPostsAsync(int pageIndex, int pageSize, string? key = null)
    {
        var source = (await GetAllBlogPostsAsync())?.AsEnumerable() ?? [];

        if (!string.IsNullOrWhiteSpace(key))
        {
            source = source.Where(post =>
                post.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                || post.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                || post.Slug?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                || post.Author?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                || post.Content?.Contains(key, StringComparison.OrdinalIgnoreCase) == true);
        }

        var ordered = source
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue);

        var total = ordered.Count();
        var data = ordered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(ToBlogPostBrief)
            .ToList();

        return new PageData<BlogPostBrief>(pageIndex, pageSize, total, data);
    }

    private static BlogPostBrief ToBlogPostBrief(BlogPost post) => new()
    {
        SourcePath = post.SourcePath,
        Title = post.Title,
        Slug = post.Slug,
        Description = post.Description,
        Date = post.Date,
        Lastmod = post.Lastmod,
        Copyright = post.Copyright,
        Banner = post.Banner,
        Author = post.Author,
        LastModifyUser = post.LastModifyUser,
        OriginalTitle = post.OriginalTitle,
        OriginalLink = post.OriginalLink,
        Draft = post.Draft,
        Cover = post.Cover,
        Albums = post.Albums?.ToList(),
        Categories = post.Categories?.ToList(),
        Tags = post.Tags?.ToList()
    };

    public Task<Dictionary<string, string>> GetWebSiteCountAsync()
    {
        if (_webSiteCountInfos != null)
        {
            return Task.FromResult(_webSiteCountInfos);
        }

        _webSiteCountInfos = new();
        var total = _blogPosts?.Count ?? 0;
        var original = _blogPosts?.Count(post => string.IsNullOrWhiteSpace(post.Author)) ?? 0;
        var originalPercentage = total > 0 ? (double)original / total * 100 : 0;
        _webSiteCountInfos["网站创建"] = $"{DateTime.Now.Year - siteOption.Value.StartYear}年";
        _webSiteCountInfos["文章分类"] = $"{_categoryItems?.Count}个";
        _webSiteCountInfos["文章总计"] = $"{total}篇";
        _webSiteCountInfos["文章原创"] = $"{original}篇({originalPercentage:F2}%)";
        return Task.FromResult(_webSiteCountInfos);
    }

    public async Task<(string? Markdown, string? HtmlContent)> ReadAboutAsync()
    {
        var language = RequestLanguage.CurrentLanguage;
        if (_aboutByLanguage.TryGetValue(language, out var localizedAbout)
            && !string.IsNullOrWhiteSpace(localizedAbout.Markdown))
        {
            return localizedAbout;
        }

        if (string.Equals(language, RequestLanguage.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(_aboutMarkdown))
        {
            return (_aboutMarkdown, _aboutHtmlContent);
        }

        var filePath = await GetLocalizedSiteAssetPathAsync(language, "about.md");
        if (filePath is null || !File.Exists(filePath))
        {
            return ("## 关于", "关于");
        }

        var markdown = await File.ReadAllTextAsync(filePath);
        var htmlContent = markdown.ToHtml();
        _aboutByLanguage[language] = (markdown, htmlContent);
        if (string.Equals(language, RequestLanguage.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
        {
            _aboutMarkdown = markdown;
            _aboutHtmlContent = htmlContent;
        }

        return (markdown, htmlContent);
    }

    public async Task<(string? Markdown, string? HtmlContent)> ReadDonationAsync()
    {
        var language = RequestLanguage.CurrentLanguage;
        if (_donationByLanguage.TryGetValue(language, out var localizedDonation)
            && !string.IsNullOrWhiteSpace(localizedDonation.Markdown))
        {
            return localizedDonation;
        }

        if (string.Equals(language, RequestLanguage.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(_donationMarkdown))
        {
            return (_donationMarkdown, _donationHtmlContent);
        }

        var filePath = await GetLocalizedPayAssetPathAsync(language, "Donation.md");
        if (filePath is null || !File.Exists(filePath))
        {
            return ("## 赞助", "赞助");
        }

        var markdown = await File.ReadAllTextAsync(filePath);
        var htmlContent = markdown.ToHtml();
        _donationByLanguage[language] = (markdown, htmlContent);
        if (string.Equals(language, RequestLanguage.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
        {
            _donationMarkdown = markdown;
            _donationHtmlContent = htmlContent;
        }

        return (markdown, htmlContent);
    }

    public async Task<BlogPost?> GetPostBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var language = RequestLanguage.CurrentLanguage;
        var sourcePath = await FindBlogPostSourcePathBySlugAsync(slug);
        if (sourcePath is null)
        {
            return null;
        }

        var post = await ReadLocalizedBlogPostAsync(
            sourcePath,
            language,
            createMissingTranslation: true,
            renderContent: true);
        _blogPostsByLanguage.TryRemove(language, out _);
        return post;
    }

    private async Task<string?> FindBlogPostSourcePathBySlugAsync(string slug)
    {
        var localAssetsDir = GetLocalAssetsDir();
        if (localAssetsDir is null)
        {
            return null;
        }

        foreach (var postDir in GetPostYearDirectories(localAssetsDir))
        {
            foreach (var postFile in Directory.GetFiles(postDir, "*.md", SearchOption.AllDirectories))
            {
                if (IsLocalizedBlogPostFile(postFile))
                {
                    continue;
                }

                try
                {
                    var post = await ReadBlogPostAsync(postFile, renderContent: false);
                    if (string.Equals(post.Slug, slug, StringComparison.OrdinalIgnoreCase))
                    {
                        return postFile;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to inspect blog post {postFile}: {ex.Message}");
                }
            }
        }

        return null;
    }

    public static Task<BlogPost> ReadBlogPostAsync(string markdownFilePath) =>
        ReadBlogPostAsync(markdownFilePath, renderContent: true);

    private static Task<BlogPost> ReadBlogPostAsync(string markdownFilePath, bool renderContent) =>
        BlogPostFiles.ReadAsync(markdownFilePath, renderContent);

    public async Task<List<FriendLinkItem>?> GetAllFriendLinkItemsAsync()
    {
        var language = RequestLanguage.Normalize(RequestLanguage.CurrentLanguage) ?? RequestLanguage.DefaultLanguage;
        if (RequestLanguage.IsDefaultLanguage(language) && _friendLinkItems?.Any() == true)
        {
            return _friendLinkItems;
        }

        if (_friendLinkItemsByLanguage.TryGetValue(language, out var localizedItems) && localizedItems.Any())
        {
            return localizedItems;
        }

        var filePath = await GetLocalizedSiteAssetPathAsync(language, "friend-links.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return RequestLanguage.IsDefaultLanguage(language)
                ? _friendLinkItems
                : localizedItems;
        }

        CodeWfLogger.Info(
            $"友情链接资源加载开始。language={language}; file={filePath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        var fileContent = await File.ReadAllTextAsync(filePath);
        List<FriendLinkItem>? items = null;
        try
        {
            items = JsonSerializer.Deserialize<List<FriendLinkItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            CodeWfLogger.Error(
                $"友情链接资源反序列化失败。language={language}; file={filePath}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            Console.WriteLine($"Failed to deserialize friend-links.json: {ex.Message}");
        }

        items ??= [];
        if (RequestLanguage.IsDefaultLanguage(language))
        {
            _friendLinkItems = items;
        }

        _friendLinkItemsByLanguage[language] = items;
        CodeWfLogger.Info(
            $"友情链接资源加载完成。language={language}; count={items.Count}; file={filePath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        return items;
    }

    public async Task<List<TimeLineItem>?> GetTimeLineItemsAsync()
    {
        var language = RequestLanguage.Normalize(RequestLanguage.CurrentLanguage) ?? RequestLanguage.DefaultLanguage;
        if (RequestLanguage.IsDefaultLanguage(language) && _timeLineItems?.Any() == true)
        {
            return _timeLineItems;
        }

        if (_timeLineItemsByLanguage.TryGetValue(language, out var localizedItems) && localizedItems.Any())
        {
            return localizedItems;
        }

        var filePath = await GetLocalizedSiteAssetPathAsync(language, "timelines.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return RequestLanguage.IsDefaultLanguage(language)
                ? _timeLineItems
                : localizedItems;
        }

        CodeWfLogger.Info(
            $"时间线资源加载开始。language={language}; file={filePath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        var fileContent = await File.ReadAllTextAsync(filePath);
        List<TimeLineItem>? items = null;
        try
        {
            items = JsonSerializer.Deserialize<List<TimeLineItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            CodeWfLogger.Error(
                $"时间线资源反序列化失败。language={language}; file={filePath}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            Console.WriteLine($"Failed to deserialize timelines.json: {ex.Message}");
        }

        items ??= [];
        if (RequestLanguage.IsDefaultLanguage(language))
        {
            _timeLineItems = items;
        }

        _timeLineItemsByLanguage[language] = items;
        CodeWfLogger.Info(
            $"时间线资源加载完成。language={language}; count={items.Count}; file={filePath}.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        return items;
    }

    public async Task<string> GetRssAsync()
    {
        if (!string.IsNullOrWhiteSpace(_rss))
        {
            return _rss;
        }

        var data = _blogPosts?
            .OrderByDescending(p => p.Lastmod)
            .ThenByDescending(p => p.Date)
            .Take(10)
            .ToList();
        // RSS 只保留最近几篇，兼顾订阅时效性和输出体积。

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine(
            "<rss xmlns:atom=\"http://www.w3.org/2005/Atom\" xmlns:content=\"http://purl.org/rss/1.0/modules/content/\" version=\"2.0\">");
        sb.Append("<channel>");
        var rssPath = RequestLanguage.LocalizePath("/rss", RequestLanguage.DefaultLanguage);
        sb.Append(
            $"<atom:link rel=\"self\" type=\"application/rss+xml\" href=\"{XmlEncode($"{siteOption.Value.Domain}{rssPath}")}\"/>");
        sb.Append($"<title>{XmlEncode($"{siteOption.Value.AppTitle}_{siteOption.Value.Memo}")}</title>");
        sb.Append($"<link>{XmlEncode($"{siteOption.Value.Domain}{rssPath}")}</link>");
        sb.Append($"<description>{XmlEncode(siteOption.Value.Memo)}</description>");
        sb.Append($"<copyright>{XmlEncode($"{siteOption.Value.AppTitle}_{siteOption.Value.Memo}")}</copyright>");
        sb.Append("<language>zh-cn</language>");
        if (data is { Count: > 0 })
        {
            foreach (var item in data)
            {
                sb.Append("<item>");
                sb.Append($"<title>{XmlEncode(item.Title)}</title>");
                sb.Append(
                    $"<link>{XmlEncode($"{siteOption.Value.Domain}{ConstantUtil.GetPostUrl(item)}")}</link>");
                sb.Append($"<description>{XmlEncode(item.Description)}</description>");
                sb.Append($"<author>{XmlEncode(item.Author ?? siteOption.Value.Owner)}</author>");
                sb.Append($"<category>{XmlEncode(string.Join(",", item.Categories ?? []))}</category>");
                sb.Append(
                    $"<guid>{XmlEncode($"{siteOption.Value.Domain}{ConstantUtil.GetPostUrl(item)}")}</guid>");
                sb.Append($"<pubDate>{item.Date:R}</pubDate>");
                sb.Append($"<content:encoded><![CDATA[{ToCData(item.Description)}]]></content:encoded>");
                sb.Append("</item>");
            }
        }

        sb.Append("</channel>");
        sb.AppendLine("</rss>");

        _rss = sb.ToString();

        return _rss;
    }

    public async Task<string> GetSiteMapAsync()
    {
        if (!string.IsNullOrWhiteSpace(_siteMap))
        {
            return _siteMap;
        }

        List<SitemapNode> siteMapNodes = new();
        var domain = (siteOption.Value.Domain ?? string.Empty).TrimEnd('/');
        var now = DateTimeOffset.UtcNow;
        var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddNode(string path, double priority, SitemapFrequency frequency, DateTimeOffset? lastModified = null)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(domain))
            {
                return;
            }

            var normalizedPath = RequestLanguage.LocalizePath(path.StartsWith('/') ? path : $"/{path}", RequestLanguage.DefaultLanguage);
            var url = $"{domain}{normalizedPath}";
            if (!seenUrls.Add(url))
            {
                // 文章、分类、工具会从不同来源汇总进来，提前去重避免 sitemap 里重复输出。
                return;
            }

            siteMapNodes.Add(new SitemapNode
            {
                LastModified = lastModified ?? now,
                Priority = priority,
                Url = url,
                Frequency = frequency
            });
        }

        AddNode("/", 1.0, SitemapFrequency.Daily);
        AddNode(ConstantUtil.GetBlogUrl(), 0.9, SitemapFrequency.Daily);
        AddNode(ConstantUtil.GetProjectDirectoryUrl(), 0.8, SitemapFrequency.Weekly);
        AddNode("/tool", 0.8, SitemapFrequency.Weekly);
        AddNode(ConstantUtil.GetCategoryDirectoryUrl(), 0.7, SitemapFrequency.Weekly);
        AddNode(ConstantUtil.GetAlbumDirectoryUrl(), 0.7, SitemapFrequency.Weekly);
        AddNode(ConstantUtil.GetTagDirectoryUrl(), 0.7, SitemapFrequency.Weekly);
        AddNode("/about", 0.5, SitemapFrequency.Monthly);
        AddNode("/timeline", 0.5, SitemapFrequency.Monthly);
        AddNode("/donation", 0.4, SitemapFrequency.Monthly);

        if (_categoryItems?.Any() == true)
        {
            foreach (var item in _categoryItems.Where(static item =>
                         !string.IsNullOrWhiteSpace(item.Slug)
                         && !string.Equals(item.Slug, ConstantUtil.DefaultCategory, StringComparison.OrdinalIgnoreCase)))
            {
                AddNode(ConstantUtil.GetCategoryUrl(item.Slug!), 0.8, SitemapFrequency.Monthly);
            }
        }

        if (_albumItems?.Any() == true)
        {
            foreach (var item in _albumItems.Where(static item =>
                         !string.IsNullOrWhiteSpace(item.Slug)
                         && !string.Equals(item.Slug, ConstantUtil.DefaultCategory, StringComparison.OrdinalIgnoreCase)))
            {
                AddNode(ConstantUtil.GetAlbumUrl(item.Slug!), 0.8, SitemapFrequency.Monthly);
            }
        }

        var tags = await GetAllTagItemsAsync();
        foreach (var tag in tags.Where(static item => !string.IsNullOrWhiteSpace(item.Name)))
        {
            AddNode(ConstantUtil.GetTagUrl(tag.Name), 0.7, SitemapFrequency.Weekly);
        }

        foreach (var doc in FlattenDocItems(_docItems).Where(static item => !string.IsNullOrWhiteSpace(item.Slug)))
        {
            AddNode(ConstantUtil.GetDocUrl(doc.Slug), 0.7, SitemapFrequency.Monthly);
        }

        foreach (var tool in FlattenToolItems(_toolItems).Where(static item => !string.IsNullOrWhiteSpace(item.Slug)))
        {
            AddNode(ConstantUtil.GetToolUrl(tool.Slug), 0.7, SitemapFrequency.Monthly);
        }

        if (_blogPosts?.Any() == true)
        {
            foreach (var item in _blogPosts
                         .Where(static post => !string.IsNullOrWhiteSpace(post.Slug))
                         .OrderByDescending(post => post.Lastmod)
                         .ThenByDescending(post => post.Date))
            {
                AddNode(ConstantUtil.GetPostUrl(item), 0.9, SitemapFrequency.Daily, item.Lastmod ?? item.Date ?? now);
            }
        }

        StringBuilder sb = new();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"");
        sb.AppendLine("   xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
        sb.AppendLine(
            "   xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\">");

        foreach (SitemapNode m in siteMapNodes)
        {
            sb.AppendLine("    <url>");

            sb.AppendLine($"        <loc>{XmlEncode(m.Url)}</loc>");
            sb.AppendLine($"        <lastmod>{m.LastModified.ToString("yyyy-MM-dd")}</lastmod>");
            sb.AppendLine($"        <changefreq>{m.Frequency.ToString().ToLowerInvariant()}</changefreq>");
            sb.AppendLine($"        <priority>{m.Priority}</priority>");

            sb.AppendLine("    </url>");
        }

        sb.AppendLine("</urlset>");

        _siteMap = sb.ToString();

        return _siteMap;
    }

    private static IEnumerable<DocItem> FlattenDocItems(IEnumerable<DocItem>? items)
    {
        if (items == null)
        {
            yield break;
        }

        foreach (var item in items)
        {
            if (item.Children?.Any() == true)
            {
                foreach (var child in FlattenDocItems(item.Children))
                {
                    yield return child;
                }

                continue;
            }

            yield return item;
        }
    }

    private static IEnumerable<ToolItem> FlattenToolItems(IEnumerable<ToolItem>? items)
    {
        if (items == null)
        {
            yield break;
        }

        foreach (var item in items)
        {
            yield return item;

            foreach (var child in FlattenToolItems(item.Children))
            {
                yield return child;
            }
        }
    }

    private static string XmlEncode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string ToCData(string? value) => (value ?? string.Empty).Replace("]]>", "]]]]><![CDATA[>");

    public void Dispose()
    {
        if (_assetWatcher is not null)
        {
            _assetWatcher.EnableRaisingEvents = false;
            _assetWatcher.Changed -= OnAssetChanged;
            _assetWatcher.Created -= OnAssetChanged;
            _assetWatcher.Deleted -= OnAssetChanged;
            _assetWatcher.Renamed -= OnAssetChanged;
            _assetWatcher.Dispose();
        }

        _assetWatcherDebounceTimer?.Dispose();
        _searchIndexLock.Dispose();
        foreach (var lockItem in _searchQueryStatsFileLocks.Values)
        {
            lockItem.Dispose();
        }

        foreach (var lockItem in _localizedAssetLocks.Values)
        {
            lockItem.Dispose();
        }
    }
}
