# 配置 - 本地配置

基于 `Microsoft.Extensions.Configuration` 的基础上进行了优化，不需要手动 `Configure<XXXOptions>`。

## 使用

1. 注册`MasaConfiguration`

   ```csharp
   builder.Services.AddMasaConfiguration(new List<Assembly>{ typeof(Program).Assembly });
   ```

2. 新增配置 `AppConfig` 类，并在 `appsettings.json` 文件新增配置项

   :::: code-group
   ::: code-group-item AppConfig.cs
   ```csharp AppConfig.cs
   public class AppConfig : LocalMasaConfigurationOptions
   {
       public List<string> PositionTypes { get; set; }
   
       public JWTConfig JWTConfig { get; set; }
   }
   
   public class JWTConfig
   {
       public string Issuer { get; set; }
       public string SecretKey { get; set; }
       public string Audience { get; set; }
   }
   ```
   :::
   ::: code-group-item appsettings.json

   ```json appsettings.json l:2-9
   {
     "AppConfig": {
       "PositionTypes": [ "Low", "Medium", "Hight" ],
       "JWTConfig": {
         "SecretKey": "MASAStack.com",
         "Issuer": "MASAStack",
         "Audience": "MASAStack"
       }
     }
   }
   
   ```
   :::
   ::::

3. 在构造函数中注入 `IOptions<AppConfig>` 对象获取`AppConfig`配置信息

   ```csharp
   [Route("api/[controller]")]
   [ApiController]
   public class HomeController : ControllerBase
   {
       private readonly IOptions<AppConfig> _positionTypeOptions;
   
       public HomeController(IOptions<AppConfig> positionTypeOptions)
       {
           _positionTypeOptions = positionTypeOptions;
       }
   
       [HttpGet]
       public AppConfig GetStrings()
       {
           return _positionTypeOptions.Value;
       }
   }
   ```

## 高阶用法

### 选项模式

本地配置也是基于 `.NET Core` 的 `Microsoft.Extensions.Configuration` 来实现的，所以它也支持 [.NET Core的 Options 模式](https://learn.microsoft.com/zh-cn/dotnet/core/extensions/options)

> 支持选项模式后，我们可以通过`IOptions<TModel>`、`IOptionsMonitor<TModel>`、`IOptionsSnapshot<TModel>` 获取配置信息

#### 自动映射指定 Section 

自动映射配置默认是通过类名称和属性名称跟本地配置项的名称匹配。如果当我们的配置节点和属性名称不一致时，那么可以重写父类的 `Section` 属性，如下所示：

```csharp AppConfig.cs l:3
public class AppConfig : LocalMasaConfigurationOptions
{
    public override string? Section => "App";
    
    public List<string> PositionTypes { get; set; }

    public JWTConfig JWTConfig { get; set; }
}

public class JWTConfig
{
    public string Issuer { get; set; }
    public string SecretKey { get; set; }
    public string Audience { get; set; }
}
```

#### 手动映射

当我们系统中某些类无法继承 `LocalMasaConfigurationOptions` 类时，那么可以手动指定映射的配置节点。

:::: code-group
::: code-group-item AppConfig.cs
```csharp AppConfig.cs
public class AppConfig
{
    public List<string> PositionTypes { get; set; }

    public JWTConfig JWTConfig { get; set; }
}

public class JWTConfig
{
    public string Issuer { get; set; }
    public string SecretKey { get; set; }
    public string Audience { get; set; }
}
```
:::
::: code-group-item 手动添加映射
```csharp Program.cs l:3-6
builder.Services.AddMasaConfiguration(configureBuilder =>
{
    configureBuilder.UseMasaOptions(options =>
    {
        options.MappingLocal<AppConfig>("App");
    });
});
```
:::
::::

### 通过 Configuration 获取配置

当然我们的配置也可以通过使用 [IConfiguration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) 或 `IMasaConfiguration` 获取配置。我们更加推荐你使用 `IMasaConfiguration` 去获取配置

首先我们先在配置文件中新增一个配置

:::: code-group
::: code-group-item appsettings.json
```json appsettings.json
{
  "App": {
    "PositionTypes": [ "Low", "Medium", "Hight" ],
    "JWTConfig": {
      "SecretKey": "MASAStack.com",
      "Issuer": "MASAStack",
      "Audience": "MASAStack"
    }
  }
}

```
:::
::: code-group-item 注册MasaConfiguration
```csharp
builder.Services.AddMasaConfiguration(new List<Assembly>{ typeof(Program).Assembly });
```
:::
::::

1. 推荐使用 `IMasaConfiguration` 获取配置值。我们只需要在构造函数中注入该对象，并使用 `Local` 属性中的 `GetSection` 方法

   ```csharp l:15
   [Route("api/[controller]")]
   [ApiController]
   public class HomeController : ControllerBase
   {
       private readonly IMasaConfiguration _masaConfiguration;
   
       public HomeController(IMasaConfiguration masaConfiguration)
       {
           _masaConfiguration = masaConfiguration;
       }
   
       [HttpGet]
       public string? GetStrings()
       {
           return _masaConfiguration.Local.GetSection("App:JWTConfig:Issuer")?.Value;
       }
   }
   ```

2. 使用 `IConfiguration` 获取配置值

   > 特此说明：如果使用了AddMasaConfiguration，通过 IConfiguration 获取配置需要再配置值前面增加 Local 节点。如获取 App 节点值，则需要 Configuration[Local:App]。这个获取方式会在 **1.0** 正式版本中调整

   ```csharp l:15
   [Route("api/[controller]")]
   [ApiController]
   public class HomeController : ControllerBase
   {
       private readonly IConfiguration _configuration;
   
       public HomeController(IConfiguration configuration)
       {
           _configuration = configuration;
       }
   
       [HttpGet]
       public string? GetStrings()
       {
           return _configuration.GetSection("Local:App:JWTConfig:Issuer")?.Value;
       }
   }
   ```

## 原理剖析

### 自动 Configure 映射配置

MASA Framework 中不需要手动 `Configure<xxxOptions>` 的原理，主要是通过扫描 `AddMasaConfiguration(assemblies)` 方法传入的程序集集合（不传则默认当前程序集），找出所有继承自 `LocalMasaConfigurationOptions` 的类，然后遍历子类集合，创建 [NamedConfigureFromConfigurationOptions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.options.namedconfigurefromconfigurationoptions-1) 实例添加 Options 配置。