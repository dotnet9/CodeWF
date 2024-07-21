# 配置 - 概念

基于 [Microsoft.Extensions.Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration)。在 `Microsoft.Extensions.Configuration` 的基础上扩展出 **本地配置** 和 **远程配置** 。对于本地配置和远程配置，我们依然使用注入 `IOptions<xxxOptions>` 的方式获取配置。

* 本地配置：优化了 `Microsoft.Extensions.Configuration` 的配置方式，不需要手动 `Configure<xxxOption>`。
* 远程配置：对接分布式配置中心，远程配置更新各应用集群也会相应更新。一般用于应用程序较多，每个应用程序有相同的配置及自己的配置。

## 最佳实践

* Local：本地配置
  * [Masa.Contrib.Configuration](/framework/building-blocks/configuration/configuration-local): 基于 `Microsoft.Extensions.Configuration` 的基础上提供了更加方便的配置方式，不需要手动 `Configure<XXXOptions>`。
* ConfigurationApi：远程配置
  * [Masa.Contrib.Configuration.ConfigurationApi.Dcc](/framework/building-blocks/configuration/dcc): 对接分布式配置中心 [MASA DCC](/stack/dcc/introduce)，从而实现配置集中管理。
  * 更多……（敬请期待）

## 源码解读

### 结构调整

使用 `MasaConfiguration` 之后，配置的结构发生调整

```csharp 
IConfiguration
├── Local                           本地节点（固定）
│   ├── Redis                       自定义配置
│   ├── ├── Host                    参数
├── ConfigurationApi                远程节点（固定）
│   ├── AppId                       替换为你的AppId
│   ├── AppId ├── Redis             自定义节点
│   ├── AppId ├── Redis ├── Host    参数
```

### IMasaConfiguration

通过 `DI` 获取，并提供获取 `Local` 配置与 `ConfigurationApi` 配置的能力

### IConfigurationApiClient

通过 `DI` 获取，并提供获取指定远程配置的能力（与通过 `IConfiguration` 获取配置不同的是，它们的配置源不同，需要发送网络请求）

* GetRawAsync: 得到配置的原始配置信息
* GetAsync: 得到强类型的配置信息
* GetDynamicAsync: 得到动态类型的配置信息

### IConfigurationApiManage

通过 `DI` 获取，并提供新增或者更新配置的能力

* AddAsync：添加配置
* UpdateAsync：更新配置