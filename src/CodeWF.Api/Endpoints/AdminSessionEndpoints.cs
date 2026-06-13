using CodeWF.Api.Models;
using CodeWF.Api.Options;
using CodeWF.Api.Services;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Endpoints;

public static class AdminSessionEndpoints
{
    public static RouteGroupBuilder MapAdminSessionEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/admin/session", (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            AdminSessionService session) =>
        {
            var role = AdminGuard.GetRole(context, adminOptions.Value, session);
            return role is null
                ? Results.Unauthorized()
                : Results.Ok(ToSessionResponse(role));
        });

        api.MapPost("/admin/session", (
            HttpContext context,
            IHostEnvironment env,
            AdminSessionService session,
            AdminLoginRequest request) =>
        {
            var issued = session.TryIssue(request.UserName, request.Password);
            if (issued is null)
            {
                return Results.Unauthorized();
            }

            context.Response.Cookies.Append(
                AdminSessionService.CookieName,
                issued.Token,
                AdminSessionService.BuildCookieOptions(env, issued.ExpiresAt));

            return Results.Ok(ToSessionResponse(issued.Role));
        });

        api.MapDelete("/admin/session", (HttpContext context, IHostEnvironment env) =>
        {
            context.Response.Cookies.Delete(AdminSessionService.CookieName, AdminSessionService.BuildCookieOptions(env));
            return Results.NoContent();
        });

        return api;
    }

    private static object ToSessionResponse(string role) =>
        new { role, canWrite = string.Equals(role, "super-admin", StringComparison.Ordinal) };
}
