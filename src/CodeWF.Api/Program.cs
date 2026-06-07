using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using CodeWF.Api.Models;
using CodeWF.Api.Options;
using CodeWF.Api.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));
builder.Services.AddMemoryCache();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<BlogPostFileService>();
builder.Services.AddSingleton<ContentRepository>();
builder.Services.AddSingleton<GitRepositoryService>();
builder.Services.AddScoped<AdminAuthorizationEndpointFilter>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var cors = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        if (cors.AllowedOrigins.Count > 0)
        {
            policy.WithOrigins(cors.AllowedOrigins.ToArray());
        }
        else
        {
            policy.AllowAnyOrigin();
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();
app.UseCors("frontend");

app.MapGet("/site/favicon/logo.ico", (IOptionsMonitor<SiteOptions> siteOptions) =>
{
    var root = Environment.ExpandEnvironmentVariables(siteOptions.CurrentValue.LocalAssetsDir);
    var path = Path.GetFullPath(Path.Combine(root, "site", "favicon", "logo.ico"));
    if (!File.Exists(path))
    {
        return Results.NotFound();
    }

    return Results.File(path, "image/x-icon");
});

var api = app.MapGroup("/api");

api.MapGet("/health", () => Results.Ok(new { status = "ok", at = DateTimeOffset.UtcNow }));

api.MapGet("/site", (ContentRepository repository) => Results.Ok(repository.GetSiteInfo()));

api.MapGet("/site-settings", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(repository.GetSiteInfo());
});

var admin = api.MapGroup("/admin")
    .AddEndpointFilter<AdminAuthorizationEndpointFilter>();

admin.MapGet("/site-settings", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(repository.GetSiteInfo());
});

admin.MapPut("/site-settings", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    IHostEnvironment env,
    SiteSettingsRequest request) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var result = await UpdateSiteSettingsAsync(env.ContentRootPath, request, repository.GetSiteInfo());
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

api.MapPut("/site-settings", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    IHostEnvironment env,
    SiteSettingsRequest request) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var result = await UpdateSiteSettingsAsync(env.ContentRootPath, request, repository.GetSiteInfo());
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

api.MapGet("/localization", async (HttpRequest request, ContentRepository repository) =>
{
    var culture = RequestCulture(request);
    return Results.Ok(await repository.GetLocalizationAsync(culture));
});

api.MapGet("/home", async (HttpRequest request, ContentRepository repository, int recent = 8) =>
{
    var culture = RequestCulture(request);
    return Results.Ok(await repository.GetHomeAsync(culture, recent));
});

api.MapGet("/posts", async (
    HttpRequest request,
    ContentRepository repository,
    int pageIndex = 1,
    int pageSize = 12,
    string? keyword = null,
    string? category = null,
    string? album = null,
    string? tag = null) =>
{
    var culture = RequestCulture(request);
    return Results.Ok(await repository.GetPagedPostsAsync(culture, pageIndex, pageSize, keyword, category, album, tag));
});

api.MapGet("/posts/{year:int}/{month:int}/{slug}", async (
    HttpRequest request,
    ContentRepository repository,
    int year,
    int month,
    string slug) =>
{
    var culture = RequestCulture(request);
    var post = await repository.GetPostAsync(culture, slug, year, month);
    return post is null ? Results.NotFound() : Results.Ok(post);
});

api.MapGet("/posts/{slug}", async (HttpRequest request, ContentRepository repository, string slug) =>
{
    var culture = RequestCulture(request);
    var post = await repository.GetPostAsync(culture, slug);
    return post is null ? Results.NotFound() : Results.Ok(post);
});

api.MapGet("/categories", async (HttpRequest request, ContentRepository repository) =>
    Results.Ok(await repository.GetCategoriesAsync(RequestCulture(request))));

api.MapGet("/albums", async (HttpRequest request, ContentRepository repository) =>
    Results.Ok(await repository.GetAlbumsAsync(RequestCulture(request))));

api.MapGet("/tags", async (HttpRequest request, ContentRepository repository) =>
    Results.Ok(await repository.GetTagsAsync(RequestCulture(request))));

