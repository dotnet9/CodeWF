namespace WebApp.Services;

public interface IContentTranslationService
{
    Task<string?> TranslateAsync(
        string source,
        LanguageInfo targetLanguage,
        ContentTranslationKind kind,
        string? resourceName = null,
        CancellationToken cancellationToken = default);
}
