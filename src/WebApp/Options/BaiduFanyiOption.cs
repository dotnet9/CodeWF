namespace WebApp.Options;

public class BaiduFanyiOption
{
    public const int DefaultNetworkTimeoutSeconds = 60;
    public const int DefaultMaxRetries = 1;
    public const int DefaultMaxCharsPerRequest = 2000;

    public string? AppID { get; set; }
    public string? Key { get; set; }
    public string? Endpoint { get; set; } = "https://fanyi-api.baidu.com/api/trans/vip/translate";
    public int? NetworkTimeoutSeconds { get; set; } = DefaultNetworkTimeoutSeconds;
    public int? MaxRetries { get; set; } = DefaultMaxRetries;
    public int? MaxCharsPerRequest { get; set; } = DefaultMaxCharsPerRequest;
}