api.MapGet("/tools", async (HttpRequest request, ContentRepository repository) =>
    Results.Ok(await repository.GetToolsAsync(RequestCulture(request))));

api.MapGet("/tools/{slug}", async (HttpRequest request, ContentRepository repository, string slug) =>
{
    var tool = await repository.GetToolAsync(RequestCulture(request), slug);
    return tool is null ? Results.NotFound() : Results.Ok(tool);
});

api.MapGet("/docs", async (HttpRequest request, ContentRepository repository) =>
    Results.Ok(await repository.GetDocsAsync(RequestCulture(request))));

api.MapGet("/docs/{slug}", async (HttpRequest request, ContentRepository repository, string slug) =>
{
    var doc = await repository.GetDocAsync(RequestCulture(request), slug);
    return doc is null ? Results.NotFound() : Results.Ok(doc);
});

api.MapGet("/pages/about", async (HttpRequest request, ContentRepository repository) =>
    Results.Ok(await repository.ReadSitePageAsync(RequestCulture(request), "about.md")));

api.MapGet("/pages/donation", async (HttpRequest request, ContentRepository repository) =>
    Results.Ok(await repository.ReadSitePageAsync(RequestCulture(request), "pays", "Donation.md")));

api.MapGet("/pages/privacy", async (HttpRequest request, ContentRepository repository) =>
    Results.Ok(await repository.ReadSitePageAsync(RequestCulture(request), "Privacy.md")));

api.MapGet("/friend-links", async (HttpRequest request, ContentRepository repository) =>
    Results.Ok(await repository.GetFriendLinksAsync(RequestCulture(request))));

api.MapGet("/timelines", async (HttpRequest request, ContentRepository repository) =>
    Results.Ok(await repository.GetTimelinesAsync(RequestCulture(request))));

api.MapGet("/search", async (
    HttpRequest request,
    ContentRepository repository,
    string? q = null,
    int pageIndex = 1,
    int pageSize = 10,
    SearchResultKind? kind = null) =>
{
    var culture = RequestCulture(request);
    return Results.Ok(await repository.SearchAsync(culture, q, pageIndex, pageSize, kind));
});

api.MapGet("/search/suggest", async (
    HttpRequest request,
    ContentRepository repository,
    string? q = null,
    int take = 10) =>
{
    var culture = RequestCulture(request);
    return Results.Ok(await repository.GetSearchSuggestionsAsync(culture, q, take));
});

app.MapGet("/rss.xml", async (ContentRepository repository) =>
    Results.Text(await repository.GetRssAsync(), "application/rss+xml; charset=utf-8"));

app.MapGet("/rss", async (ContentRepository repository) =>
    Results.Text(await repository.GetRssAsync(), "application/rss+xml; charset=utf-8"));

app.MapGet("/sitemap.xml", async (ContentRepository repository) =>
    Results.Text(await repository.GetSitemapAsync(), "application/xml; charset=utf-8"));

app.MapGet("/sitemap", async (ContentRepository repository) =>
    Results.Text(await repository.GetSitemapAsync(), "application/xml; charset=utf-8"));

admin.MapGet("/posts", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    int pageIndex = 1,
    int pageSize = 20,
    string? keyword = null) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await repository.GetPagedPostsAsync(SiteOptions.DefaultCultureName, pageIndex, pageSize, keyword, includeDrafts: true));
});

admin.MapGet("/posts/{slug}", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    string slug) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var post = await repository.GetPostAsync(SiteOptions.DefaultCultureName, slug, includeDrafts: true);
    return post is null ? Results.NotFound() : Results.Ok(post);
});

admin.MapPost("/posts", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    AdminPostRequest request) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var result = await repository.UpsertPostAsync(request);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

admin.MapPut("/posts/{slug}", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    string slug,
    AdminPostRequest request) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var result = await repository.UpsertPostAsync(request, slug);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

