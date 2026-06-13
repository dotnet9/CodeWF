using CodeWF.Api.Options;
using CodeWF.Api.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace CodeWF.Api.Tests;

public sealed class AdminSecurityTests
{
    [Fact]
    public void HasUnsafeDefaultCredentials_DetectsDevelopmentDefaults()
    {
        var options = new AdminOptions
        {
            Read = [new AdminAccountOptions { UserName = "codewf", Password = "codewf.com" }],
            Super = [new AdminAccountOptions { UserName = "admin", Password = "change-me" }]
        };

        Assert.True(AdminGuard.HasUnsafeDefaultCredentials(options));
    }

    [Fact]
    public void HasUnsafeDefaultCredentials_AllowsCustomSecrets()
    {
        var options = new AdminOptions
        {
            Read = [new AdminAccountOptions { UserName = "reader", Password = "reader-secret" }],
            Super = [new AdminAccountOptions { UserName = "owner", Password = "owner-secret" }]
        };

        Assert.False(AdminGuard.HasUnsafeDefaultCredentials(options));
    }

    [Fact]
    public void AdminSessionService_IssuesAndReadsRoleFromCookie()
    {
        var options = new AdminOptions
        {
            Read = [new AdminAccountOptions { UserName = "reader", Password = "reader-secret" }],
            Super = [new AdminAccountOptions { UserName = "owner", Password = "owner-secret" }]
        };
        var service = CreateSessionService(options);
        var issued = service.TryIssue("owner", "owner-secret");
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{AdminSessionService.CookieName}={issued!.Token}";

        Assert.NotNull(issued);
        Assert.Equal("super-admin", service.GetRole(context));
    }

    [Fact]
    public void AdminSessionService_RejectsInvalidCredentials()
    {
        var options = new AdminOptions
        {
            Read = [new AdminAccountOptions { UserName = "reader", Password = "reader-secret" }],
            Super = [new AdminAccountOptions { UserName = "owner", Password = "owner-secret" }]
        };
        var service = CreateSessionService(options);

        Assert.Null(service.TryIssue("owner", "wrong-secret"));
    }

    private static AdminSessionService CreateSessionService(AdminOptions options)
    {
        var provider = DataProtectionProvider.Create("CodeWF.Tests");
        return new AdminSessionService(provider, Microsoft.Extensions.Options.Options.Create(options));
    }
}
