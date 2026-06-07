using CodeWF.Api.Options;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Services;

public sealed class AdminAuthorizationEndpointFilter(IOptions<AdminOptions> adminOptions) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        if (!AdminGuard.IsAuthorized(httpContext, adminOptions.Value))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
