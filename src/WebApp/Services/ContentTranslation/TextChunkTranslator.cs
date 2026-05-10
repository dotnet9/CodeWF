namespace WebApp.Services;

internal delegate Task<string?> TextChunkTranslator(
    string source,
    LanguageInfo targetLanguage,
    string resource,
    int chunkIndex,
    int chunkCount,
    CancellationToken cancellationToken);
