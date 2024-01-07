var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.RegisterCommonService();

await builder.Build().RunAsync();