using CodeWF.Api.Options;

namespace CodeWF.Api.Services;

public static class AdminGuard
{
    public static bool IsAuthorized(HttpContext context, AdminOptions options)
        => GetRole(context, options) is not null;

    public static bool IsSuperAdmin(HttpContext context, AdminOptions options)
        => string.Equals(GetRole(context, options), "super-admin", StringComparison.Ordinal);

    public static string? GetRole(HttpContext context, AdminOptions options, AdminSessionService? session = null)
    {
        session ??= context.RequestServices.GetService<AdminSessionService>();
        var cookieRole = session?.GetRole(context);
        if (cookieRole is not null)
        {
            return cookieRole;
        }

        return TryReadCredentials(context, out var userName, out var password)
            ? GetRole(options, userName, password)
            : null;
    }

    public static string? GetRole(AdminOptions options, string userName, string password)
    {
        if (Matches(options.Super, userName, password))
        {
            return "super-admin";
        }

        return Matches(options.Read, userName, password) ? "reader" : null;
    }

    private static bool Matches(IEnumerable<AdminAccountOptions> accounts, string userName, string password) =>
        accounts.Any(account =>
            string.Equals(userName, account.UserName, StringComparison.Ordinal)
            && string.Equals(password, account.Password, StringComparison.Ordinal));

    public static bool HasUserWithRole(AdminOptions options, string userName, string role) =>
        string.Equals(role, "super-admin", StringComparison.Ordinal)
            ? options.Super.Any(account => string.Equals(account.UserName, userName, StringComparison.Ordinal))
            : string.Equals(role, "reader", StringComparison.Ordinal)
              && options.Read.Any(account => string.Equals(account.UserName, userName, StringComparison.Ordinal));

    public static bool HasUnsafeDefaultCredentials(AdminOptions options) =>
        options.Read.Any(account => MatchesDevelopmentDefault(account, "codewf", "codewf.com"))
        || options.Super.Any(account => MatchesDevelopmentDefault(account, "admin", "111111"))
        || options.Super.Any(account => MatchesDevelopmentDefault(account, "admin", "change-me"));

    private static bool MatchesDevelopmentDefault(AdminAccountOptions account, string userName, string password) =>
        string.Equals(account.UserName, userName, StringComparison.Ordinal)
        && string.Equals(account.Password, password, StringComparison.Ordinal);

    public static IResult RequireSuperAdmin(HttpContext context, AdminOptions options)
    {
        return IsSuperAdmin(context, options)
            ? Results.Ok()
            : Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
    }

    public static bool TryReadCredentials(HttpContext context, out string userName, out string password)
    {
        userName = string.Empty;
        password = string.Empty;

        if (context.Request.Headers.TryGetValue("X-CodeWF-Admin-User", out var user)
            && context.Request.Headers.TryGetValue("X-CodeWF-Admin-Password", out var pass))
        {
            userName = user.ToString();
            password = pass.ToString();
            return true;
        }

        var authorization = context.Request.Headers.Authorization.ToString();
        if (authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var raw = Convert.FromBase64String(authorization["Basic ".Length..].Trim());
                var decoded = System.Text.Encoding.UTF8.GetString(raw);
                var separator = decoded.IndexOf(':');
                if (separator >= 0)
                {
                    userName = decoded[..separator];
                    password = decoded[(separator + 1)..];
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        return false;
    }
}
