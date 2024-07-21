# 示例

## 简介

`MASA.DCC` 提供了两个 `SDK`，一个是 `Masa.Contrib.Configuration.ConfigurationApi.Dcc` 用来获取和管理你的配置信息。另一个是 `Masa.Contrib.StackSdks.Dcc` 用来获取标签信息。

## 配置管理用例

通过 `DCC` 扩展 `IConfiguration` 管理远程配置的能力。而这不单依赖于 `DCC` 的 `SDK`，还需要依赖`MasaConfiguration`。`MasaConfiguration` 把配置分为本地节点和远程节点，而 `DCC` 就是远程节点。

```csharp
IConfiguration
├── Local                                本地节点（固定）
├── ConfigurationApi                     远程节点（固定 Dcc扩展其能力）
│   ├── AppId                            Replace-With-Your-AppId
│   ├── AppId ├── Redis                  自定义节点
│   ├── AppId ├── Redis ├── Host         参数
```

安装包

```shell 终端
dotnet add package Masa.Contrib.Configuration //MasaConfiguration的核心
dotnet add package Masa.Contrib.Configuration.ConfigurationApi.Dcc //由 DCC 提供远程配置的能力
```

### 入门

1. 配置 `DCC` 所需参数（远程能力）

   ```json appsettings.json
   {
     "DccOptions": {
       //Dcc服务地址
       "ManageServiceAddress": "",
       //Redis节点（因为Dcc通过Redis来提供远程配置的能力）
       "RedisOptions": {
         "Servers": [
           {
             "Host": "",
             "Port": ""
           }
         ],
         "DefaultDatabase": 0,
         "Password": ""
       },
       //应用Id
       "AppId": "Masa_Dcc_Test",
       //环境
       "Environment": "Development",
       //配置访问的对象列表
       "ConfigObjects": [ "AppSettings" ],
       //加密秘钥
       "Secret": "",
       //集群
       "Cluster": "Default"
     }
   }
   
   ```

2. 注册 `MasaConfiguration`，并使用 `DCC`

   ```csharp Program.cs
   builder.AddMasaConfiguration(configurationBuilder =>
   {
       configurationBuilder.UseDcc()
   });
   ```

3. 获取配置

   ```csharp
   using Masa.BuildingBlocks.Configuration;
   using Masa.Contrib.Configuration.ConfigurationApi.Dcc;
   
   /// <summary>
   /// 一个测试的获取配置的Controller
   /// </summary>
   [ApiController]
   [Route("[controller]/[action]")]
   public class DccConfigController : ControllerBase
   {
       private readonly IMasaConfiguration _configuration;
       public DccConfigController(IMasaConfiguration configuration)
       {
           _configuration = configuration;
       }
   
       [HttpGet]
       public string GetConfig()
       {
           //获取当前App的配置(如果想获取PublicConfig可以通过GetPublic())
           IConfiguration configuration = _configuration.ConfigurationApi.GetDefault();
           //AppSettings：配置项名称，ConnectionStrings:Db：json path
           string config = configuration.GetSection("AppSettings:ConnectionStrings:Db").Get<string>();
           return config;
       }
   }
   ```

### 2. 自定义配置Options映射关系

1. 映射的实体

   ```csharp
   public class AppSettings : ConfigurationApiMasaConfigurationOptions
   {
       public Logging Logging { get; set; }
   
       public string AllowedHosts { get; set; }
   
       public ConnectionStrings ConnectionStrings { get; set; }
   }
   
   public class Logging
   {
       public LogLevel LogLevel { get; set; }
   }
   
   public class LogLevel
   {
       public string Default { get; set; }
   
       [JsonPropertyName("Microsoft.AspNetCore")]
       public string Microsoft_AspNetCore { get; set; }
   }
   
   public class ConnectionStrings
   {
       public string Db { get; set; }
   }
   ```

2. 注册 `MasaConfiguration`，并使用 `DCC`

   :::: code-group
   ::: code-group-item 无需手动映射
   ```csharp Program
   builder.Services.AddMasaConfiguration(configurationBuilder =>
   {
       configurationBuilder.UseDcc();
       configurationBuilder.UseMasaOptions(options =>
       {
           //继承ConfigurationApiMasaConfigurationOptions类的无需手动注入
       });
   });
   ```
   :::
   ::: code-group-item 需要手动映射
   ```csharp
   builder.Services.AddMasaConfiguration(configurationBuilder =>
   {
       configurationBuilder.UseDcc();
       configurationBuilder.UseMasaOptions(options =>
       {
           //没有继承ConfigurationApiMasaConfigurationOptions类需要手动映射
           options.MappingConfigurationApi<AppSettings>("Masa_Dcc_Test", "AppSettings");
       });
   });
   ```
   :::
   ::::

3. 获取配置

   ```csharp
   using Microsoft.Extensions.Options;
   
   /// <summary>
   /// 一个测试的获取配置的Controller
   /// </summary>
   [ApiController]
   [Route("[controller]/[action]")]
   public class DccConfigController : ControllerBase
   {
       private readonly IOptions<AppSettings> _options;
       public DccConfigController(IOptions<AppSettings> options)
       {
           _options = options;
       }
   
       [HttpGet]
       public string GetConfig()
       {
           string config = _options.Value.ConnectionStrings.Db;
           return config;
       }
   }
   ```

### 3. 总结

`DCC` 为 `IConfiguration` 提供了远程配置的管理以及查看能力，完整的能力请查看 [文档](https://docs.masastack.com/framework/building-blocks/configuration/overview)

此处 Redis 为远程配置，介绍的是远程配置挂载到 `IConfiguration` 之后的效果以及用法，此配置与 `MASA.Contrib.Configuration` 中 `Redis` 的毫无关系，仅仅是展示同一个配置信息在两个源的使用方式以及映射节点关系的差别

### 4. 标签管理用例

通过 `DCC SDK` 的 `DccClient` 获取 `DCC` 的相关数据（标签）

```csharp
IDccClient
├── LabelService                  标签服务
```

1. 安装包

   ``` shell
   dotnet add package Masa.Contrib.StackSdks.Dcc
   ```

2. 注册 `MasaConfiguration`，并使用 `DCC`

   ```csharp Program.cs
   builder.Services.AddDccClient();
   ```

3. 获取标签
   
   ```csharp
   using Masa.BuildingBlocks.StackSdks.Dcc;
   using Masa.BuildingBlocks.StackSdks.Dcc.Contracts.Model;
   
   /// <summary>
   /// 一个测试的获取Label的Controller
   /// </summary>
   [ApiController]
   [Route("[controller]/[action]")]
   public class DccLabelController : ControllerBase
   {
       IDccClient _dccClient;
       public DccLabelController(IDccClient dccClient)
       {
           _dccClient = dccClient;
       }
   
       [HttpGet]
       public async Task<List<LabelModel>> GetLabel()
       {
           List<LabelModel> labels = await _dccClient.LabelService.GetListByTypeCodeAsync("TestLabel");
           return labels;
       }
   }
   ```
