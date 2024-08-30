using CodeWF.AspNetCore.EventBus;
using CodeWF.Core.Abouts;
using CodeWF.Data.MySql;
using CodeWF.Data.PostgreSQL;
using CodeWF.Data.SQLite;
using CodeWF.WebAPI;
using CodeWF.WebAPI.Options;

var builder = WebApplication.CreateBuilder(args);
builder.WriteParameterTable();
ConfigureServices(builder.Services);

var app = builder.Build();

await app.InitStartUp();

Configure();

app.Run();

void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    services.Configure<SiteOption>(builder.Configuration.GetSection("Site"));

    services.AddMemoryCache();

    var databaseType = builder.Configuration.GetConnectionString("DatabaseType");
    var connectionString = builder.Configuration.GetConnectionString("CodeWFDatabase");
    switch (databaseType!.ToLower())
    {
        case "mysql":
            services.AddMySqlStorage(connectionString!);
            break;
        case "postgresql":
            services.AddPostgreSQLStorage(connectionString!);
            break;
        default:
            services.AddSQLiteStorage(connectionString!);
            break;
    }

    services.AddEventBus(typeof(GetAboutQuery).Assembly);
}

void Configure()
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();
    app.MapControllers();
    app.UseEventBus(typeof(GetAboutQuery).Assembly);
}