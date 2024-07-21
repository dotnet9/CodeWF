# 事件总线 -常见问题

## 概述

记录了事件总线可能遇到的问题以及问题如何解决

## 进程内事件

获取'事件'关系链失败

1. 当 `Event` 、 `EventHandler` 、主工程不在同一个程序集中时，通过 `EventBus` 发布 `Event` 时会出现获取 `Event` 关系链失败的情况。当我们在没有特殊指定程序集的情况下使用 `AddEventBus` 时，默认使用当前域下的程序集。由于延迟加载特性，导致事件关系链的获取不完整。有以下两种解决方案：

   方案1：使用 `AddEventBus` 时，通过指定`Assembly`集合来指定当前项目使用的完整的应用程序集集合

   ```csharp
   var services = new ServiceCollection();
   services.AddEventBus(new[] { typeof(CustomEventMiddleware<>).Assembly });
   ```

   方案2：在使用 `AddEventBus` 之前，通过直接调用 `Event` 、 `EventHandler` 所在程序集的任何方法或类，确保其所在的应用程序程序集已经加载到当前程序集

   > 可通过 **AppDomain.CurrentDomain.GetAssemblies()** 查看其中是否包含对应 `Event`、`EventHandler` 的程序集


2. 按照文档操作，通过 `EventBus` 发布事件后，对应的 Handler 并没有执行，也没有发现错误？

   1. EventBus.PublishAsync(@event) 是异步方法，确保等待方法调用成功，检查是否出现同步方法调用异步方法的情况

   2. 注册 `EventBus` 时指定程序集集合，Assembly 被用于注册时获取并保存事件与 Handler 的对应关系

   > Assembly 的优先级：
   >
   > ```
   > 手动指定Assembly集合 -> MasaApp.GetAssemblies()
   > ```

   ```
   var builder = WebApplication.CreateBuilder(args);
   var assemblies = new[]
   {
       typeof(UserHandler).Assembly
   };
   builder.Services.AddEventBus(assemblies);
   ```

3. 通过 EventBus 发布事件，Handler出错，但数据依然保存到数据库中

   1. 检查是否禁用事务
      1. DisableRollbackOnFailure 是否为 true（是否失败时禁止回滚）
      2. UseTransaction 是否为 false（禁止使用事务）
   2. 检查当前数据库是否支持回滚。例如: 使用的是 MySQL 数据库，但回滚数据失败，请[查看](https://developer.aliyun.com/article/357842)

4. 为什么开启了异常重试却未执行重试？

   默认 `UserFriendlyException` 异常不支持重试，如果需要支持重试，则需要重新实现 `IExceptionStrategyProvider` 

5. 支持 Transaction

   配合 `MASA.Contrib.Ddd.Domain.Repository.EF.Repository` 、`UnitOfWork` 使用，当 `Event` 实现了 `ITransaction`，会在执行 `Add` 、`Update`、`Delete` 方法时自动开启事务，且在 `Handler` 全部执行后提交事务，当事务出现异常后，会自动回滚事务

6. EventBus 是线程安全的吗？

   不是线程安全的，如果多线程并发执行 EventBus.PublishAsync() ，则可能会出现数据未提交等异常

## 集成事件

1. 发生异常后，集成事件还会发送成功吗？

   此类问题需要判断当前场景是否开启事务，如果开启了事务，且提交事务在发生异常之后，那么集成事件是不会发送成功的，反之则会继续发送. [哪些场景会自动开启事务?](/framework/building-blocks/data/uow)