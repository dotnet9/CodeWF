using CodeWF.Web.Components;
using CodeWF.Web.Options;
using CodeWF.Web.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.AddMemoryCache();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<BlogPostFileService>();
builder.Services.AddSingleton<ContentRepository>();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(10)));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseStaticFiles();
app.UseAntiforgery();
app.UseStatusCodePages();
app.UseOutputCache();

// 内容索引在启动时后台预热，避免首个请求同步扫全库
_ = Task.Run(async () =>
{
    try
    {
        var scope = app.Services;
        await scope.GetRequiredService<ContentRepository>().GetHomeAsync(
            scope.GetRequiredService<IOptionsMonitor<SiteOptions>>().CurrentValue.DefaultCulture, 6);
    }
    catch
    {
        // 预热失败不阻塞启动；首个请求会再触发加载
    }
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/site/favicon/logo.ico", (IOptionsMonitor<SiteOptions> siteOptions) =>
{
    var root = Environment.ExpandEnvironmentVariables(siteOptions.CurrentValue.LocalAssetsDir);
    var path = Path.GetFullPath(Path.Combine(root, "site", "favicon", "logo.ico"));
    return File.Exists(path) ? Results.File(path, "image/x-icon") : Results.NotFound();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", at = DateTimeOffset.UtcNow }));

app.MapGet("/rss.xml", async (ContentRepository repository) =>
    Results.Text(await repository.GetRssAsync(), "application/rss+xml; charset=utf-8"));
app.MapGet("/rss", async (ContentRepository repository) =>
    Results.Text(await repository.GetRssAsync(), "application/rss+xml; charset=utf-8"));
app.MapGet("/sitemap.xml", async (ContentRepository repository) =>
    Results.Text(await repository.GetSitemapAsync(), "application/xml; charset=utf-8"));
app.MapGet("/sitemap", async (ContentRepository repository) =>
    Results.Text(await repository.GetSitemapAsync(), "application/xml; charset=utf-8"));
app.MapGet("/robots.txt", () => Results.Text(
    "User-agent: *\nAllow: /\nDisallow: /admin\nSitemap: https://dotnet9.com/sitemap.xml", "text/plain; charset=utf-8"));

app.Run();

public partial class Program;
