namespace WebApp.Options;

public class TencentFanyiOption
{
    public const string DefaultRegion = "ap-chengdu";
    public const string DefaultEndpoint = "tmt.tencentcloudapi.com";
    public const long DefaultProjectId = 1;
    public const int DefaultNetworkTimeoutSeconds = 60;
    public const int DefaultMaxRetries = 1;
    public const int DefaultMaxCharsPerRequest = 5999;
    public const int DefaultMaxRequestsPerSecond = 4;
    public const int DefaultMemoryCacheLimit = 10000;

    public string? AppID { get; set; }
    public string? Key { get; set; }
    public string? SecretId { get; set; }
    public string? SecretKey { get; set; }
    public string? Region { get; set; } = DefaultRegion;
    public string? Endpoint { get; set; } = DefaultEndpoint;
    public long? ProjectId { get; set; } = DefaultProjectId;
    public int? NetworkTimeoutSeconds { get; set; } = DefaultNetworkTimeoutSeconds;
    public int? MaxRetries { get; set; } = DefaultMaxRetries;
    public int? MaxCharsPerRequest { get; set; } = DefaultMaxCharsPerRequest;
    public int? MaxRequestsPerSecond { get; set; } = DefaultMaxRequestsPerSecond;
    public int? MemoryCacheLimit { get; set; } = DefaultMemoryCacheLimit;
}
