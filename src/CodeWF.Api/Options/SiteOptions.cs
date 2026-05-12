namespace CodeWF.Api.Options;

public sealed class SiteOptions
{
    public const string SectionName = "Site";
    public const string DefaultCultureName = "zh-CN";

    public string AppTitle { get; set; } = "CodeWF";
    public string Domain { get; set; } = "https://dotnet9.com";
    public string Memo { get; set; } = "Articles and tools";
    public string Owner { get; set; } = "dotnet9";
    public string? OwnerDesc { get; set; }
    public string? Favicon { get; set; }
    public string LocalAssetsDir { get; set; } = @"D:\wwwroot\img1.dotnet9.com";
    public string AssetBaseUrl { get; set; } = "https://img1.dotnet9.com";
    public string? RemoteAssetsRepository { get; set; }
    public int StartYear { get; set; } = 2019;
    public string? BaiAn { get; set; }
    public string? WeChatName { get; set; }
    public string? WeChatImg { get; set; }
    public string DefaultCulture { get; set; } = DefaultCultureName;
    public List<string> SupportedCultures { get; set; } = ["zh-CN", "en", "ja", "zh-TW"];
}

public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    public string? ApiKey { get; set; }
}

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public List<string> AllowedOrigins { get; set; } = [];
}
