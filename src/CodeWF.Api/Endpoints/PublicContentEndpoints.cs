using CodeWF.Api.Models;
using CodeWF.Api.Options;
using CodeWF.Api.Services;

namespace CodeWF.Api.Endpoints;

public static class PublicContentEndpoints
{
    public static RouteGroupBuilder MapPublicContentEndpoints(this RouteGroupBuilder api)
    {
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

        return api;
    }

    public static WebApplication MapSyndicationEndpoints(this WebApplication app)
    {
        app.MapGet("/rss.xml", async (ContentRepository repository) =>
            Results.Text(await repository.GetRssAsync(), "application/rss+xml; charset=utf-8"));

        app.MapGet("/rss", async (ContentRepository repository) =>
            Results.Text(await repository.GetRssAsync(), "application/rss+xml; charset=utf-8"));

        app.MapGet("/sitemap.xml", async (ContentRepository repository) =>
            Results.Text(await repository.GetSitemapAsync(), "application/xml; charset=utf-8"));

        app.MapGet("/sitemap", async (ContentRepository repository) =>
            Results.Text(await repository.GetSitemapAsync(), "application/xml; charset=utf-8"));

        return app;
    }

    private static string RequestCulture(HttpRequest request)
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
}
