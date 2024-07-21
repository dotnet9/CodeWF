# 配置 - 分布式配置（DCC）

## 概念

基于 [MASA DCC](/stack/dcc/introduce) 分布式配置中心实现的远程配置。当远程配置发生变更的时候，我们的应用中的配置也会同步更新[查看原理](#同步更新配置)

## 使用

1. 安装 `Masa.Contrib.Configuration.ConfigurationApi.Dcc`

   ``` shell 终端
   dotnet add package Masa.Contrib.Configuration.ConfigurationApi.Dcc
   ```

2. 添加 `MASA DCC` 配置，并注册 DCC 服务

   > MASA DCC 是将配置写入到 Redis。所以我们的项目读取配置信息需要配置 Redis 服务。

   :::: code-group
   ::: code-group-item appsettings.json
   ``` json appsettings.json
   {
     //Dcc configuration, extended Configuration capabilities, support for remote configuration (new)
     "DccOptions": {
       "ManageServiceAddress ": "http://localhost:8890",
       "RedisOptions": {
         "Servers": [
           {
             "Host": "localhost",
             "Port": 8889
           }
         ],
         "DefaultDatabase": 0,
         "Password": ""
       }
     },
     "AppId": "Dcc's Application Id",
     "Environment": "Development",
     "ConfigObjects": [ "Platforms" ], //The name of the configuration object to be mounted
     "Secret": "", //The secret key of Dcc AppId, which provides permission for updating remote configuration
     "Cluster": "Default"
   }
   ```
   :::
   ::: code-group-item Program.cs
   ``` csharp Program.cs
   builder.Services.AddMasaConfiguration(configureBuilder => configureBuilder.UseDcc());
   ```
   :::
   ::::

3. 新增 AppConfig 配置类，并继承 `DccConfigurationOptions` 类

   ```csharp
   public class AppConfig : DccConfigurationOptions
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

4. 在 MASA DCC 中增加一个 AppConfig 配置项，如下图所示：

   > 增加的配置项，最好添加到指定的应用集群环境中去，指定的应用集群环境是指和appsettings.json配置的AppId、Cluster、Environment 保持一致。
   
   ![DCC-Configuration](https://cdn.masastack.com/framework/building-blocks/configuration/dcc-configuration.png)

5. 在构造函数中注入 `IOptions<AppConfig>` 对象获取 `AppConfig` 配置信息

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

### 自动映射指定 Section 

`MASA DCC` 配置默认是通过类名称和属性名称去跟远程配置项的名称进行匹配。如果当我们的配置节点和属性名称不一致时，那么可以通过重写父类的 `Section` 属性来实现自动映射，如下所示：

```csharp AppConfig.cs l:3
public class AppConfig : DccConfigurationOptions
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

### 手动映射

当我们系统中某些类无法继承 `DccConfigurationOptions` 类时，那么可以手动指定映射的配置节点。

:::: code-group
::: code-group-item  AppConfig.cs
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
```csharp Program.cs l:4-7
builder.Services.AddMasaConfiguration(configureBuilder =>
{
    configureBuilder.UseDcc();
    configureBuilder.UseMasaOptions(options =>
    {
        options.MappingConfigurationApi<AppConfig>("Dcc's Application Id","App");
    });
});
```
:::
::::

### 通过 Configuration 获取配置

当然我们的配置也可以通过使用 [IConfiguration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) 或 `IMasaConfiguration` 获取配置。我们更加推荐你使用 `IMasaConfiguration` 去获取配置

首先我们先添加一个配置类，并去 MASA DCC 中配置它

:::: code-group
::: code-group-item AppConfig.cs
```csharp AppConfig.cs l:3
public class AppConfig : DccConfigurationOptions
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
:::
::: code-group-item 注册MasaConfiguration
```csharp
builder.Services.AddMasaConfiguration(configureBuilder => configureBuilder.UseDcc());
```
:::
::::

![DCC-Configuration](https://cdn.masastack.com/framework/building-blocks/configuration/dcc-configuration.png)

1. 推荐使用 `IMasaConfiguration` 获取配置值。我们只需要在构造函数中注入该对象，并使用 `ConfigurationApi` 属性中的 `GetSection` 方法

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
           return _masaConfiguration.ConfigurationApi.Get("Dcc's Application Id").GetSection("App:JWTConfig:Issuer")?.Value;
       }
   }
   ```

2. 使用 `IConfiguration` 获取配置值

   > 特此说明：如果使用了 AddMasaConfiguration，通过 IConfiguration 获取配置需要再配置值前面增加 ConfigurationApi 节点，如获取App节点值，则需要 Configuration[ConfigurationApi:App]。这个获取方式会在 **1.0** 正式版本中调整

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
           return _configuration.GetSection("ConfigurationApi:App:JWTConfig:Issuer")?.Value;
       }
   }
   ```

### 通过 IConfigurationApiClient 获取配置

我们也可以通过 `IConfigurationApiClient` 对象获取配置，以及监听配置变更。并且该对象除了能够获取当前应用的配置，也可以获取其它集群环境应用的配置信息。

1. 获取当前应用的配置

   ```csharp
   [Route("api/[controller]")]
   [ApiController]
   public class HomeController : ControllerBase
   {
       private readonly IConfigurationApiClient _configurationApiClient;
   
       public HomeController(IConfigurationApiClient configurationApiClient)
       {
           _configurationApiClient = configurationApiClient;
       }
   
       [HttpGet]
       public async Task<AppConfig> GetAppConfig()
       {
           return await _configurationApiClient.GetAsync<AppConfig>("AppConfig");
       }
   }
   ```
   
2. 获取其它集群环境应用的配置

   ```csharp
   [Route("api/[controller]")]
   [ApiController]
   public class HomeController : ControllerBase
   {
       private readonly IConfigurationApiClient _configurationApiClient;
   
       public HomeController(IConfigurationApiClient configurationApiClient)
       {
           _configurationApiClient = configurationApiClient;
       }
   
       [HttpGet]
       public async Task<AppConfig> GetAppConfig()
       {
           return await _configurationApiClient.GetAsync<AppConfig>("enviroment", "cluster", "appId", "AppConfig");
       }
   }
   ```

### 使用 IConfigurationApiManage 管理配置

当你需要在当前应用修改和更新某个配置的时候，你可以使用 `IConfigurationApiManage` 对象进行管理，如下：

```csharp
[Route("api/[controller]")]
[ApiController]
public class ConfigurationApiManageController : ControllerBase
{
    private readonly IConfigurationApiManage _configurationApiManage;

