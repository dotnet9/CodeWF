using CodeWF.Api.Options;

namespace CodeWF.Api.Services;

public static class AdminGuard
{
    public static bool IsAuthorized(HttpContext context, AdminOptions options)
    {
        if (TryReadCredentials(context, out var userName, out var password))
        {
            return string.Equals(userName, options.UserName, StringComparison.Ordinal)
                   && string.Equals(password, options.Password, StringComparison.Ordinal);
        }

        return false;
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
