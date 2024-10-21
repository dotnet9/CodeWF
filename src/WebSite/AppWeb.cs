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
        });
        builder.Services.AddKnownWeb();
        builder.Services.AddKnownWeixin(builder.Configuration);
    }

    public static void UseApp(this WebApplication app)
    {
        app.UseKnown();
        app.UseKnownWeixin();
    }
}