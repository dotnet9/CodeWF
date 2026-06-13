using System.Text.Json.Serialization;
using CodeWF.Api.Endpoints;
using CodeWF.Api.Models;
using CodeWF.Api.Options;
using CodeWF.Api.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));
builder.Services.AddDataProtection();
builder.Services.AddMemoryCache();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<BlogPostFileService>();
builder.Services.AddSingleton<ContentRepository>();
builder.Services.AddSingleton<GitRepositoryService>();
builder.Services.AddSingleton<AdminSessionService>();
builder.Services.AddScoped<AdminAuthorizationEndpointFilter>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var cors = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        var allowedOrigins = cors.AllowedOrigins.Count > 0
            ? cors.AllowedOrigins
            : builder.Environment.IsDevelopment()
                ? ["http://localhost:5000", "http://localhost:5001", "http://127.0.0.1:5000", "http://127.0.0.1:5001"]
                : throw new InvalidOperationException("Cors:AllowedOrigins must be configured outside Development.");

        policy
            .WithOrigins(allowedOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    var adminOptions = app.Services.GetRequiredService<IOptions<AdminOptions>>().Value;
    if (AdminGuard.HasUnsafeDefaultCredentials(adminOptions))
    {
        throw new InvalidOperationException("Unsafe default admin credentials are not allowed outside Development.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();
app.UseCors("frontend");

app.MapGet("/site/favicon/logo.ico", (IOptionsMonitor<SiteOptions> siteOptions) =>
{
    var root = Environment.ExpandEnvironmentVariables(siteOptions.CurrentValue.LocalAssetsDir);
    var path = Path.GetFullPath(Path.Combine(root, "site", "favicon", "logo.ico"));
    if (!File.Exists(path))
    {
        return Results.NotFound();
    }

    return Results.File(path, "image/x-icon");
});

var api = app.MapGroup("/api");

api.MapGet("/health", () => Results.Ok(new { status = "ok", at = DateTimeOffset.UtcNow }));

api.MapGet("/site", (ContentRepository repository) => Results.Ok(repository.GetSiteInfo()));

api.MapAdminSessionEndpoints();

var admin = api.MapGroup("/admin")
    .AddEndpointFilter<AdminAuthorizationEndpointFilter>();

admin.MapSiteSettingsEndpoints();
admin.MapAdminPostEndpoints();
admin.MapAdminContentEndpoints();
admin.MapAdminRepositoryEndpoints();
api.MapPublicContentEndpoints();
app.MapSyndicationEndpoints();

app.Run();

public partial class Program;