admin.MapDelete("/posts/{slug}", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    string slug) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var deleted = await repository.DeletePostAsync(slug);
    return deleted ? Results.NoContent() : Results.NotFound();
});

admin.MapGet("/content/markdown/{name}", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    string name,
    string? culture = null) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var (resourceName, path) = ResolveMarkdownResource(name);
    var data = await repository.ReadMarkdownResourceAsync(culture ?? RequestCulture(context.Request), resourceName, path);
    return Results.Ok(data);
});

admin.MapPut("/content/markdown/{name}", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    string name,
    ContentSaveRequest request,
    string? culture = null) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var (resourceName, path) = ResolveMarkdownResource(name);
    return Results.Ok(await repository.SaveMarkdownResourceAsync(culture ?? RequestCulture(context.Request), resourceName, request.Content, path));
});

admin.MapGet("/content/json/{name}", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    string name,
    string? culture = null) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var (resourceName, path) = ResolveJsonResource(name);
    var data = await repository.ReadJsonResourceAsync(culture ?? RequestCulture(context.Request), resourceName, path);
    return Results.Ok(data);
});

admin.MapPut("/content/json/{name}", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    string name,
    ContentSaveRequest request,
    string? culture = null) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var (resourceName, path) = ResolveJsonResource(name);
    return Results.Ok(await repository.SaveJsonResourceAsync(culture ?? RequestCulture(context.Request), resourceName, request.Content, path));
});

admin.MapGet("/assets", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    string? path = null) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await repository.GetAssetsAsync(path ?? string.Empty));
});

admin.MapPost("/assets", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    string? path = null,
    string? name = null) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(name))
    {
        return Results.BadRequest(new AssetUploadResult(false, "File name is required."));
    }

    var result = await repository.SaveAssetAsync(path ?? string.Empty, name, context.Request.Body);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

admin.MapDelete("/assets", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    ContentRepository repository,
    string? path = null) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    var result = await repository.DeleteAssetAsync(path ?? string.Empty);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

admin.MapGet("/repository/status", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    GitRepositoryService git) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await git.GetStatusAsync());
});

admin.MapGet("/repository/status/details", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    GitRepositoryService git) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await git.GetStatusDetailsAsync());
});

admin.MapGet("/repository/log", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    GitRepositoryService git,
    int count = 20) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await git.GetLogAsync(count));
});

admin.MapGet("/repository/file", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    GitRepositoryService git,
    string path) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await git.GetFilePreviewAsync(path));
});

admin.MapPost("/repository/fetch", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    GitRepositoryService git) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await git.FetchAsync());
});

admin.MapPost("/repository/pull", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    GitRepositoryService git) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await git.PullAsync());
});

admin.MapPost("/repository/commit", async (
    HttpContext context,
    IOptions<AdminOptions> adminOptions,
    GitRepositoryService git,
    GitCommitRequest request) =>
{
    if (!AdminGuard.IsAuthorized(context, adminOptions.Value))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await git.CommitAsync(request.Message));
});

app.Run();

static string RequestCulture(HttpRequest request)
{
    if (request.Query.TryGetValue("culture", out var queryCulture) && !string.IsNullOrWhiteSpace(queryCulture))
    {
        return queryCulture.ToString();
    }

    if (request.Headers.TryGetValue("X-CodeWF-Culture", out var headerCulture) && !string.IsNullOrWhiteSpace(headerCulture))
    {
        return headerCulture.ToString();
    }

    return request.Headers.AcceptLanguage.FirstOrDefault()?.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0]
           ?? SiteOptions.DefaultCultureName;
}

static (string Name, string[] Segments) ResolveMarkdownResource(string name) =>
    name.ToLowerInvariant() switch
    {
        "about" => ("about", ["about.md"]),
        "donation" => ("donation", ["pays", "Donation.md"]),
        "privacy" => ("privacy", ["Privacy.md"]),
        _ => (name, [$"{name}.md"])
    };

