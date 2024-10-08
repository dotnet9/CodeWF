using Coravel;
using Coravel.Invocable;

namespace WebAdmin;

public static class AppWeb
{
    public static void AddApp(this IServiceCollection services, Action<AppInfo> action = null)
    {
        services.AddCodeWFAdmin();
        services.AddKnownCore(info =>
        {
            //info.ProductId = "Test";
            //info.CheckSystem = info => Result.Error("无效密钥，请重新授权！");
            //info.SqlMonitor = c => Console.WriteLine($"{DateTime.Now:HH:mm:ss} {c}");
            action?.Invoke(info);
        });
        services.AddKnownCells();
        services.AddKnownWeb();

        //添加模块
        Config.AddModule(typeof(AppWeb).Assembly, false);

        //添加定时任务
        services.AddScheduler();
        services.AddTransient<ImportTaskJob>();
    }

    public static void UseApp(this WebApplication app)
    {
        //使用Known框架
        app.UseKnown();
        //配置定时任务
        app.Services.UseScheduler(scheduler =>
        {
            //每5秒执行一次异步导入
            scheduler.Schedule<ImportTaskJob>().EveryFiveSeconds();
        });
    }
}

class ImportTaskJob : IInvocable
{
    public Task Invoke() => ImportHelper.ExecuteAsync();
}