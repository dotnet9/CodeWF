using CodeWF.Options;
using CodeWF.Tools.FileExtensions;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.WebEncoders;
using Quartz;
using Scalar.AspNetCore;
using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using WebSite.Jobs;
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
builder.Services.AddSingleton<ISevenZipCompressor, SevenZipCompressor>();

builder.Services.Configure<GzipCompressionProviderOptions>(options => 
{
    options.Level = CompressionLevel.SmallestSize;
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => 
{
    options.Level = CompressionLevel.SmallestSize;
});
    
builder.Services.AddResponseCompression();
//builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();


builder.Services.Configure<OpenAIOption>(builder.Configuration.GetSection("OpenAI"));

builder.Services.AddOpenApi();

builder.Services.AddTransient<FileCleanerJob>();
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("fileCleanerJob", "group1");
    q.AddJob<FileCleanerJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("fileCleanerTrigger", "group1")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(2)
            .RepeatForever()));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

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

app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseApplication();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies([.. Config.Assemblies]);
app.Run();