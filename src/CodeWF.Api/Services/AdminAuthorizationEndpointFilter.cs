using CodeWF.Api.Options;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Services;

public sealed class AdminAuthorizationEndpointFilter(
    IOptions<AdminOptions> adminOptions,
    AdminSessionService session) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        if (AdminGuard.GetRole(httpContext, adminOptions.Value, session) is null)
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
