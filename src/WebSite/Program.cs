using CodeWF.Options;
using Microsoft.Extensions.WebEncoders;
using Scalar.AspNetCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using WebSite.Models;
using WebSite.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<SiteOption>(builder.Configuration.GetSection("Site"));
builder.Services.AddSingleton<AppService>();
builder.AddApplication();


builder.Services.Configure<OpenAIOption>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddScoped<OpenAIHttpClientHandler>();

builder.Services.AddOpenApi();
var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
    var service = serviceScope.ServiceProvider.GetRequiredService<AppService>();
    await service.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}
else
{
    // /scalar/v1
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseApplication();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies([.. Config.Assemblies]);
app.Run();