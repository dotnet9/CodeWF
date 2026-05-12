using CodeWF.Api.Options;

namespace CodeWF.Api.Services;

public static class AdminGuard
{
    public static bool IsAuthorized(HttpContext context, AdminOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return true;
        }

        if (context.Request.Headers.TryGetValue("X-CodeWF-Admin-Key", out var value)
            && string.Equals(value.ToString(), options.ApiKey, StringComparison.Ordinal))
        {
            return true;
        }

        var authorization = context.Request.Headers.Authorization.ToString();
        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
               && string.Equals(authorization["Bearer ".Length..].Trim(), options.ApiKey, StringComparison.Ordinal);
    }
}
