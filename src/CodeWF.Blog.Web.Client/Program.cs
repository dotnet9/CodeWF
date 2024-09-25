using CodeWF.Blog.Web.Client.IServices;
using CodeWF.Blog.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.Services.AddSingleton<IBlogPostService, BlogPostService>();
//builder.Services.AddSingleton<IFriendLinkService, FriendLinkService>();
//builder.Services.AddSingleton(new LibraryConfiguration() { CollocatedJavaScriptQueryString = null });
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
