using CodeWF.Options;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<WebEncoderOptions>(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

builder.Services.Configure<SiteOption>(builder.Configuration.GetSection("Site"));
builder.Services.AddSingleton<AppService>();
builder.AddApp();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseApp();
app.MapControllers();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .AddAdditionalAssemblies([.. Config.Assemblies]);
app.Run();