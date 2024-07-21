# 0.6.0 升级指南

## 包重命名

在 `0.6.0` 中我们对项目目录结构以及类库名统一规范，因此涉及到很多类库的重命名，重命名规则如下：

* `EntityFrameworkCore` -> `EFCore`: 受影响包为
    * `Masa.Contrib.Authentication.Oidc.EntityFrameworkCore` → `Masa.Contrib.Authentication.OpenIdConnect.EFCore`
    * `Masa.Contrib.Data.EntityFrameworkCore` → `Masa.Contrib.Data.EFCore`
    * `Masa.Contrib.Data.EntityFrameworkCore.SqlServer` → `Masa.Contrib.Data.EFCore.SqlServer`
    * `Masa.Contrib.Data.EntityFrameworkCore.Sqlite` → `Masa.Contrib.Data.EFCore.Sqlite`
    * `Masa.Contrib.Data.EntityFrameworkCore.Cosmos` → `Masa.Contrib.Data.EFCore.Cosmos`
    * `Masa.Contrib.Data.EntityFrameworkCore.Oracle` → `Masa.Contrib.Data.EFCore.Oracle`
    * `Masa.Contrib.Data.EntityFrameworkCore.PostgreSql` → `Masa.Contrib.Data.EFCore.PostgreSql`
    * `Masa.Contrib.Data.EntityFrameworkCore.MySql` → `Masa.Contrib.Data.EFCore.MySql`
    * `Masa.Contrib.Data.EntityFrameworkCore.Pomelo.MySql` → `Masa.Contrib.Data.EFCore.Pomelo.MySql`
    * `Masa.Contrib.Data.EntityFrameworkCore.InMemory` → `Masa.Contrib.Data.EFCore.InMemory`
* `XXX.EF` -> `XXX.EFCore`
    * `Masa.Contrib.Isolation.UoW.EF` → `Masa.Contrib.Isolation.UoW.EFCore`
    * `Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EF` → `Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore`
    * `Masa.Contrib.Ddd.Domain.Repository.EF` → `Masa.Contrib.Ddd.Domain.Repository.EFCore`
    * `Masa.Contrib.Data.UoW.EF` → `Masa.Contrib.Data.UoW.EFCore`
    * `Masa.Contrib.Data.Contracts.EF` → `Masa.Contrib.Data.Contracts.EFCore`
* `Oidc` -> `OpenIdConnect`
    * `Masa.BuildingBlocks.Authentication.Oidc.Cache` → `Masa.BuildingBlocks.Authentication.OpenIdConnect.Cache`
    * `Masa.BuildingBlocks.Authentication.Oidc.Domain` → `Masa.BuildingBlocks.Authentication.OpenIdConnect.Domain`
    * `Masa.BuildingBlocks.Authentication.Oidc.Models` → `Masa.BuildingBlocks.Authentication.OpenIdConnect.Models`
    * `Masa.BuildingBlocks.Authentication.Oidc.Storage` → `Masa.BuildingBlocks.Authentication.OpenIdConnect.Storage`
    * `Masa.Contrib.Authentication.Oidc.Cache.Storage` → `Masa.Contrib.Authentication.OpenIdConnect.Cache.Storage`
    * `Masa.Contrib.Authentication.Oidc.EFCore` → `Masa.Contrib.Authentication.OpenIdConnect.EFCore`
    * `Masa.Contrib.Authentication.Oidc.Cache.Storage` → `Masa.Contrib.Authentication.OpenIdConnect.Cache.Storage`
    * `Masa.Contrib.Authentication.Oidc.Cache` → `Masa.Contrib.Authentication.OpenIdConnect.Cache`
* `BasicAbility` -> `StackSdks`
    * `Masa.BuildingBlocks.BasicAbility.Auth` → `Masa.BuildingBlocks.StackSdks.Auth`
    * `Masa.BuildingBlocks.BasicAbility.Auth.Contracts` → `Masa.BuildingBlocks.StackSdks.Auth.Contracts`
    * `Masa.BuildingBlocks.BasicAbility.Dcc` → `Masa.BuildingBlocks.StackSdks.Dcc`
    * `Masa.BuildingBlocks.BasicAbility.Pm` → `Masa.BuildingBlocks.StackSdks.Pm`
    * `Masa.BuildingBlocks.BasicAbility.Mc` → `Masa.BuildingBlocks.StackSdks.Mc`
    * `Masa.BuildingBlocks.BasicAbility.Scheduler` → `Masa.BuildingBlocks.StackSdks.Scheduler`
    * `Masa.BuildingBlocks.BasicAbility.Tsc` → `Masa.BuildingBlocks.StackSdks.Tsc`
    * `Masa.Contrib.BasicAbility.Auth` → `Masa.Contrib.StackSdks.Auth`
    * `Masa.Contrib.BasicAbility.Auth.Contracts` → `Masa.Contrib.StackSdks.Auth.Contracts`
    * `Masa.Contrib.BasicAbility.Dcc` → `Masa.Contrib.StackSdks.Dcc`
    * `Masa.Contrib.BasicAbility.Pm` → `Masa.Contrib.StackSdks.Pm`
    * `Masa.Contrib.BasicAbility.Mc` → `Masa.Contrib.StackSdks.Mc`
    * `Masa.Contrib.BasicAbility.Scheduler` → `Masa.Contrib.StackSdks.Scheduler`
    * `Masa.Contrib.BasicAbility.Tsc` → `Masa.Contrib.StackSdks.Tsc`
