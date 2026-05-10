using CodeWfLogger = CodeWF.Log.Core.Logger;
using WebApp.Options;
using WebApp.Services;

namespace WebApp.Extensions;

public static class ContentTranslationServiceCollectionExtensions
{
    public static IServiceCollection AddContentTranslation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 翻译服务按策略模式注册：业务只依赖 IContentTranslationService，具体 provider 由 FanyiUse 配置选择。
        services.Configure<OpenAIOption>(configuration.GetSection("OpenAI"));
        services.Configure<BaiduFanyiOption>(configuration.GetSection("BaiduFanyi"));
        services.Configure<TencentFanyiOption>(configuration.GetSection("TencentFanyi"));

        services.AddSingleton<OpenAiContentTranslationService>();
        services.AddSingleton<BaiduFanyiContentTranslationService>();
        services.AddSingleton<TencentFanyiContentTranslationService>();
        services.AddSingleton<IContentTranslationService>(provider =>
            SelectContentTranslationService(provider, configuration["FanyiUse"]));

        return services;
    }

    private static IContentTranslationService SelectContentTranslationService(
        IServiceProvider provider,
        string? fanyiUse)
    {
        if (string.Equals(fanyiUse, "OpenAI", StringComparison.OrdinalIgnoreCase))
        {
            CodeWfLogger.Info(
                "Content translation provider selected. fanyiUse=OpenAI; provider=OpenAI.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return provider.GetRequiredService<OpenAiContentTranslationService>();
        }

        if (string.Equals(fanyiUse, "BaiduFanyi", StringComparison.OrdinalIgnoreCase))
        {
            CodeWfLogger.Info(
                "Content translation provider selected. fanyiUse=BaiduFanyi; provider=BaiduFanyi.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return provider.GetRequiredService<BaiduFanyiContentTranslationService>();
        }

        if (string.Equals(fanyiUse, "TencentFanyi", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fanyiUse, "Tencent", StringComparison.OrdinalIgnoreCase))
        {
            CodeWfLogger.Info(
                $"Content translation provider selected. fanyiUse={fanyiUse}; provider=TencentFanyi.",
                log2UI: false,
                log2File: false,
                log2Console: true);
            return provider.GetRequiredService<TencentFanyiContentTranslationService>();
        }

        CodeWfLogger.Warn(
            $"Content translation provider disabled or unknown. fanyiUse={fanyiUse ?? "(null)"}; provider=Null.",
            log2UI: false,
            log2File: false,
            log2Console: true);
        return NullContentTranslationService.Instance;
    }
}
