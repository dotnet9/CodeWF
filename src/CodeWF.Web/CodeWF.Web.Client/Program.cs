using CodeWF.Core;
using Masa.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<ITranslationService, TranslationService>();
builder.Services.AddMasaBlazor(options =>
{
    options.ConfigureIcons(IconSet.MaterialDesign);
    options.ConfigureSsr(ssr =>
    {
        ssr.Left = 256;
        ssr.Top = 64;
    });
});

await builder.Build().RunAsync();