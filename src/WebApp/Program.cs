using WebApp.Options;
using WebApp.Services;
using WebApp.Extensions;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.WebEncoders;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.Net.Http.Headers;
using CodeWF.Log.Core;
using CodeWfLogger = CodeWF.Log.Core.Logger;

var builder = WebApplication.CreateBuilder(args);

CodeWfLogger.EnableConsoleOutput = true;
CodeWfLogger.Level = LogType.Debug;
CodeWfLogger.Info("CodeWF.Log.Core server console logging enabled.", log2UI: false, log2File: false, log2Console: true);

// Add services to the container.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    // SVG 属于文本资源，压缩后通常能明显降低文章封面和图标的传输体积。
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["image/svg+xml"]);
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.AddRazorPages(options =>
{
    // 保留更直觉的短路由，兼顾历史链接、用户记忆和爬虫抓取入口。
    options.Conventions.AddPageRoute("/Blog/Index", "/blog");
    options.Conventions.AddPageRoute("/Search", "/search");
    options.Conventions.AddPageRoute("/Doc/Index", "/doc");
    options.Conventions.AddPageRoute("/SiteMap", "/sitemap.xml");
});
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<AppService>();
builder.Services.AddSingleton<I18nService>();
builder.Services.AddSingleton<LanguagePreparationService>();
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy
        // 页面 HTML 做短期服务端缓存；搜索、API、语言预热等请求仍然实时执行，避免吞掉副作用。
        .With(context => ShouldCachePageResponse(context.HttpContext))
        .Expire(TimeSpan.FromMinutes(10))
        .SetVaryByQuery("*")
        // 语言中间件会把 /en、/zh-cn 等路径改写为真实 Razor 路径，缓存键必须额外按语言拆开。
        .VaryByValue(context => new KeyValuePair<string, string>("language", GetOutputCacheLanguage(context))));
});
builder.Services.Configure<SiteOption>(builder.Configuration.GetSection("Site"));
builder.Services.AddContentTranslation(builder.Configuration);
// 站点正文和配置里包含大量中文，统一放开编码范围，避免输出时被过度转义。
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

var app = builder.Build();

// 启动时预热内容缓存，避免第一批请求承担 Markdown/JSON 的解析成本。
using (var serviceScope = app.Services.CreateScope())
{
    var service = serviceScope.ServiceProvider.GetRequiredService<AppService>();
    await service.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseMiddleware<RequestLanguageMiddleware>();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        if (app.Environment.IsDevelopment())
        {
            return;
        }

        var extension = Path.GetExtension(context.File.Name);
        if (string.IsNullOrWhiteSpace(extension))
        {
            return;
        }

        var cacheableExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".css",
            ".js",
            ".png",
            ".jpg",
            ".jpeg",
            ".webp",
            ".svg",
            ".ico",
            ".woff",
            ".woff2",
            ".json",
            ".txt"
        };

        if (!cacheableExtensions.Contains(extension))
        {
            return;
        }

        // 版本化静态资源允许较长缓存时间，能显著改善二次访问体验。
        context.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=604800";
    }
});
app.UseRouting();
app.UseOutputCache();

app.UseAuthorization();

app.MapGet("/logo.svg", (HttpContext context) => ServeLogoFile(context, app.Environment, "logo.svg", "image/svg+xml"));
app.MapGet("/logo.png", (HttpContext context) => ServeLogoFile(context, app.Environment, "logo.png", "image/png"));
app.MapGet("/logo.ico", (HttpContext context) => ServeLogoFile(context, app.Environment, "logo.ico", "image/x-icon"));
app.MapGet("/favicon.ico", () => Results.Redirect("/logo.ico"));
app.MapGet("/favicon.png", () => Results.Redirect("/logo.png"));

app.MapRazorPages();
app.MapControllers();

app.Run();

static bool ShouldCachePageResponse(HttpContext context)
{
    if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
    {
        return false;
    }

    if (context.Request.Headers.ContainsKey("X-CodeWF-Language-Prepare"))
    {
        return false;
    }

    var path = context.Request.Path;
    if (path.StartsWithSegments("/api")
        || path.StartsWithSegments("/search"))
    {
        return false;
    }

    return !Path.HasExtension(path);
}

static string GetOutputCacheLanguage(HttpContext context)
{
    if (context.Items.TryGetValue(RequestLanguageMiddleware.LanguageItemKey, out var item)
        && item is string language
        && RequestLanguage.Normalize(language) is { } normalized)
    {
        return normalized;
    }

    return RequestLanguage.CurrentLanguage;
}

static IResult ServeLogoFile(HttpContext context, IWebHostEnvironment environment, string fileName, string contentType)
{
    var filePath = ResolveLogoFile(environment, fileName);
    if (!File.Exists(filePath))
    {
        return Results.NotFound();
    }

    if (!environment.IsDevelopment())
    {
        context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=604800";
    }

    return Results.File(filePath, contentType);
}

static string ResolveLogoFile(IWebHostEnvironment environment, string fileName)
{
    var contentRootFile = Path.Combine(environment.ContentRootPath, fileName);
    if (File.Exists(contentRootFile))
    {
        return contentRootFile;
    }

    return Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", fileName));
}
