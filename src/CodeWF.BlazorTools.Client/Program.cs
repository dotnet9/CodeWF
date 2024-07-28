using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMasaBlazor(options =>
{
    options.ConfigureSsr(ssr =>
    {
        ssr.Left = 256;
        ssr.Top = 64;
    });
});

await builder.Build().RunAsync();
