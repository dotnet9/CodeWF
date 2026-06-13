using CodeWF.Api.Models;
using CodeWF.Api.Options;
using CodeWF.Api.Services;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Endpoints;

public static class AdminContentEndpoints
{
    public static RouteGroupBuilder MapAdminContentEndpoints(this RouteGroupBuilder admin)
    {
        admin.MapGet("/content/markdown/{name}", async (
            HttpContext context,
            ContentRepository repository,
            string name,
            string? culture = null) =>
        {
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
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
            }

            var (resourceName, path) = ResolveMarkdownResource(name);
            return Results.Ok(await repository.SaveMarkdownResourceAsync(culture ?? RequestCulture(context.Request), resourceName, request.Content, path));
        });

        admin.MapGet("/content/json/{name}", async (
            HttpContext context,
            ContentRepository repository,
            string name,
            string? culture = null) =>
        {
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
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
            }

            var (resourceName, path) = ResolveJsonResource(name);
            return Results.Ok(await repository.SaveJsonResourceAsync(culture ?? RequestCulture(context.Request), resourceName, request.Content, path));
        });

        admin.MapGet("/assets", async (ContentRepository repository, string? path = null) =>
            Results.Ok(await repository.GetAssetsAsync(path ?? string.Empty)));

        admin.MapGet("/assets/file", async (GitRepositoryService git, string path) =>
            Results.Ok(await git.GetFilePreviewAsync(path)));

        admin.MapPost("/assets", async (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            ContentRepository repository,
            string? path = null,
            string? name = null) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
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
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
            }

            var result = await repository.DeleteAssetAsync(path ?? string.Empty);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        });

        return admin;
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

    private static (string Name, string[] Segments) ResolveMarkdownResource(string name) =>
        name.ToLowerInvariant() switch
        {
            "about" => ("about", ["about.md"]),
            "donation" => ("donation", ["pays", "Donation.md"]),
            "privacy" => ("privacy", ["Privacy.md"]),
            _ => (name, [$"{name}.md"])
        };

    private static (string Name, string[] Segments) ResolveJsonResource(string name) =>
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
}
