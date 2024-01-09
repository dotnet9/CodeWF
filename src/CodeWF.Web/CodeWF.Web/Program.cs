using CodeWF.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMasaBlazor(builder =>
{
	builder.ConfigureTheme(theme =>
	{
		theme.Themes.Light.Primary = "#4318FF";
		theme.Themes.Light.Accent = "#4318FF";
	});
}).AddI18nForServer("wwwroot/i18n");


var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
               throw new Exception("Get the assembly root directory exception!");
builder.Services.AddNav(Path.Combine(basePath, $"wwwroot/nav/nav.json"));
builder.Services.AddScoped<CookieStorage>();
builder.Services.AddScoped<GlobalConfig>();
builder.Services.AddSingleton<ITranslationService, TranslationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();