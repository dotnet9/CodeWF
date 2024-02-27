namespace CodeWF.Auth;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlogAuthenticaton(this IServiceCollection services,
        IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection("Authentication");
        AuthenticationSettings? authentication = section.Get<AuthenticationSettings>();
        services.Configure<AuthenticationSettings>(section);

        switch (authentication.Provider)
        {
            case AuthenticationProvider.EntraID:
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                }).AddMicrosoftIdentityWebApp(configuration.GetSection("Authentication:EntraID"));
                // Internally pass `null` to cookie options so there's no way to add `AccessDeniedPath` here.

                break;
            case AuthenticationProvider.Local:
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                    {
                        options.AccessDeniedPath = "/auth/accessdenied";
                        options.LoginPath = "/auth/signin";
                        options.LogoutPath = "/auth/signout";
                    });
                break;
            default:
                string msg = $"Provider {authentication.Provider} is not supported.";
                throw new NotSupportedException(msg);
        }

        return services;
    }
}