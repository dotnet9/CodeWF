using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using CodeWfLogger = CodeWF.Log.Core.Logger;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using WebApp.Options;
using AiChatMessage = Microsoft.Extensions.AI.ChatMessage;
using AiChatRole = Microsoft.Extensions.AI.ChatRole;
using OpenAIChatClient = OpenAI.Chat.ChatClient;

namespace WebApp.Services;

public sealed class OpenAiContentTranslationService : IContentTranslationService
{
    private static readonly Regex FencedResponseRegex = new(
        "^```(?:markdown|md|json)?\\s*(?<content>[\\s\\S]*?)\\s*```$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IOptions<OpenAIOption> _options;
    private readonly ILogger<OpenAiContentTranslationService> _logger;

    public OpenAiContentTranslationService(
        IOptions<OpenAIOption> options,
        ILogger<OpenAiContentTranslationService> logger)
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
        if (string.IsNullOrWhiteSpace(option.Key)
            || string.IsNullOrWhiteSpace(option.ChatModel)
            || string.Equals(option.Key.Trim(), "your key", StringComparison.OrdinalIgnoreCase))
        {
            CodeWfLogger.Warn(
                $"语言内容翻译跳过：OpenAI:Key 或 OpenAI:ChatModel 未配置。language={targetLanguage.Code}; kind={kind}; resource={resource}; inputChars={source.Length}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogWarning(
                "OpenAI translation is skipped because OpenAI:Key or OpenAI:ChatModel is not configured. Language={Language}; Kind={Kind}; Resource={Resource}; InputChars={InputChars}.",
                targetLanguage.Code,
                kind,
                resource,
                source.Length);
            return null;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var networkTimeout = GetNetworkTimeout(option);
            var maxRetries = GetMaxRetries(option);
            CodeWfLogger.Info(
                $"语言内容翻译开始。language={targetLanguage.Code}; culture={targetLanguage.DotNetCulture}; kind={kind}; resource={resource}; inputChars={source.Length}; networkTimeoutSeconds={networkTimeout.TotalSeconds:0}; maxRetries={maxRetries}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "Translating content to {Language}. Kind={Kind}; Resource={Resource}; InputChars={InputChars}; NetworkTimeoutSeconds={NetworkTimeoutSeconds}; MaxRetries={MaxRetries}.",
                targetLanguage.Code,
                kind,
                resource,
                source.Length,
                networkTimeout.TotalSeconds,
                maxRetries);
            var client = CreateChatClient(option);
            var targetName = $"{targetLanguage.NativeName} ({targetLanguage.DotNetCulture})";
            var translated = NormalizeResponse(await TranslateStreamingAsync(
                client,
                BuildStatelessTranslationMessages(kind, targetName, source),
                CreateStatelessTranslationOptions(source),
                cancellationToken));
            stopwatch.Stop();
            CodeWfLogger.Info(
                $"语言内容翻译完成。language={targetLanguage.Code}; kind={kind}; resource={resource}; outputChars={translated.Length}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogInformation(
                "Translated content to {Language}. Kind={Kind}; Resource={Resource}; OutputChars={OutputChars}; ElapsedMs={ElapsedMs}.",
                targetLanguage.Code,
                kind,
                resource,
                translated.Length,
                stopwatch.ElapsedMilliseconds);
            return string.IsNullOrWhiteSpace(translated) ? null : translated;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            CodeWfLogger.Error(
                $"语言内容翻译失败。language={targetLanguage.Code}; kind={kind}; resource={resource}; inputChars={source.Length}; elapsedMs={stopwatch.ElapsedMilliseconds}.",
                ex,
                log2UI: false,
                log2File: false,
                log2Console: true);
            _logger.LogError(
                ex,
                "Failed to translate content to {Language}. Kind={Kind}; Resource={Resource}; InputChars={InputChars}; ElapsedMs={ElapsedMs}.",
                targetLanguage.Code,
                kind,
                resource,
                source.Length,
                stopwatch.ElapsedMilliseconds);
            return null;
        }
    }

    private static string NormalizeResourceName(string? resourceName) =>
        string.IsNullOrWhiteSpace(resourceName)
            ? "unknown"
            : resourceName.Trim().Replace('\r', ' ').Replace('\n', ' ');

