using CodeWF.Data.SQLite;
using CodeWF.WebAPI;
using CodeWF.WebAPI.Options;

var builder = WebApplication.CreateBuilder(args);
builder.WriteParameterTable();
ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    // 绑定网站配置
    services.Configure<SiteOption>(builder.Configuration.GetSection("Site"));

    // 添加MemoryCache支持
    services.AddMemoryCache();

    var dbType = builder.Configuration.GetConnectionString("DatabaseType");
    var connStr = builder.Configuration.GetConnectionString("CodeWFDatabase");
    switch (dbType!.ToLower())
    {
        default:
            services.AddSQLiteStorage(connStr!);
            break;
    }
}