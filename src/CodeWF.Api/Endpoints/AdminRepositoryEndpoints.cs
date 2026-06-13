using CodeWF.Api.Models;
using CodeWF.Api.Options;
using CodeWF.Api.Services;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Endpoints;

public static class AdminRepositoryEndpoints
{
    public static RouteGroupBuilder MapAdminRepositoryEndpoints(this RouteGroupBuilder admin)
    {
        admin.MapGet("/repository/status", async (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            GitRepositoryService git) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return RequireSuperAdmin();
            }

            return Results.Ok(await git.GetStatusAsync());
        });

        admin.MapGet("/repository/status/details", async (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            GitRepositoryService git) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return RequireSuperAdmin();
            }

            return Results.Ok(await git.GetStatusDetailsAsync());
        });

        admin.MapGet("/repository/log", async (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            GitRepositoryService git,
            int count = 20) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return RequireSuperAdmin();
            }

            return Results.Ok(await git.GetLogAsync(count));
        });

        admin.MapGet("/repository/file", async (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            GitRepositoryService git,
            string path) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return RequireSuperAdmin();
            }

            return Results.Ok(await git.GetFilePreviewAsync(path));
        });

        admin.MapPost("/repository/fetch", async (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            GitRepositoryService git) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return RequireSuperAdmin();
            }

            return Results.Ok(await git.FetchAsync());
        });

        admin.MapPost("/repository/pull", async (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            GitRepositoryService git) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return RequireSuperAdmin();
            }

            return Results.Ok(await git.PullAsync());
        });

        admin.MapPost("/repository/commit", async (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            GitRepositoryService git,
            GitCommitRequest request) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return RequireSuperAdmin();
            }

            return Results.Ok(await git.CommitAsync(request.Message));
        });

        return admin;
    }

    private static IResult RequireSuperAdmin() =>
        Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
}
