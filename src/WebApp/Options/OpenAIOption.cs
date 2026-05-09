namespace WebApp.Options;

public class OpenAIOption
{
    public const int DefaultNetworkTimeoutSeconds = 300;
    public const int DefaultMaxRetries = 1;

    public string? Key { get; set; }
    public string? Endpoint { get; set; }
    public string? ChatModel { get; set; }
    public int? NetworkTimeoutSeconds { get; set; } = DefaultNetworkTimeoutSeconds;
    public int? MaxRetries { get; set; } = DefaultMaxRetries;
}
