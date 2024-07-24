using CodeWF.Docs.Core;
using CodeWF.Docs.Shared;
using CodeWF.Docs.Shared.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<LazyAssemblyLoader>();

await builder.Services.AddCodeWFDocs(builder.HostEnvironment.BaseAddress, BlazorMode.Wasm)
             .AddI18nForWasmAsync($"{builder.HostEnvironment.BaseAddress}/_content/CodeWF.Docs.Shared/locale");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.RootComponents.RegisterCustomElementsUsedJSCustomElementAttribute();

await builder.Build().RunAsync();
