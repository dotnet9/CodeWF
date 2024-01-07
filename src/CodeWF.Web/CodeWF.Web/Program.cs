using CodeWF.Core;
using CodeWF.Web.Client.Pages;
using CodeWF.Web.Components;
using Masa.Blazor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents()
	.AddInteractiveWebAssemblyComponents();

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
	.AddAdditionalAssemblies(typeof(GenerateSlug).Assembly);

app.Run();