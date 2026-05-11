using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebApp.Services;

internal static class StructuredContentTranslation
{
    private const int IndexedBatchLineNumberWidth = 6;

    private static readonly Regex ChineseTextRegex = new(@"[\u3400-\u9fff]", RegexOptions.Compiled);
    private static readonly Regex YamlKeyValueRegex = new(@"^(?<prefix>\s*)(?<key>[A-Za-z][\w-]*)\s*:\s*(?<value>.*)$", RegexOptions.Compiled);
    private static readonly Regex YamlListItemRegex = new(@"^(?<prefix>\s*-\s*)(?<value>.*)$", RegexOptions.Compiled);
    private static readonly Regex MarkdownLinePrefixRegex = new(@"^(?<prefix>\s*(?:#{1,6}\s+|[-*+]\s+|\d+\.\s+|>\s*)*)(?<text>.*)$", RegexOptions.Compiled);
    private static readonly Regex MarkdownTableSeparatorCellRegex = new(@"^:?-{3,}:?$", RegexOptions.Compiled);

    // 结构化 JSON payload 里不是所有字符串都该翻译，路由、CSS 类名、文件路径等字段必须原样保留。
    private static readonly HashSet<string> JsonValueSkipNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "slug",
        "url",
        "urls",
        "href",
        "link",
        "links",
        "repository",
        "cover",
        "banner",
        "icon",
        "id",
        "key",
        "code",
        "path",
        "route",
        "routes",
        "file",
        "fileName",
        "filename",
        "class",
        "css",
        "method",
        "target",
        "license",
        "version",
        "package",
        "keywords",
        "keyword",
        "pattern"
    };

    private static readonly HashSet<string> TranslatableYamlListKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "categories",
        "albums",
        "tags"
    };

    public static async Task<string?> TranslateAsync(
        string source,
        LanguageInfo targetLanguage,
        ContentTranslationKind kind,
        string resource,
        int maxCharsPerRequest,
        TextChunkTranslator translateChunkAsync,
        CancellationToken cancellationToken)
    {
        return kind switch
        {
            // 文章 metadata 是结构化 JSON payload，只翻译标题、摘要、分类、专辑和标签等人类可读字段。
            ContentTranslationKind.ArticleMetadata => await TranslateStructuredJsonPayloadAsync(
                source,
                targetLanguage,
                resource,
                maxCharsPerRequest,
                translateChunkAsync,
                cancellationToken),
            ContentTranslationKind.MarkdownArticle => await TranslateMarkdownAsync(
                source,
                targetLanguage,
                resource,
                maxCharsPerRequest,
                translateChunkAsync,
                translateFrontMatter: true,
                cancellationToken),
            _ => await TranslateMarkdownAsync(
                source,
                targetLanguage,
                resource,
                maxCharsPerRequest,
                translateChunkAsync,
                translateFrontMatter: false,
                cancellationToken)
        };
    }

    private static async Task<string?> TranslateStructuredJsonPayloadAsync(
        string source,
        LanguageInfo targetLanguage,
        string resource,
        int maxCharsPerRequest,
        TextChunkTranslator translateChunkAsync,
        CancellationToken cancellationToken)
    {
        try
        {
            // 不直接把整份 JSON 交给翻译 API：API 可能翻译 key 或破坏转义。
            // 这里只抽出需要翻译的 value 批量发送，最后再把翻译结果写回原结构。
            using var document = JsonDocument.Parse(source, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
            var stringsToTranslate = new List<string>();
            CollectTranslatableJsonStrings(document.RootElement, null, stringsToTranslate);
            var translations = await TranslateStructuredJsonStringsAsync(
                stringsToTranslate,
                targetLanguage,
                resource,
                maxCharsPerRequest,
                translateChunkAsync,
                cancellationToken);

            await using var stream = new MemoryStream();
            await using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = true
            }))
            {
                WriteTranslatedJsonElement(
                    writer,
                    document.RootElement,
                    null,
                    translations);
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
        catch (JsonException)
        {
            return await TranslateLargeTextAsync(
                source,
                targetLanguage,
                resource,
                maxCharsPerRequest,
                translateChunkAsync,
                cancellationToken);
        }
    }

    private static void CollectTranslatableJsonStrings(
        JsonElement element,
        string? propertyName,
        List<string> stringsToTranslate)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    CollectTranslatableJsonStrings(property.Value, property.Name, stringsToTranslate);
                }

                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    CollectTranslatableJsonStrings(item, propertyName, stringsToTranslate);
                }

                break;
            case JsonValueKind.String:
                var value = element.GetString();
                if (ShouldTranslateJsonString(value, propertyName))
                {
                    stringsToTranslate.Add(value!);
                }

                break;
        }
    }

    private static async Task<IReadOnlyDictionary<string, string>> TranslateStructuredJsonStringsAsync(
        IReadOnlyList<string> stringsToTranslate,
        LanguageInfo targetLanguage,
        string resource,
        int maxCharsPerRequest,
        TextChunkTranslator translateChunkAsync,
        CancellationToken cancellationToken)
    {
        // 相同中文只翻译一次，既减少请求字符数，也避免同一资源内重复扣费。
        return await TranslateDistinctTextValuesAsync(
            stringsToTranslate,
            targetLanguage,
            resource,
            maxCharsPerRequest,
            translateChunkAsync,
            "article metadata JSON",
            cancellationToken);
    }

    private static async Task<IReadOnlyDictionary<string, string>> TranslateDistinctTextValuesAsync(
        IReadOnlyList<string> stringsToTranslate,
        LanguageInfo targetLanguage,
        string resource,
        int maxCharsPerRequest,
        TextChunkTranslator translateChunkAsync,
        string contentKind,
        CancellationToken cancellationToken)
    {
        if (stringsToTranslate.Count == 0)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var distinctSources = new List<string>();
        var sourceIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var source in stringsToTranslate)
        {
            if (!sourceIndexes.ContainsKey(source))
            {
                sourceIndexes[source] = distinctSources.Count;
                distinctSources.Add(source);
            }
        }

        if (distinctSources.Count == 1)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                // 单条文本不走编号批量协议，降低翻译器误改序号或格式的概率。
                [distinctSources[0]] = await TranslateRequiredAsync(
                    distinctSources[0],
                    targetLanguage,
                    resource,
                    maxCharsPerRequest,
                    translateChunkAsync,
                    cancellationToken)
            };
        }

        var translatedSources = new string[distinctSources.Count];
        var batches = CreateIndexedTranslationBatches(distinctSources, maxCharsPerRequest).ToList();
        for (var i = 0; i < batches.Count; i++)
        {
            var batch = batches[i];
            if (!batch.IsIndexedLineBatch)
            {
                var sourceIndex = batch.SourceIndexes[0];
                translatedSources[sourceIndex] = await TranslateRequiredAsync(
                    distinctSources[sourceIndex],
                    targetLanguage,
                    resource,
                    maxCharsPerRequest,
                    translateChunkAsync,
                    cancellationToken);
                continue;
            }

            var translated = await translateChunkAsync(
                batch.Source,
                targetLanguage,
                resource,
                i + 1,
                batches.Count,
                cancellationToken);
            string? parseError = null;
            if (translated is null
                || !TryReadIndexedBatchTranslations(translated, batch.SourceIndexes, translatedSources, out parseError))
            {
                throw new InvalidOperationException(
                    $"{contentKind} translation batch returned an invalid indexed result. language={targetLanguage.Code}; resource={resource}; batch={i + 1}/{batches.Count}; reason={parseError ?? "empty result"}.");
            }
        }

        var translations = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in sourceIndexes)
        {
            translations[item.Key] = translatedSources[item.Value];
        }

        return translations;
    }

    private static IEnumerable<IndexedTranslationBatch> CreateIndexedTranslationBatches(
        IReadOnlyList<string> sources,
        int maxCharsPerRequest)
    {
        var maxChars = Math.Clamp(maxCharsPerRequest, 500, 6000);
        var currentIndexes = new List<int>();
        var currentValues = new List<string>();

        for (var i = 0; i < sources.Count; i++)
        {
            // 腾讯文本翻译会把 JSON 当普通文本处理，返回值不一定是合法 JSON。
            // 批量协议改用“固定数字序号 + 制表符 + 文本”的行协议，避免 JSON 转义被翻译器破坏。
            var canUseLineBatch = !ContainsLineBreak(sources[i]);
            var singlePayload = canUseLineBatch
                ? CreateIndexedBatchPayload([i], [sources[i]])
                : sources[i];
            if (!canUseLineBatch || singlePayload.Length > maxChars)
            {
                if (currentIndexes.Count > 0)
                {
                    yield return new IndexedTranslationBatch(
                        currentIndexes.ToArray(),
                        CreateIndexedBatchPayload(currentIndexes, currentValues),
                        IsIndexedLineBatch: true);
                    currentIndexes.Clear();
                    currentValues.Clear();
                }

                yield return new IndexedTranslationBatch([i], sources[i], IsIndexedLineBatch: false);
                continue;
            }

            currentValues.Add(sources[i]);
            var payload = CreateIndexedBatchPayload(currentIndexes.Append(i), currentValues);
            if (payload.Length > maxChars)
            {
                currentValues.RemoveAt(currentValues.Count - 1);
                yield return new IndexedTranslationBatch(
                    currentIndexes.ToArray(),
                    CreateIndexedBatchPayload(currentIndexes, currentValues),
                    IsIndexedLineBatch: true);
                currentIndexes.Clear();
                currentValues.Clear();
                currentValues.Add(sources[i]);
            }

            currentIndexes.Add(i);
        }

        if (currentIndexes.Count > 0)
        {
            yield return new IndexedTranslationBatch(
                currentIndexes.ToArray(),
                CreateIndexedBatchPayload(currentIndexes, currentValues),
                IsIndexedLineBatch: true);
        }
    }

    private static string CreateIndexedBatchPayload(
        IEnumerable<int> sourceIndexes,
        IReadOnlyList<string> values)
    {
        var builder = new StringBuilder();
        var valueIndex = 0;
        foreach (var sourceIndex in sourceIndexes)
        {
            if (builder.Length > 0)
            {
                builder.Append('\n');
            }

            builder
                .Append(sourceIndex.ToString($"D{IndexedBatchLineNumberWidth}"))
                .Append('\t')
                .Append(values[valueIndex]);
            valueIndex++;
        }

        return builder.ToString();
    }

    private static bool TryReadIndexedBatchTranslations(
        string translatedBatch,
        IReadOnlyList<int> sourceIndexes,
        string[] translatedSources,
        out string? error)
    {
        error = null;
        var expectedIndexes = sourceIndexes.ToHashSet();
        var translatedByIndex = new Dictionary<int, string>();
        var lines = NormalizeLineEndings(translatedBatch).Split('\n');
        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!TryReadIndexedLine(line, out var sourceIndex, out var translated))
            {
                error = $"line {lineIndex + 1} does not start with a numeric translation index";
                return false;
            }

            if (!expectedIndexes.Contains(sourceIndex))
            {
                error = $"unexpected translation index; sourceIndex={sourceIndex}";
                return false;
            }

            if (!translatedByIndex.TryAdd(sourceIndex, translated))
            {
                error = $"duplicate translation index; sourceIndex={sourceIndex}";
                return false;
            }
        }

        foreach (var sourceIndex in sourceIndexes)
        {
            if (!translatedByIndex.TryGetValue(sourceIndex, out var translated))
            {
                error = $"translation index not found; sourceIndex={sourceIndex}";
                return false;
            }

            translatedSources[sourceIndex] = translated;
        }

        return true;
    }

    private static bool TryReadIndexedLine(string line, out int sourceIndex, out string translated)
    {
        sourceIndex = -1;
        translated = string.Empty;
        var span = line.AsSpan().TrimStart();
        var digitCount = 0;
        while (digitCount < span.Length && char.IsDigit(span[digitCount]))
        {
            digitCount++;
        }

        if (digitCount == 0 || !int.TryParse(span[..digitCount], out sourceIndex))
        {
            return false;
        }

        span = span[digitCount..];
        span = span.TrimStart();
        if (span.Length > 0 && IsIndexSeparator(span[0]))
        {
            span = span[1..].TrimStart();
        }

        translated = span.ToString();
        return true;
    }

    private static bool IsIndexSeparator(char value) =>
        value is '\t' or ':' or '：' or '-' or '－' or '.' or '。' or '、';

    private static bool ContainsLineBreak(string value) =>
        value.Contains('\n', StringComparison.Ordinal) || value.Contains('\r', StringComparison.Ordinal);

    private static void WriteTranslatedJsonElement(
        Utf8JsonWriter writer,
        JsonElement element,
        string? propertyName,
        IReadOnlyDictionary<string, string> translations)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);
                    WriteTranslatedJsonElement(writer, property.Value, property.Name, translations);
                }

                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteTranslatedJsonElement(writer, item, propertyName, translations);
                }

                writer.WriteEndArray();
                break;
            case JsonValueKind.String:
                var value = element.GetString();
                writer.WriteStringValue(value is not null
                                        && ShouldTranslateJsonString(value, propertyName)
                                        && translations.TryGetValue(value, out var translated)
                    ? translated
                    : value);
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }

    private sealed record IndexedTranslationBatch(int[] SourceIndexes, string Source, bool IsIndexedLineBatch);

    private sealed record MarkdownTranslationSegment(
        int LineIndex,
        int? CellIndex,
        string Prefix,
        string LeadingWhitespace,
        string Source,
        string TrailingWhitespace);

    private static bool ShouldTranslateJsonString(string? value, string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(value) || !ChineseTextRegex.IsMatch(value))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(propertyName) || !JsonValueSkipNames.Contains(propertyName);
    }

    private static async Task<string?> TranslateMarkdownAsync(
        string source,
        LanguageInfo targetLanguage,
        string resource,
        int maxCharsPerRequest,
        TextChunkTranslator translateChunkAsync,
        bool translateFrontMatter,
        CancellationToken cancellationToken)
    {
        if (translateFrontMatter && TrySplitFrontMatter(source, out var frontMatter, out var body))
        {
            var translatedFrontMatter = await TranslateYamlFrontMatterAsync(
                frontMatter,
                targetLanguage,
                resource,
                maxCharsPerRequest,
                translateChunkAsync,
                cancellationToken);
            var translatedBody = await TranslateMarkdownBodyAsync(
                body,
                targetLanguage,
                resource,
                maxCharsPerRequest,
                translateChunkAsync,
                cancellationToken);
            return $"---\n{translatedFrontMatter}\n---\n\n{translatedBody}";
        }

        return await TranslateMarkdownBodyAsync(
            source,
            targetLanguage,
            resource,
            maxCharsPerRequest,
            translateChunkAsync,
            cancellationToken);
    }

    private static async Task<string> TranslateYamlFrontMatterAsync(
        string frontMatter,
        LanguageInfo targetLanguage,
        string resource,
        int maxCharsPerRequest,
        TextChunkTranslator translateChunkAsync,
        CancellationToken cancellationToken)
    {
        var lines = NormalizeLineEndings(frontMatter).Split('\n');
        string? activeListKey = null;
        for (var i = 0; i < lines.Length; i++)
        {
            var keyValueMatch = YamlKeyValueRegex.Match(lines[i]);
            if (keyValueMatch.Success)
            {
                var key = keyValueMatch.Groups["key"].Value;
                var value = keyValueMatch.Groups["value"].Value;
                activeListKey = TranslatableYamlListKeys.Contains(key) ? key : null;
                if (IsTranslatableYamlScalarKey(key) && ChineseTextRegex.IsMatch(value))
                {
                    lines[i] = $"{keyValueMatch.Groups["prefix"].Value}{key}: {await TranslateYamlScalarAsync(
                        value,
                        targetLanguage,
                        resource,
                        maxCharsPerRequest,
                        translateChunkAsync,
                        cancellationToken)}";
                }

                continue;
            }

            if (activeListKey is not null)
            {
                var listItemMatch = YamlListItemRegex.Match(lines[i]);
                if (listItemMatch.Success && ChineseTextRegex.IsMatch(listItemMatch.Groups["value"].Value))
                {
                    lines[i] = $"{listItemMatch.Groups["prefix"].Value}{await TranslateYamlScalarAsync(
                        listItemMatch.Groups["value"].Value,
                        targetLanguage,
                        resource,
                        maxCharsPerRequest,
                        translateChunkAsync,
                        cancellationToken)}";
                }
            }
        }

        return string.Join('\n', lines);
    }

    private static bool IsTranslatableYamlScalarKey(string key) =>
        string.Equals(key, "title", StringComparison.OrdinalIgnoreCase)
        || string.Equals(key, "description", StringComparison.OrdinalIgnoreCase);

    private static async Task<string> TranslateYamlScalarAsync(
        string value,
        LanguageInfo targetLanguage,
        string resource,
        int maxCharsPerRequest,
        TextChunkTranslator translateChunkAsync,
        CancellationToken cancellationToken)
    {
        var trimmed = value.Trim();
        if (trimmed.Length < 2)
        {
            return value;
        }

        var quote = trimmed[0] is '"' or '\'' && trimmed[^1] == trimmed[0]
            ? trimmed[0]
            : '\0';
        var content = quote == '\0' ? trimmed : trimmed[1..^1];
        var translated = await TranslateRequiredAsync(
            content,
            targetLanguage,
            resource,
            maxCharsPerRequest,
            translateChunkAsync,
            cancellationToken);
        var output = quote == '\0' ? translated : $"{quote}{translated}{quote}";
        return $"{value[..(value.Length - value.TrimStart().Length)]}{output}";
    }

    private static async Task<string> TranslateMarkdownBodyAsync(
        string source,
        LanguageInfo targetLanguage,
        string resource,
        int maxCharsPerRequest,
        TextChunkTranslator translateChunkAsync,
        CancellationToken cancellationToken)
    {
        var lines = NormalizeLineEndings(source).Split('\n');
        var segments = new List<MarkdownTranslationSegment>();
        var tableCellsByLine = new Dictionary<int, string[]>();
        var inFence = false;
        for (var i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("```", StringComparison.Ordinal) || trimmed.StartsWith("~~~", StringComparison.Ordinal))
            {
                inFence = !inFence;
                continue;
            }

            if (inFence || !ChineseTextRegex.IsMatch(lines[i]))
            {
                continue;
            }

            CollectMarkdownLineSegments(
                lines[i],
                i,
                segments,
                tableCellsByLine);
        }

        if (segments.Count > 0)
        {
            // Markdown 不能整篇原样交给翻译 API，否则代码块、链接和表格分隔符都可能被改坏。
            // 这里只抽取安全的正文片段，再用带序号的行协议尽量凑满单次 6000 字符上限。
            var translations = await TranslateDistinctTextValuesAsync(
                segments.Select(static segment => segment.Source).ToList(),
                targetLanguage,
                resource,
                maxCharsPerRequest,
                translateChunkAsync,
                "Markdown",
                cancellationToken);

            foreach (var segment in segments)
            {
                var translated = translations.TryGetValue(segment.Source, out var value)
                    ? value
                    : segment.Source;
                var replacement = $"{segment.LeadingWhitespace}{translated}{segment.TrailingWhitespace}";
                if (segment.CellIndex.HasValue)
                {
                    tableCellsByLine[segment.LineIndex][segment.CellIndex.Value] = replacement;
                }
                else
                {
                    lines[segment.LineIndex] = $"{segment.Prefix}{replacement}";
                }
            }

            foreach (var item in tableCellsByLine)
            {
                lines[item.Key] = string.Join('|', item.Value);
            }
        }

        return string.Join('\n', lines).Trim();
    }

    private static void CollectMarkdownLineSegments(
        string line,
        int lineIndex,
        List<MarkdownTranslationSegment> segments,
        Dictionary<int, string[]> tableCellsByLine)
    {
        if (line.TrimStart().StartsWith("|", StringComparison.Ordinal) && line.Contains('|', StringComparison.Ordinal))
        {
            var cells = line.Split('|');
            for (var i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                if (string.IsNullOrWhiteSpace(cell)
                    || MarkdownTableSeparatorCellRegex.IsMatch(cell.Trim())
                    || ContainsUnsafeInlineMarkdown(cell))
                {
                    continue;
                }

                TryAddMarkdownTranslationSegment(
                    cell,
                    lineIndex,
                    i,
                    string.Empty,
                    segments);
            }

            if (segments.Any(segment => segment.LineIndex == lineIndex && segment.CellIndex.HasValue))
            {
                tableCellsByLine[lineIndex] = cells;
            }

            return;
        }

        var match = MarkdownLinePrefixRegex.Match(line);
        if (!match.Success)
        {
            TryAddMarkdownTranslationSegment(
                line,
                lineIndex,
                null,
                string.Empty,
                segments);
            return;
        }

        var text = match.Groups["text"].Value;
        if (ContainsUnsafeInlineMarkdown(text))
        {
            return;
        }

        TryAddMarkdownTranslationSegment(
            text,
            lineIndex,
            null,
            match.Groups["prefix"].Value,
            segments);
    }

    private static bool TryAddMarkdownTranslationSegment(
        string value,
        int lineIndex,
        int? cellIndex,
        string prefix,
        List<MarkdownTranslationSegment> segments)
    {
        if (!ChineseTextRegex.IsMatch(value))
        {
            return false;
        }

        var start = 0;
        var end = value.Length - 1;
        while (start <= end && char.IsWhiteSpace(value[start]))
        {
            start++;
        }

        while (end >= start && char.IsWhiteSpace(value[end]))
        {
            end--;
        }

        var core = value[start..(end + 1)];
        if (!ChineseTextRegex.IsMatch(core))
        {
            return false;
        }

        segments.Add(new MarkdownTranslationSegment(
            lineIndex,
            cellIndex,
            prefix,
            value[..start],
            core,
            value[(end + 1)..]));
        return true;
    }

    private static bool ContainsUnsafeInlineMarkdown(string value) =>
        value.Contains("](", StringComparison.Ordinal) || value.Contains('`', StringComparison.Ordinal);

    private static async Task<string> TranslateRequiredAsync(
        string source,
        LanguageInfo targetLanguage,
        string resource,
        int maxCharsPerRequest,
        TextChunkTranslator translateChunkAsync,
        CancellationToken cancellationToken)
    {
        var translated = await TranslateLargeTextAsync(
            source,
            targetLanguage,
            resource,
            maxCharsPerRequest,
            translateChunkAsync,
            cancellationToken);
        if (translated is null)
        {
            throw new InvalidOperationException(
                $"Content translation provider returned no translation result. language={targetLanguage.Code}; resource={resource}; inputChars={source.Length}.");
        }

        return translated;
    }

    private static async Task<string?> TranslateLargeTextAsync(
        string source,
        LanguageInfo targetLanguage,
        string resource,
        int maxCharsPerRequest,
        TextChunkTranslator translateChunkAsync,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(source) || !ChineseTextRegex.IsMatch(source))
        {
            return source;
        }

        var chunks = SplitIntoChunks(source, maxCharsPerRequest).ToList();
        var translatedChunks = new List<string>(chunks.Count);
        for (var i = 0; i < chunks.Count; i++)
        {
            var translated = await translateChunkAsync(
                chunks[i],
                targetLanguage,
                resource,
                i + 1,
                chunks.Count,
                cancellationToken);
            if (translated is null)
            {
                return null;
            }

            translatedChunks.Add(translated);
        }

        return string.Concat(translatedChunks);
    }

    private static IEnumerable<string> SplitIntoChunks(string source, int maxChars)
    {
        maxChars = Math.Clamp(maxChars, 500, 6000);
        var remaining = source;
        while (remaining.Length > maxChars)
        {
            var splitIndex = remaining.LastIndexOf('\n', maxChars - 1);
            if (splitIndex < maxChars / 2)
            {
                splitIndex = remaining.LastIndexOfAny(['。', '！', '？', '.', '!', '?', ';', '；'], maxChars - 1);
            }

            if (splitIndex < maxChars / 2)
            {
                splitIndex = maxChars;
            }

            yield return remaining[..splitIndex];
            remaining = remaining[splitIndex..];
        }

        if (remaining.Length > 0)
        {
            yield return remaining;
        }
    }

    private static bool TrySplitFrontMatter(string source, out string frontMatter, out string body)
    {
        var normalized = NormalizeLineEndings(source);
        frontMatter = string.Empty;
        body = normalized;
        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            return false;
        }

        var end = normalized.IndexOf("\n---", 4, StringComparison.Ordinal);
        if (end < 0)
        {
            return false;
        }

        var bodyStart = normalized.IndexOf('\n', end + 4);
        frontMatter = normalized[4..end].Trim('\n');
        body = bodyStart < 0 ? string.Empty : normalized[(bodyStart + 1)..].TrimStart('\n');
        return true;
    }

    private static string NormalizeLineEndings(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}
