using BlogWebSite.Client.RenderModes;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;


const bool isAutoProject = true;

if (isAutoProject)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


    builder.Services.AddMasaBlazorLocal();
    builder.Services.AddSingleton<IRenderMode, WasmRenderMode>();
    var app = builder.Build();
    await app.RunAsync();
}
else
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<Routes>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


    builder.Services.AddMasaBlazorLocal();
    builder.Services.AddSingleton<IRenderMode, WasmRenderMode>();

    await builder.Build().RunAsync();
}
