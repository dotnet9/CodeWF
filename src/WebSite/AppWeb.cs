using CodeWF;
using Known.Core;

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