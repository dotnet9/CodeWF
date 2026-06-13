using CodeWF.Api.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Services;

public sealed class AdminSessionService
{
    public const string CookieName = "CodeWF.Admin";
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

    private readonly IDataProtector protector;
    private readonly IOptions<AdminOptions> adminOptions;

    public AdminSessionService(IDataProtectionProvider dataProtection, IOptions<AdminOptions> adminOptions)
    {
        protector = dataProtection.CreateProtector("CodeWF.Admin.Session.v1");
        this.adminOptions = adminOptions;
    }

    public AdminSessionIssueResult? TryIssue(string userName, string password)
    {
        var role = AdminGuard.GetRole(adminOptions.Value, userName, password);
        if (role is null)
        {
            return null;
        }

        var expiresAt = DateTimeOffset.UtcNow.Add(SessionLifetime);
        var payload = string.Join('|', userName, role, expiresAt.ToUnixTimeSeconds());
        return new AdminSessionIssueResult(protector.Protect(payload), role, expiresAt);
    }

    public string? GetRole(HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue(CookieName, out var token) || string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var payload = protector.Unprotect(token);
            var parts = payload.Split('|');
            if (parts.Length != 3 || !long.TryParse(parts[2], out var expiresUnix))
            {
                return null;
            }

            if (DateTimeOffset.FromUnixTimeSeconds(expiresUnix) <= DateTimeOffset.UtcNow)
            {
                return null;
            }

            var userName = parts[0];
            var role = parts[1];
            return AdminGuard.HasUserWithRole(adminOptions.Value, userName, role) ? role : null;
        }
        catch
        {
            return null;
        }
    }

    public static CookieOptions BuildCookieOptions(IHostEnvironment environment, DateTimeOffset? expiresAt = null) =>
        new()
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt,
            MaxAge = expiresAt is null ? null : SessionLifetime,
            Path = "/api"
        };
}

public sealed record AdminSessionIssueResult(string Token, string Role, DateTimeOffset ExpiresAt);
