using System.Text.Json.Serialization;
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

var api = app.MapGroup("/api");

api.MapGet("/health", () => Results.Ok(new { status = "ok", at = DateTimeOffset.UtcNow }));

api.MapGet("/site", (ContentRepository repository) => Results.Ok(repository.GetSiteInfo()));

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

var admin = api.MapGroup("/admin");

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

public partial class Program;
