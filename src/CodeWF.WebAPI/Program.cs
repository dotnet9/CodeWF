using CodeWF.AspNetCore.EventBus;
using CodeWF.Auth.Options;
using CodeWF.Core.Abouts;
using CodeWF.Data.MySql;
using CodeWF.Data.PostgreSQL;
using CodeWF.Data.SQLite;
using CodeWF.WebAPI;
using CodeWF.WebAPI.Options;

var builder = WebApplication.CreateBuilder(args);
builder.WriteParameterTable();
builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SiteOption>(builder.Configuration.GetSection("Site"));
builder.Services.Configure<EncryptionOption>(builder.Configuration.GetSection("Encryption"));

builder.Services.AddMemoryCache();

var databaseType = builder.Configuration.GetConnectionString("DatabaseType");
var connectionString = builder.Configuration.GetConnectionString("CodeWFDatabase");
switch (databaseType!.ToLower())
{
    case "mysql":
        builder.Services.AddMySqlStorage(connectionString!);
        break;
    case "postgresql":
        builder.Services.AddPostgreSQLStorage(connectionString!);
        break;
    default:
        builder.Services.AddSQLiteStorage(connectionString!);
        break;
}

builder.Services.AddEventBus(typeof(GetAboutQuery).Assembly);

var app = builder.Build();

await app.InitStartUp();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.UseEventBus(typeof(GetAboutQuery).Assembly);

app.Run();