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

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