static (string Name, string[] Segments) ResolveJsonResource(string name) =>
    name.ToLowerInvariant() switch
    {
        "tools" => ("tools", ["tools", "tools.json"]),
        "navigation" or "doc-navigation" => ("navigation", ["doc", "navigation.json"]),
        "friend-links" => ("friend-links", ["friend-links.json"]),
        "timelines" => ("timelines", ["timelines.json"]),
        "blocked-search-keywords" => ("blocked-search-keywords", ["blocked-search-keywords.json"]),
        "lang" => ("lang", ["lang.json"]),
        "categories" => ("categories", ["categories.json"]),
        "albums" => ("albums", ["albums.json"]),
        _ => (name, [$"{name}.json"])
    };

static async Task<SiteSettingsResult> UpdateSiteSettingsAsync(string contentRootPath, SiteSettingsRequest request, SiteInfo current)
{
    var appsettingsPath = Path.Combine(contentRootPath, "appsettings.json");
    if (!File.Exists(appsettingsPath))
    {
        return new SiteSettingsResult(false, $"appsettings.json not found: {appsettingsPath}");
    }

    JsonNode? root;
    try
    {
        root = JsonNode.Parse(await File.ReadAllTextAsync(appsettingsPath));
    }
    catch (Exception ex)
    {
        return new SiteSettingsResult(false, $"Unable to read appsettings.json: {ex.Message}");
    }

    if (root is not JsonObject rootObject)
    {
        return new SiteSettingsResult(false, "appsettings.json is not a JSON object.");
    }

    var site = new JsonObject
    {
        ["AppTitle"] = request.AppTitle ?? current.AppTitle,
        ["Domain"] = request.Domain ?? current.Domain,
        ["Memo"] = request.Memo ?? current.Memo,
        ["Owner"] = request.Owner ?? current.Owner,
        ["OwnerDesc"] = request.OwnerDesc ?? current.OwnerDesc,
        ["Favicon"] = request.Favicon ?? current.Favicon,
        ["LocalAssetsDir"] = request.LocalAssetsDir ?? current.LocalAssetsDir,
        ["AssetBaseUrl"] = request.AssetBaseUrl ?? current.AssetBaseUrl,
        ["RemoteAssetsRepository"] = request.RemoteAssetsRepository ?? current.RemoteAssetsRepository,
        ["StartYear"] = request.StartYear == 0 ? current.StartYear : request.StartYear,
        ["BaiAn"] = request.BaiAn ?? current.BaiAn,
        ["WeChatName"] = request.WeChatName ?? current.WeChatName,
        ["WeChatImg"] = request.WeChatImg ?? current.WeChatImg,
        ["DefaultCulture"] = request.DefaultCulture ?? current.DefaultCulture,
        ["SupportedCultures"] = JsonValue.Create(request.SupportedCultures is { Count: > 0 } ? request.SupportedCultures : current.SupportedCultures.ToList())
    };

    rootObject["Site"] = site;

    try
    {
        await File.WriteAllTextAsync(appsettingsPath, rootObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
        return new SiteSettingsResult(true, "Site settings saved.", new SiteInfo(
            request.AppTitle ?? current.AppTitle,
            request.Domain ?? current.Domain,
            request.Memo ?? current.Memo,
            request.Owner ?? current.Owner,
            request.OwnerDesc ?? current.OwnerDesc,
            request.Favicon ?? current.Favicon,
            request.LocalAssetsDir ?? current.LocalAssetsDir,
            request.AssetBaseUrl ?? current.AssetBaseUrl,
            request.RemoteAssetsRepository ?? current.RemoteAssetsRepository,
            request.StartYear == 0 ? current.StartYear : request.StartYear,
            request.DefaultCulture ?? current.DefaultCulture,
            request.SupportedCultures is { Count: > 0 } ? request.SupportedCultures : current.SupportedCultures.ToList(),
            request.BaiAn ?? current.BaiAn,
            request.WeChatName ?? current.WeChatName,
            request.WeChatImg ?? current.WeChatImg));
    }
    catch (Exception ex)
    {
        return new SiteSettingsResult(false, $"Unable to write appsettings.json: {ex.Message}");
    }
}

public partial class Program;