    public ConfigurationApiManageController(IConfigurationApiManage configurationApiManage)
    {
        _configurationApiManage = configurationApiManage;
    }

    [HttpPost]
    public async Task AddConfiguration()
    {
        var configObjectDic = new Dictionary<string, object> { };
        configObjectDic[nameof(AppConfig)] = new AppConfig
        {
            JWTConfig = new JWTConfig { Audience = "MASAStack.com" },
            PositionTypes = new List<string> { "MASA Stack" }
        };
        await _configurationApiManage.AddAsync("development enviroment", "default cluster", "application id", configObjectDic);
    }

    [HttpPut]
    public async Task UpdateConfiguration()
    {
        var configObject = new AppConfig
        {
            JWTConfig = new JWTConfig { Audience = "MASAStack.com" },
            PositionTypes = new List<string> { "MASA Stack" }
        };
        await _configurationApiManage.UpdateAsync("development enviroment", "default cluster", "application id", nameof(AppConfig), configObject);
    }
}
```

## 扩展

如何支持其它配置中心，以 `Apollo` 为例：

1. 新建类库 `Masa.Contrib.Configuration.ConfigurationApi.Apollo`
2. 新建 `ApolloConfigurationRepository` 并继承类 `AbstractConfigurationRepository`

   ```csharp
   internal class ApolloConfigurationRepository : AbstractConfigurationRepository
   {
       private readonly IConfigurationApiClient _client;
       public override SectionTypes SectionType => SectionTypes.ConfigurationAPI;
   
       public DccConfigurationRepository(
           IConfigurationApiClient client,
           ILoggerFactory loggerFactory)
           : base(loggerFactory)
       {
           _client = client;
           
           //todo: Use IConfigurationApiClient to obtain configuration information that needs to be mounted to remote nodes and monitor configuration changes
           // Fired when configuration changes FireRepositoryChange(SectionType, Load());
       }
   
       public override Properties Load()
       {
           //todo: 返回当前挂载到远程节点的配置信息
       }
   }
   ```

3. 新建类 `ConfigurationApiClient`，为 `ConfigurationApi` 提供获取基础配置的能力

   ```csharp
   public class ConfigurationApiClient : IConfigurationApiClient
   {
       public Task<(string Raw, ConfigurationTypes ConfigurationType)> GetRawAsync(string configObject, Action<string>? valueChanged = null)
       {
           throw new NotImplementedException();
       }
   
       public Task<(string Raw, ConfigurationTypes ConfigurationType)> GetRawAsync(string environment, string cluster, string appId, string configObject, Action<string>? valueChanged = null)
       {
           throw new NotImplementedException();
       }
   
       public Task<T> GetAsync<T>(string configObject, Action<T>? valueChanged = null);
       {
           throw new NotImplementedException();
       }  
       public Task<T> GetAsync<T>(string environment, string cluster, string appId, string configObject, Action<T>? valueChanged = null);
       {
           throw new NotImplementedException();
       }  
       public Task<dynamic> GetDynamicAsync(string environment, string cluster, string appId, string configObject, Action<dynamic> valueChanged)
       {
           throw new NotImplementedException();
       }
   
       public Task<dynamic> GetDynamicAsync(string key)
       {
           throw new NotImplementedException();
       }
   }
   ```

4. 新建类 `ConfigurationApiManage`，为 `ConfigurationApi` 提供管理配置的能力

   ```csharp
   public class ConfigurationApiManage : IConfigurationApiManage
   {
       // Initialize the remote configuration under the AppId through the management terminal
       public Task InitializeAsync(string environment, string cluster, string appId, Dictionary<string, string> configObjects)
       {
           throw new NotImplementedException();
       }
   
       // Update the information of the specified configuration through the management terminal
       public Task UpdateAsync(string environment, string cluster, string appId, string configObject, object value)
       {
           throw new NotImplementedException();
       }
   }
   ```

5. 新建 `ConfigurationApiMasaConfigurationOptions` 类，并继承 `MasaConfigurationOptions`
   
   不同的配置中心中存储配置的名称是不一样的，在 `Apollo` 中配置对象名称叫做命名空间，因此为了方便开发人员可以使用起来更方便，我们建议不同的配置中心可以有自己专属的属性，以此来降低开发人员的学习成本

   ```csharp
   public abstract class ConfigurationApiMasaConfigurationOptions : MasaConfigurationOptions
   {
       /// <summary>
       /// The name of the parent section, if it is empty, it will be mounted under SectionType, otherwise it will be mounted to the specified section under SectionType
       /// </summary>
       [JsonIgnore]
       public sealed override string? ParentSection => AppId;
   
       public virtual string AppId => StaticConfig.AppId;
   
       /// <summary>
       /// The section null means same as the class name, else load from the specify section
       /// </summary>
       [JsonIgnore]
       public sealed override string? Section => Namespace;
   
       /// <summary>
       /// 
       /// </summary>
       public virtual string? Namespace { get; }
   
       /// <summary>
       /// Configuration object name
       /// </summary>
       [JsonIgnore]
       public sealed override SectionTypes SectionType => SectionTypes.ConfigurationApi;
   }
   ```

6. 选中类库 `Masa.Contrib.BasicAbility.Apollo`，并新建 `IMasaConfigurationBuilder` 的扩展方法 `UseApollo`

   ```csharp
   public static class MasaConfigurationExtensions
   {
       public static IMasaConfigurationBuilder UseApollo(this IMasaConfigurationBuilder builder)
       {
           //todo：Register IConfigurationApiClient and IConfigurationApiManage into the service collection, and add ApolloConfigurationRepository through builder.AddRepository()
           return builder;
       }
   }
   ```

## 原理剖析

### 同步更新配置

   为何分布式配置可以实现远程配置发生更新后，应用的配置会随之更新?
   
   远程配置更新使用了分布式缓存提供的 [Pub/Sub](/framework/building-blocks/caching/stackexchange-redis#使用PubSub)能力
