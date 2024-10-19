using CodeWF.EntityFramework;
using CodeWF.Options;

namespace WebSite;

static class AppWeb
{
    internal const string AuthType = "Known_Cookie";

    public static void AddApp(this WebApplicationBuilder builder)
    {
        foreach (var item in Language.Items)
        {
            item.Visible = item.Id != "en-US" && item.Id != "vi-VN";
        }

        AppConfig.AppTitle = "Known开源企业级开发框架";
        AIHelper.Tags = new()
        {
            ["前端"] = ["前端", "界面", "Blazor", "UI"],
            ["后端"] = ["后端", "服务", "数据依赖", "Service", "Repository"],
            ["数据库"] = ["数据库", "Database"],
            ["需求"] = ["需求"],
            ["设计"] = ["设计"],
            ["建议"] = ["建议"],
            ["BUG"] = ["BUG", "Exception", "异常"]
        }; 

        //ModuleHelper.InitAppModules();
        //Stopwatcher.Enabled = true;
        builder.Services.AddAuthentication().AddCookie(AuthType);
        builder.Services.AddCodeWF();
        builder.Services.AddKnownCore(info =>
        {
            info.WebRoot = builder.Environment.WebRootPath;
            info.ContentRoot = builder.Environment.ContentRootPath;
            info.Connections = [new Known.ConnectionInfo
            {
                Name = "Default",
                DatabaseType = DatabaseType.SQLite,
                ProviderType = typeof(Microsoft.Data.Sqlite.SqliteFactory),
                ConnectionString = builder.Configuration.GetSection("ConnString").Get<string>()
            }];
        });
        builder.Services.AddKnownWeb();
        builder.Services.AddKnownWeixin(builder.Configuration);
        // 如下为EFCore配置，若开启，请注释上面框架自带的连接配置
        //builder.Services.AddCodeWFEntityFramework(config =>
        //{
        //    config.ConnString = builder.Configuration.GetSection("ConnString").Get<string>();
        //});
    }

    public static void UseApp(this WebApplication app)
    {
        app.UseKnown();
        app.UseKnownWeixin();
    }
}