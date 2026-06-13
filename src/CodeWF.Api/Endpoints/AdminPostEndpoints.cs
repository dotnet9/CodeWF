using CodeWF.Api.Models;
using CodeWF.Api.Options;
using CodeWF.Api.Services;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Endpoints;

public static class AdminPostEndpoints
{
    public static RouteGroupBuilder MapAdminPostEndpoints(this RouteGroupBuilder admin)
    {
        admin.MapGet("/posts", async (
            ContentRepository repository,
            int pageIndex = 1,
            int pageSize = 20,
            string? keyword = null) =>
            Results.Ok(await repository.GetPagedPostsAsync(SiteOptions.DefaultCultureName, pageIndex, pageSize, keyword, includeDrafts: true)));

        admin.MapGet("/posts/{slug}", async (
            ContentRepository repository,
            string slug) =>
        {
            var post = await repository.GetPostAsync(SiteOptions.DefaultCultureName, slug, includeDrafts: true);
            return post is null ? Results.NotFound() : Results.Ok(post);
        });

        admin.MapPost("/posts", async (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            ContentRepository repository,
            AdminPostRequest request) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
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
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
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
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
            }

            var deleted = await repository.DeletePostAsync(slug);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return admin;
    }
}