* `ReadWriteSpliting` -> `ReadWriteSplitting`
    * `Masa.BuildingBlocks.ReadWriteSpliting.Cqrs` -> `Masa.BuildingBlocks.ReadWriteSplitting.Cqrs`
    * `Masa.Contrib.ReadWriteSpliting.Cqrs` -> `Masa.Contrib.ReadWriteSplitting.Cqrs`
* 其它
    * `Masa.Contrib.Identity.IdentityModel` → `Masa.Contrib.Authentication.Identity`
    * `Masa.BuildingBlocks.Identity.IdentityModel` → `Masa.BuildingBlocks.Authentication.Identity`
* 重构
    * `Caller` 从原来的 `Utils` 改为 `Contrib`，调整如下
        * `MASA.Utils.Caller.Core` -> `Masa.Contrib.Service.Caller`
        * `MASA.Utils.Caller.HttpClient` -> `Masa.Contrib.Service.Caller.HttpClient`
        * `MASA.Utils.Caller.DaprClient` -> `Masa.Contrib.Service.Caller.DaprClient`
    * `Caching` 从原来的 `Utils` 改为 `Contrib`，调整如下
        * `Masa.Utils.Caching.Redis` -> `Masa.Contrib.Caching.Distributed.StackExchangeRedis` (分布式缓存，由 `Redis` 提供，查看[文档](/framework/building-blocks/caching/stackexchange-redis))
        * `Masa.Utils.Caching.DistributedMemory` -> `Masa.Contrib.Caching.MultilevelCache` （多级缓存，由内存缓存与分布式缓存联合提供，查看[文档](/framework/building-blocks/caching/multilevel-cache)）
        * `Masa.Utils.Caching.Core`: 已删除，删除包即可

## 命名空间调整

调整命名空间，删除了很多命名空间，遇到命名空间不存在时可直接删除

## 写法优化

1. 集成事件

   :::: code-group
   ::: code-group-item 0.6.0之前

   ```csharp Program.cs
   builder.Services.AddDaprEventBus<IntegrationEventLogService>(options =>
   {
       options.UseEventLog<CatalogDbContext>()
              .UseEventBus()
              .UseUoW<CatalogDbContext>(dbOptions => dbOptions.UseSqlServer());
   });
   ```
   :::
   ::: code-group-item 0.6.0
   ```csharp Program.cs
   builder.Services.AddIntegrationEventBus(options =>
   {
       options.UseDapr().UseEventLog<CatalogDbContext>()
              .UseEventBus(dispatcherOptions => dispatcherOptions.UseMiddleware(typeof(ValidatorMiddleware<>)))
              .UseUoW<CatalogDbContext>(dbOptions => dbOptions.UseSqlServer());
   });
   ```
   :::
   ::::

2. 领域事件

   :::: code-group
   ::: code-group-item 0.6.0之前
   ```csharp Program.cs
   builder.Services.AddDomainEventBus(options =>
   {
       options.UseDaprEventBus<IntegrationEventLogService>(options => options.UseEventLog<PaymentDbContext>())
              .UseEventBus(eventBuilder => eventBuilder.UseMiddleware(typeof(ValidatorMiddleware<>)))
              .UseUoW<PaymentDbContext>(dbOptions => dbOptions.UseSqlServer())
              .UseRepository<PaymentDbContext>();
   });
   ```
   :::
   ::: code-group-item 0.6.0
   ```csharp Program.cs
   builder.Services.AddDomainEventBus(options =>
   {
       options.UseIntegrationEventBus(dispatcherOptions => dispatcherOptions.UseDapr().UseEventLog<PaymentDbContext>())
              .UseEventBus(eventBuilder => eventBuilder.UseMiddleware(typeof(ValidatorMiddleware<>)))
              .UseUoW<PaymentDbContext>(dbOptions => dbOptions.UseSqlServer())
              .UseRepository<PaymentDbContext>();
   });
   ```
   :::
   ::::

3. 最小 `API`

   原来不支持自动映射路由，且服务必须有一个无参的构造函数，例：

   :::: code-group
   ::: code-group-item 0.6.0之前
   ```csharp Services/DemoService.cs
   public class DemoService : ServiceBase
   {
       public DemoService(IServiceCollection services) : base(services)
       {
           App.MapGet("/api/v1/demo/username", GetUserName);
       }
   
       public string GetUserName()
       {
           return "Tony";
       }
   }
   ```
   :::
   ::: code-group-item 0.6.0
   ```csharp Services/DemoService.cs
   public class DemoService : ServiceBase
   {
       public string GetUserName()
       {
           return "Tony";
       }
   }
   ```
   :::
   ::::

[查看详情](/framework/building-blocks/minimal-apis)

## `MasaConfiguration`

`MasaConfiguration` 支持 `IServiceCollection` 扩展。

:::: code-group
::: code-group-item 0.6.0之前
```csharp Program.cs
builder.AddMasaConfiguration();
```
:::
::: code-group-item 0.6.0
```csharp Program.cs
builder.Services.AddMasaConfiguration();
```
:::
::::

[查看详情](/framework/building-blocks/configuration/override)

## OpenIdConnect

模型主键类型变更

`Int` -> `Guid`，新增包[Masa.Contrib.Authentication.OpenIdConnect.EFCore.Oracle](/framework/building-blocks/data/orm-efcore/oracle)，以适配 `Oracle` 数据库