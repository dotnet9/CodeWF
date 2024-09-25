using CodeWF.Blog.Web.Client.IServices;
using CodeWF.Blog.Web.Client.Options;
using CodeWF.Blog.Web.Client.Services;
using CodeWF.Blog.Web.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SiteOption>(builder.Configuration.GetSection("Site"));
builder.Services.AddSingleton<IBlogPostService, BlogPostService>();
builder.Services.AddSingleton<IFriendLinkService, FriendLinkService>();
builder.Services.AddSingleton(new LibraryConfiguration() { CollocatedJavaScriptQueryString = null });

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFluentUIComponents();
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CodeWF.Blog.Web.Client._Imports).Assembly);

app.Run();