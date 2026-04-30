using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using WebApp.Extensions;
using WebApp.Models;
using WebApp.Options;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WebApp.Services;

public class AppService(IOptions<SiteOption> siteOption, IWebHostEnvironment environment)
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

    private List<DocItem>? _docItems;
    private List<ToolItem>? _toolItems;
    private List<AlbumItem>? _albumItems;
    private List<CategoryItem>? _categoryItems;
    private List<SearchBlockedKeywordGroup>? _searchBlockedKeywordGroups;
    private IReadOnlyList<string>? _searchBlockedKeywords;
    private List<BlogPost>? _blogPosts;
    private List<FriendLinkItem>? _friendLinkItems;
    private List<TimeLineItem>? _timeLineItems;
    private Dictionary<string, string>? _webSiteCountInfos;
    private string? _donationMarkdown;
    private string? _donationHtmlContent;
    private string? _aboutMarkdown;
    private string? _aboutHtmlContent;
    private string? _rss;
    private string? _siteMap;
    private List<SearchableEntry>? _searchIndex;

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

    private readonly SemaphoreSlim _searchIndexLock = new(1, 1);
    private readonly SemaphoreSlim _searchQueryStatsFileLock = new(1, 1);
    private readonly ConcurrentDictionary<string, SearchCacheEntry> _searchCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SearchQueryStats> _searchQueryStats = new(StringComparer.OrdinalIgnoreCase);
    private bool _searchQueryStatsLoaded;

    private string? GetLocalAssetsDir()
    {
        var localAssetsDir = siteOption.Value.LocalAssetsDir;
        if (string.IsNullOrWhiteSpace(localAssetsDir))
        {
            return null;
        }

        var expandedPath = System.Environment.ExpandEnvironmentVariables(localAssetsDir.Trim());
        var path = Path.IsPathRooted(expandedPath)
            ? expandedPath
            : Path.Combine(environment.ContentRootPath, expandedPath);

        return Path.GetFullPath(path);
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

    public async Task SeedAsync()
    {
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

    public async Task<List<DocItem>?> GetAllDocItemsAsync()
    {
        if (_docItems?.Any() == true)
        {
            return _docItems;
        }

        var filePath = GetAssetPath("site", "doc", "navigation.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return _docItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _docItems = JsonSerializer.Deserialize<List<DocItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize doc/navigation.json: {ex.Message}");
        }
        return _docItems;
    }

    public async Task<List<ToolItem>?> GetAllToolItemsAsync()
    {
        if (_toolItems?.Any() == true)
        {
            return _toolItems;
        }

        var filePath = GetAssetPath("site", "tools", "tools.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return _toolItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _toolItems = JsonSerializer.Deserialize<List<ToolItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize tools.json: {ex.Message}");
        }
        return _toolItems;
    }

    public async Task<DocItem?> GetDocItemAsync(string slug)
    {
        if (_docItems?.Any() != true)
        {
            return default;
        }

        foreach (var item in _docItems)
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

        var first = _docItems.FirstOrDefault()?.Children?.FirstOrDefault();
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

        if (_searchCache.TryGetValue(normalizedQuery, out var cacheEntry))
        {
            TouchSearchCacheEntry(normalizedQuery, cacheEntry);
            return CreateSearchPageData(cacheEntry.Snapshot, safePageIndex, safePageSize, kind);
        }

        var snapshot = await BuildSearchSnapshotAsync(normalizedQuery);
        _searchCache[normalizedQuery] = new SearchCacheEntry(snapshot, 1, DateTimeOffset.UtcNow);
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

        await LoadSearchQueryStatsAsync();
        foreach (var item in _searchQueryStats.Values
                     .Where(item => string.IsNullOrWhiteSpace(normalizedQuery)
                         || item.Query.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                     .OrderByDescending(item => item.Count)
                     .ThenByDescending(item => item.LastSearchedAt)
                     .Take(safeSize))
        {
            if (seen.Add(item.Query))
            {
                suggestions.Add(new SearchSuggestionItem(item.Query, "热搜", item.Count));
            }
        }

        if (suggestions.Count >= safeSize || normalizedQuery.Length < 1)
        {
            return suggestions;
        }

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
        if (_searchIndex is not null)
        {
            return _searchIndex;
        }

        await _searchIndexLock.WaitAsync();
        try
        {
            if (_searchIndex is not null)
            {
                return _searchIndex;
            }

            await GetAllBlogPostsAsync();
            await GetAllDocItemsAsync();
            await GetAllToolItemsAsync();

            var index = new List<SearchableEntry>();
            foreach (var tool in GetSearchableToolNodes())
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

            var searchableDocs = await GetSearchableDocNodesAsync();
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

            foreach (var post in _blogPosts ?? [])
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

            _searchIndex = index;
            return _searchIndex;
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
        if (_searchBlockedKeywords is not null)
        {
            return _searchBlockedKeywords;
        }

        var groups = await GetSearchBlockedKeywordGroupsAsync();
        _searchBlockedKeywords = groups
            .SelectMany(static group => group.Keywords ?? [])
            .Select(NormalizeSearchQuery)
            .Where(static keyword => !string.IsNullOrWhiteSpace(keyword))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(static keyword => keyword.Length)
            .ThenBy(static keyword => keyword, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return _searchBlockedKeywords;
    }

    private async Task TrackSearchQueryAsync(string normalizedQuery)
    {
        await LoadSearchQueryStatsAsync();

        var now = DateTimeOffset.UtcNow;
        _searchQueryStats.AddOrUpdate(
            normalizedQuery,
            new SearchQueryStats(normalizedQuery, 1, now),
            (_, current) => current with
            {
                Count = current.Count + 1,
                LastSearchedAt = now
            });

        TrimSearchQueryStats();
        await SaveSearchQueryStatsAsync();
    }

    private async Task LoadSearchQueryStatsAsync()
    {
        if (_searchQueryStatsLoaded)
        {
            return;
        }

        var filePath = GetSearchKeywordsFilePath();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _searchQueryStatsLoaded = true;
            return;
        }

        var blockedKeywords = await GetSearchBlockedKeywordsAsync();

        await _searchQueryStatsFileLock.WaitAsync();
        try
        {
            if (_searchQueryStatsLoaded)
            {
                return;
            }

            if (!File.Exists(filePath))
            {
                _searchQueryStatsLoaded = true;
                return;
            }

            var fileContent = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                _searchQueryStatsLoaded = true;
                return;
            }

            List<SearchQueryStats>? savedStats;
            try
            {
                savedStats = JsonSerializer.Deserialize<List<SearchQueryStats>>(fileContent, JsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize {SearchKeywordsFileName}: {ex.Message}");
                _searchQueryStatsLoaded = true;
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
                _searchQueryStats.AddOrUpdate(
                    query,
                    new SearchQueryStats(query, count, lastSearchedAt),
                    (_, current) => current.Count > count
                                    || (current.Count == count && current.LastSearchedAt >= lastSearchedAt)
                        ? current
                        : new SearchQueryStats(query, count, lastSearchedAt));
            }

            TrimSearchQueryStats();
            _searchQueryStatsLoaded = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load {SearchKeywordsFileName}: {ex.Message}");
            _searchQueryStatsLoaded = true;
        }
        finally
        {
            _searchQueryStatsFileLock.Release();
        }
    }

    private async Task SaveSearchQueryStatsAsync()
    {
        var filePath = GetSearchKeywordsFilePath();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        await _searchQueryStatsFileLock.WaitAsync();
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var data = _searchQueryStats.Values
                .OrderByDescending(static item => item.Count)
                .ThenByDescending(static item => item.LastSearchedAt)
                .Take(SearchQueryStatsLimit)
                .ToList();
            var json = JsonSerializer.Serialize(data, WriteJsonOptions);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write {SearchKeywordsFileName}: {ex.Message}");
        }
        finally
        {
            _searchQueryStatsFileLock.Release();
        }
    }

    private string? GetSearchKeywordsFilePath()
    {
        return GetAssetPath("site", SearchKeywordsFileName);
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

    private void TrimSearchQueryStats()
    {
        if (_searchQueryStats.Count <= SearchQueryStatsLimit)
        {
            return;
        }

        foreach (var item in _searchQueryStats
                     .OrderBy(item => item.Value.Count)
                     .ThenBy(item => item.Value.LastSearchedAt)
                     .Take(_searchQueryStats.Count - SearchQueryStatsLimit))
        {
            _searchQueryStats.TryRemove(item.Key, out _);
        }
    }

    private static string GetSuggestionLabel(SearchResultKind kind) => kind switch
    {
        SearchResultKind.Tool => "工具",
        SearchResultKind.Doc => "项目",
        SearchResultKind.Post => "文章",
        _ => "内容"
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

        var localAssetsDir = GetLocalAssetsDir();
        if (localAssetsDir is null)
        {
            return;
        }

        var contentPath = string.IsNullOrWhiteSpace(parentDir)
            ? Path.Combine(localAssetsDir, "site", "doc", $"{item.Slug}.md")
            : Path.Combine(localAssetsDir, "site", "doc", parentDir, $"{item.Slug}.md");

        if (!File.Exists(contentPath))
        {
            return;
        }

        item.Content = await File.ReadAllTextAsync(contentPath);
        item.HtmlContent = item.Content.ToHtml();
    }

    private IEnumerable<SearchableToolNode> GetSearchableToolNodes()
    {
        if (_toolItems?.Any() != true)
        {
            yield break;
        }

        foreach (var item in _toolItems)
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

    private async Task<List<SearchableDocNode>> GetSearchableDocNodesAsync()
    {
        var nodes = new List<SearchableDocNode>();
        if (_docItems?.Any() != true)
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

        await TraverseAsync(_docItems);
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
        if (_albumItems?.Any() == true)
        {
            return _albumItems;
        }

        var filePath = GetAssetPath("site", "albums.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return _albumItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _albumItems = JsonSerializer.Deserialize<List<AlbumItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize albums.json: {ex.Message}");
        }
        
        if (_albumItems == null)
        {
            _albumItems = new List<AlbumItem>();
        }

        _albumItems.Insert(0, new AlbumItem() { Slug = ConstantUtil.DefaultCategory, Name = "所有" });
        return _albumItems;
    }

    public async Task<List<CategoryItem>?> GetAllCategoryItemsAsync()
    {
        if (_categoryItems?.Any() == true)
        {
            return _categoryItems;
        }

        var filePath = GetAssetPath("site", "categories.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return _categoryItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _categoryItems = JsonSerializer.Deserialize<List<CategoryItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize categories.json: {ex.Message}");
        }
        
        if (_categoryItems == null)
        {
            _categoryItems = new List<CategoryItem>();
        }

        _categoryItems.Insert(0, new CategoryItem() { Slug = ConstantUtil.DefaultCategory, Name = "所有" });
        return _categoryItems;
    }

    public async Task<List<SearchBlockedKeywordGroup>> GetSearchBlockedKeywordGroupsAsync()
    {
        if (_searchBlockedKeywordGroups is not null)
        {
            return _searchBlockedKeywordGroups;
        }

        _searchBlockedKeywordGroups = [];
        var filePath = GetAssetPath("site", "blocked-search-keywords.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return _searchBlockedKeywordGroups;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _searchBlockedKeywordGroups = JsonSerializer.Deserialize<List<SearchBlockedKeywordGroup>>(fileContent, JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize blocked-search-keywords.json: {ex.Message}");
            _searchBlockedKeywordGroups = [];
        }

        foreach (var group in _searchBlockedKeywordGroups)
        {
            group.Keywords = group.Keywords?
                .Select(NormalizeSearchQuery)
                .Where(static keyword => !string.IsNullOrWhiteSpace(keyword))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? [];
        }

        _searchBlockedKeywordGroups = _searchBlockedKeywordGroups
            .Where(static group => group.Keywords is { Count: > 0 })
            .OrderBy(static group => group.Sort)
            .ThenBy(static group => group.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return _searchBlockedKeywordGroups;
    }


    public async Task<List<BlogPost>?> GetAllBlogPostsAsync()
    {
        if (_blogPosts?.Any() == true)
        {
            return _blogPosts;
        }

        var localAssetsDir = GetLocalAssetsDir();
        if (localAssetsDir is null)
        {
            _blogPosts = new List<BlogPost>();
            return _blogPosts;
        }

        _blogPosts = new List<BlogPost>();
        var endYear = DateTime.Now.Year;

        for (var start = siteOption.Value.StartYear; start <= endYear; start++)
        {
            var postDir = Path.Combine(localAssetsDir, start.ToString());
            if (!Directory.Exists(postDir))
            {
                continue;
            }

            var postFiles = Directory.GetFiles(postDir, "*.md", SearchOption.AllDirectories);
            foreach (var postFile in postFiles)
            {
                try
                {
                    var blogPost = await ReadBlogPostAsync(postFile);
                    if (!blogPost.Draft)
                    {
                        _blogPosts.Add(blogPost);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to read blog post {postFile}: {ex.Message}");
                }
            }
        }

        _blogPosts = _blogPosts
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue)
            .ToList();

        return _blogPosts;
    }

    public Task<PageData<BlogPost>> GetPostByAlbum(int pageIndex, int pageSize, string albumSlug,
        string? key)
    {
        AlbumItem? album = null;
        if (!string.Equals(ConstantUtil.DefaultCategory, albumSlug))
        {
            album = _albumItems?.FirstOrDefault(albumDto => albumDto.Slug == albumSlug);
        }

        IEnumerable<BlogPost> posts;
        if (!string.IsNullOrWhiteSpace(key))
        {
            posts = (_blogPosts ?? [])
                .Where(p => p.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Slug?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Author?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.LastModifyUser?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Content?.Contains(key, StringComparison.OrdinalIgnoreCase) == true);
        }
        else
        {
            posts = (_blogPosts ?? [])
                .Where(post => album == null
                               || (!string.IsNullOrWhiteSpace(album.Name)
                                   && post.Albums?.Contains(album.Name, StringComparer.OrdinalIgnoreCase) == true));
        }

        var ordered = posts
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue);

        var total = ordered.Count();
        var postDatas = ordered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(new PageData<BlogPost>(pageIndex, pageSize, total, postDatas));
    }

    public Task<PageData<BlogPost>> GetPostByCategory(int pageIndex, int pageSize, string categorySlug,
        string? key)
    {
        CategoryItem? cat = null;
        if (!string.Equals(ConstantUtil.DefaultCategory, categorySlug))
        {
            cat = _categoryItems?.FirstOrDefault(cat => cat.Slug == categorySlug);
        }

        IEnumerable<BlogPost> posts;
        if (!string.IsNullOrWhiteSpace(key))
        {
            posts = (_blogPosts ?? [])
                .Where(p => p.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Slug?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Author?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.LastModifyUser?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Content?.Contains(key, StringComparison.OrdinalIgnoreCase) == true);
        }
        else
        {
            posts = (_blogPosts ?? [])
                .Where(post => cat == null
                               || (!string.IsNullOrWhiteSpace(cat.Name)
                                   && post.Categories?.Contains(cat.Name, StringComparer.OrdinalIgnoreCase) == true));
        }

        var ordered = posts
            .OrderByDescending(post => post.Lastmod ?? post.Date ?? DateTime.MinValue)
            .ThenByDescending(post => post.Date ?? DateTime.MinValue);

        var total = ordered.Count();
        var postDatas = ordered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(new PageData<BlogPost>(pageIndex, pageSize, total, postDatas));
    }

    public async Task<List<TagItem>> GetAllTagItemsAsync()
    {
        await GetAllBlogPostsAsync();

        return (_blogPosts ?? [])
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

    public async Task<PageData<BlogPost>> GetPostByTag(int pageIndex, int pageSize, string tag, string? key = null)
    {
        await GetAllBlogPostsAsync();

        var normalizedTag = ConstantUtil.NormalizeTagName(tag);
        IEnumerable<BlogPost> posts;
        if (!string.IsNullOrWhiteSpace(key))
        {
            posts = (_blogPosts ?? [])
                .Where(p => p.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Slug?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Author?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Content?.Contains(key, StringComparison.OrdinalIgnoreCase) == true
                            || p.Tags?.Any(tagItem => tagItem.Contains(key, StringComparison.OrdinalIgnoreCase)) == true);
        }
        else
        {
            posts = (_blogPosts ?? [])
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
            .ToList();

        return new PageData<BlogPost>(pageIndex, pageSize, total, data);
    }

    public Task<List<BlogPost>?> GetBannerPostAsync()
    {
        var posts = _blogPosts
            ?.Where(post => post.Banner)
            .OrderByDescending(post => post.Date)
            .ToList();
        return Task.FromResult(posts);
    }

    public Task<PageData<BlogPost>> GetPagedBlogPostsAsync(int pageIndex, int pageSize, string? key = null)
    {
        var source = _blogPosts?.AsEnumerable() ?? [];

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
            .ToList();

        return Task.FromResult(new PageData<BlogPost>(pageIndex, pageSize, total, data));
    }

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
        if (!string.IsNullOrWhiteSpace(_aboutMarkdown))
        {
            return (_aboutMarkdown, _aboutHtmlContent);
        }

        var filePath = GetAssetPath("site", "about.md");
        if (filePath is null || !File.Exists(filePath))
        {
            return ("## 关于", "关于");
        }

        _aboutMarkdown = await File.ReadAllTextAsync(filePath);
        _aboutHtmlContent = _aboutMarkdown.ToHtml();
        return (_aboutMarkdown, _aboutHtmlContent);
    }

    public async Task<(string? Markdown, string? HtmlContent)> ReadDonationAsync()
    {
        if (!string.IsNullOrWhiteSpace(_donationMarkdown))
        {
            return (_donationMarkdown, _donationHtmlContent);
        }

        var filePath = GetAssetPath("site", "pays", "Donation.md");
        if (filePath is null || !File.Exists(filePath))
        {
            return ("## 赞助", "赞助");
        }

        _donationMarkdown = await File.ReadAllTextAsync(filePath);
        _donationHtmlContent = _donationMarkdown.ToHtml();
        return (_donationMarkdown, _donationHtmlContent);
    }

    public async Task<BlogPost?> GetPostBySlug(string slug)
    {
        var post = _blogPosts?.FirstOrDefault(post =>
            string.Equals(post.Slug, slug, StringComparison.OrdinalIgnoreCase));
        return post;
    }

    public static async Task<BlogPost> ReadBlogPostAsync(string markdownFilePath)
    {
        var markdown = await File.ReadAllTextAsync(markdownFilePath);
        var endOfFrontMatter = markdown.IndexOf("---", 3, StringComparison.Ordinal);
        if (endOfFrontMatter == -1)
        {
            throw new InvalidOperationException("Invalid markdown format. No ending '---' found for Front Matter.");
        }

        var frontMatterText = markdown[..(endOfFrontMatter + 3)];
        var markdownContent = markdown[(endOfFrontMatter + 3)..].Trim();
        if (frontMatterText.StartsWith("---"))
        {
            frontMatterText = frontMatterText[3..].Trim();
        }

        if (frontMatterText.EndsWith("---"))
        {
            frontMatterText = frontMatterText[..^3].Trim();
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        BlogPost blogPost;
        try
        {
            blogPost = deserializer.Deserialize<BlogPost>(frontMatterText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize blog post front matter: {markdownFilePath}. {ex.Message}");

            blogPost = new BlogPost();
        }

        blogPost.Content = markdownContent;
        blogPost.HtmlContent = markdownContent.ToHtml();

        return blogPost;
    }

    public async Task<List<FriendLinkItem>?> GetAllFriendLinkItemsAsync()
    {
        if (_friendLinkItems?.Any() == true)
        {
            return _friendLinkItems;
        }

        var filePath = GetAssetPath("site", "friend-links.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return _friendLinkItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _friendLinkItems = JsonSerializer.Deserialize<List<FriendLinkItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize friend-links.json: {ex.Message}");
        }
        return _friendLinkItems;
    }

    public async Task<List<TimeLineItem>?> GetTimeLineItemsAsync()
    {
        if (_timeLineItems?.Any() == true)
        {
            return _timeLineItems;
        }

        var filePath = GetAssetPath("site", "timelines.json");
        if (filePath is null || !File.Exists(filePath))
        {
            return _timeLineItems;
        }

        var fileContent = await File.ReadAllTextAsync(filePath);
        try
        {
            _timeLineItems = JsonSerializer.Deserialize<List<TimeLineItem>>(fileContent, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize timelines.json: {ex.Message}");
        }
        return _timeLineItems;
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

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine(
            "<rss xmlns:atom=\"http://www.w3.org/2005/Atom\" xmlns:content=\"http://purl.org/rss/1.0/modules/content/\" version=\"2.0\">");
        sb.Append("<channel>");
        sb.Append(
            $"<atom:link rel=\"self\" type=\"application/rss+xml\" href=\"{XmlEncode($"{siteOption.Value.Domain}/rss")}\"/>");
        sb.Append($"<title>{XmlEncode($"{siteOption.Value.AppTitle}_{siteOption.Value.Memo}")}</title>");
        sb.Append($"<link>{XmlEncode($"{siteOption.Value.Domain}/rss")}</link>");
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

            var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
            var url = $"{domain}{normalizedPath}";
            if (!seenUrls.Add(url))
            {
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
}