    private static async Task<string> TranslateStreamingAsync(
        IChatClient client,
        IReadOnlyList<AiChatMessage> messages,
        ChatOptions options,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        await foreach (var update in client.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                builder.Append(update.Text);
            }
        }

        return builder.ToString();
    }

    private static AiChatMessage[] BuildStatelessTranslationMessages(
        ContentTranslationKind kind,
        string targetName,
        string source) =>
        [
            new(AiChatRole.System, BuildSystemPrompt(kind, targetName)),
            new(AiChatRole.User, source)
        ];

    private static ChatOptions CreateStatelessTranslationOptions(string source) =>
        new()
        {
            ConversationId = null,
            Temperature = 0.2f,
            MaxOutputTokens = EstimateMaxOutputTokens(source)
        };

    private static IChatClient CreateChatClient(OpenAIOption option)
    {
        var clientOptions = new OpenAIClientOptions
        {
            NetworkTimeout = GetNetworkTimeout(option),
            RetryPolicy = new ClientRetryPolicy(GetMaxRetries(option))
        };
        if (!string.IsNullOrWhiteSpace(option.Endpoint))
        {
            clientOptions.Endpoint = new Uri(option.Endpoint.Trim());
        }

        OpenAIChatClient chatClient = new(
            model: option.ChatModel!.Trim(),
            credential: new ApiKeyCredential(option.Key!.Trim()),
            options: clientOptions);

        return chatClient.AsIChatClient();
    }

    private static TimeSpan GetNetworkTimeout(OpenAIOption option)
    {
        var seconds = option.NetworkTimeoutSeconds.GetValueOrDefault(OpenAIOption.DefaultNetworkTimeoutSeconds);
        if (seconds <= 0)
        {
            seconds = OpenAIOption.DefaultNetworkTimeoutSeconds;
        }

        return TimeSpan.FromSeconds(seconds);
    }

    private static int GetMaxRetries(OpenAIOption option)
    {
        var maxRetries = option.MaxRetries.GetValueOrDefault(OpenAIOption.DefaultMaxRetries);
        return maxRetries < 0 ? OpenAIOption.DefaultMaxRetries : maxRetries;
    }

    private static string BuildSystemPrompt(ContentTranslationKind kind, string targetName)
    {
        var basePrompt = $"""
            You translate website content from Simplified Chinese to {targetName}.
            Return only the translated content. Do not wrap the answer in Markdown code fences.
            Preserve Markdown syntax, HTML tags, image URLs, links, code blocks, tables, YAML fences, JSON property names, slugs, dates, booleans, numbers, and identifiers.
            """;

        return kind switch
        {
            ContentTranslationKind.MarkdownArticle => basePrompt + """

                For article YAML front matter, translate only human-readable title and description values.
                Also translate human-readable categories, albums, and tags list values.
                Keep slug, date, lastmod, cover, banner, author, draft, originalTitle, originalLink, and copyright values unchanged.
                Translate the article body naturally for technical readers.
                """,
            ContentTranslationKind.ArticleMetadata => basePrompt + """

                Keep the metadata payload valid and preserve its property names.
                Translate only article title, description, categories, albums, and tags values.
                Do not translate slugs, dates, booleans, URLs, IDs, field names, or any other identifiers.
                """,
            ContentTranslationKind.JsonResource => basePrompt + """

                Keep the JSON valid and preserve its original formatting as much as possible.
                Translate only human-readable display strings. Do not translate keys, slugs, URLs, IDs, icon names, CSS classes, file paths, route paths, or search keywords.
                """,
            _ => basePrompt + """

                Translate human-readable prose naturally for technical readers.
                """
        };
    }

    private static int EstimateMaxOutputTokens(string source)
    {
        var estimated = source.Length * 2;
        return Math.Clamp(estimated, 2048, 24000);
    }

    private static string NormalizeResponse(string response)
    {
        var trimmed = response.Trim();
        var match = FencedResponseRegex.Match(trimmed);
        return match.Success
            ? match.Groups["content"].Value.Trim()
            : trimmed;
    }
}
