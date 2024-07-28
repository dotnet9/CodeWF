using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMasaBlazor(options =>
{
    options.ConfigureSsr();
});

await builder.Build().RunAsync();
