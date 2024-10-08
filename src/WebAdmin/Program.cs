using Known.Data;
using CodeWF.EntityFramework;
using WebAdmin;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
builder.Services.AddApp(info =>
{
    info.WebRoot = builder.Environment.WebRootPath;
    info.ContentRoot = builder.Environment.ContentRootPath;
    //数据库连接
    info.Connections = [new Known.ConnectionInfo
    {
        Name = "Default",
        DatabaseType = DatabaseType.SQLite,
        ProviderType = typeof(Microsoft.Data.Sqlite.SqliteFactory),
        //DatabaseType = DatabaseType.Access,
        //ProviderType = typeof(System.Data.OleDb.OleDbFactory),
        //DatabaseType = DatabaseType.SqlServer,
        //ProviderType = typeof(System.Data.SqlClient.SqlClientFactory),
        //DatabaseType = DatabaseType.MySql,
        //ProviderType = typeof(MySqlConnector.MySqlConnectorFactory),
        //DatabaseType = DatabaseType.PgSql,
        //ProviderType = typeof(Npgsql.NpgsqlFactory),
        //DatabaseType = DatabaseType.DM,
        //ProviderType = typeof(Dm.DmClientFactory),
        ConnectionString = builder.Configuration.GetSection("ConnString").Get<string>()
    }];
});
// 如下为EFCore配置，若开启，请注释上面框架自带的连接配置
//builder.Services.AddCodeWFEntityFramework(config =>
//{
//    config.ConnString = builder.Configuration.GetSection("ConnString").Get<string>();
//});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAntiforgery();
app.UseApp();
app.MapRazorPages();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .AddAdditionalAssemblies([.. Config.Assemblies]);
app.Run();