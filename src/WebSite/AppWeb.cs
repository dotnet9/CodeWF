using CodeWF;
using Known.Core;

namespace WebSite;

static class AppWeb
{
    public static void AddApp(this WebApplicationBuilder builder)
    {
        builder.Services.AddCodeWF();
        builder.Services.AddKnownCore(info =>
        {
            info.WebRoot = builder.Environment.WebRootPath;
            info.ContentRoot = builder.Environment.ContentRootPath;
        });
        builder.Services.AddKnownWeb();
    }

    public static void UseApp(this WebApplication app)
    {
        app.UseKnown();
    }
}