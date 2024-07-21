# 领域驱动设计 - 常见问题

## 使用仓储时提示仓储未注册

例如：`IRepository<CatalogType>` 提示未注册，则需要依次检查：

1. `CatalogType` 必须继承 `IEntity`

2. 指定包含领域模型、仓储接口以及实现的程序集

   :::: code-group
   ::: code-group-item 指定程序集

   ```csharp Domain/Entities/CatalogBrand.cs
   var assemblies = AppDomain.CurrentDomain.GetAssemblies().Append(typeof(CatalogItem).Assembly);
   
   builder.Services.AddDomainEventBus(assemblies, options =>
   {
       options.UseRepository<CatalogDbContext>();
   });
   ```
   :::
   ::: code-group-item 更改全局程序集

   ```csharp Domain/Entities/CatalogType.cs
   MasaApp.SetAssemblies(AppDomain.CurrentDomain.GetAssemblies().Append(typeof(CatalogItem).Assembly));
   
   builder.Services.AddDomainEventBus(options =>
   {
       options.UseRepository<CatalogDbContext>();
   });
   ```
   :::
   ::::

## 通过 IDomainEventBus 入队的领域事件什么时候被发布？

* 手动调用 `IDomainEventBus` 提供的 `PublishQueueAsync`
  * 集成事件发送事件有两种情况：
    * 未禁用事务：在 `IUnitOfWork`（工作单元）提交后
    * 未开启事务：`PublishQueueAsync` 方法被调用立即发送
* 最外层的 `IEventBus` 执行结束后被发送