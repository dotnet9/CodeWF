namespace WebApp.Services;

public sealed class NullContentTranslationService : IContentTranslationService
{
    public static readonly NullContentTranslationService Instance = new();

    private NullContentTranslationService()
    {
    }

    public Task<string?> TranslateAsync(
        string source,
        LanguageInfo targetLanguage,
        ContentTranslationKind kind,
        string? resourceName = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);
}
