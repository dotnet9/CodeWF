# 事件总线 - 集成事件

## 概述

集成事件总线允许发布和订阅跨服务传输的消息，服务的发布与订阅不在同一个进程中，在 MASA Framework 中，提供了一个可以被开箱即用的程序，它们由以下程序提供

* Masa.Contrib.Dispatcher.IntegrationEvents: 支持[发件箱模式](https://www.kamilgrzybek.com/design/the-outbox-pattern/)，但仅提供集成事件发布的抽象以及本地消息的抽象，它们的实现由其它类库提供 
    * Masa.Contrib.Dispatcher.IntegrationEvents.Dapr: 借助 [Dapr](https://docs.dapr.io/zh-hans/developing-applications/building-blocks/pubsub/pubsub-overview/) 实现了集成事件发布
    * Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore: 提供了本地消息的实现，是基于 `EFCore` 实现的集成事件日志的提供者

## 入门

目前 MASA Framework 仅提供了基于 [`Dapr`](https://docs.dapr.io/zh-hans/developing-applications/building-blocks/pubsub/pubsub-overview/) 的集成事件的发布，我们以 [`Dapr`](https://docs.dapr.io/zh-hans/developing-applications/building-blocks/pubsub/pubsub-overview/) 为例，看一下如何使用集成事件

1. 安装集成事件、工作单元、SqlServer 数据库

   ```shell 终端
   dotnet add package Masa.Contrib.Dispatcher.IntegrationEvents //使用提供发件箱模式的集成事件
   dotnet add package Masa.Contrib.Dispatcher.IntegrationEvents.Dapr //使用dapr提供的pubsub能力
   dotnet add package Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore //本地消息表
   dotnet add package Masa.Contrib.Data.UoW.EFCore //工作单元
   dotnet add package Masa.Contrib.Data.EFCore.SqlServer // SqlServer数据库
   ```

2. 注册 `MasaDbContext`

   :::: code-group
   ::: code-group-item 注册 CustomDbContext

   ```csharp Program.cs l:2-4
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddMasaDbContext<CustomDbContext>(dbContextBuilder =>
   {
       dbContextBuilder.UseSqlite("server=localhost;uid=sa;pwd=P@ssw0rd;database=catalog");
   });
   ```
   :::
   ::: code-group-item CustomDbContext

   ```csharp Infrastructure/CustomDbContext.cs l:1
   public class CustomDbContext : MasaDbContext<CustomDbContext>
   {
       public DbSet<User> Users { get; set; } = null!;
       
       public CustomDbContext(MasaDbContextOptions<CustomDbContext> dbContextOptions) : base(dbContextOptions)
       {
       }
   }
   ```
   :::
   ::::

3. 注册集成事件并使用工作单元

   ```csharp Program.cs
   builder.Services
       .AddIntegrationEventBus(options=>
       {
           options
               .UseDapr()//使用Dapr提供pub/sub能力，也可以自行选择其他的
               .UseEventLog<CustomDbContext>()//使用基于EFCore的本地消息表
               .UseUoW<CustomDbContext>()//使用工作单元
       });
   ```

4. 自定义类 `DemoIntegrationEvent` ，并继承 `IntegrationEvent` 

   ```csharp
   public record DemoIntegrationEvent : IntegrationEvent
   {
       public override string Topic { get; set; } = nameof(DemoIntegrationEvent);//topic name
   
       //todo 自定义属性参数
   }
   ```

5. 发送集成事件

   ```csharp
   IEventBus eventBus;//通过 DI 得到 IEventBus
   var @event = new DemoIntegrationEvent();
   await eventBus.PublishAsync(@event);//发送集成事件
   ```

## 配置

|  参数名   | 参数描述  | 默认值  | 
|  ----  | ----  | ----  |
| LocalRetryTimes  | 发布事件最大允许重试次数 (本地队列任务) | 3 |
| MaxRetryTimes  | 发布事件最大允许重试次数 (持久化队列任务 ) | 10 |
| LocalFailedRetryInterval  | 重试间隔 (本地队列) | 3 秒 |
| FailedRetryInterval  | 重试间隔 (持久化队列) | 60 秒 |
| MinimumRetryInterval  | 最小重试间隔，重试 **多久之前** 状态为失败或进行中的本地消息 (持久化队列) | 60 秒 |
| RetryBatchSize  | 每次重试**多少条**状态为失败或进行中的本地消息 (持久化队列) | 100 |
| CleaningLocalQueueExpireInterval  | 执行删除过期任务的事件间隔 (本地队列) | 60 秒 |
| CleaningExpireInterval  | 执行删除过期任务的时间间隔 (持久化队列) | 300 秒 |
| PublishedExpireTime  | 发布成功消息的过期时间 (当状态为已发布，且修改时间与当前时间间隔大于设置的过期时间后，消息将会被删除，持久化队列) | (24 * 3600) 秒 |
| DeleteBatchCount  | 批量删除过期的本地消息记录的最大条数 (持久化队列) | 1000 |

例如，最大重试次数改为5次，则:

```csharp
builder.Services
    .AddIntegrationEventBus(options=>
    { 
        options
            .UseDapr()//使用Dapr提供pub/sub能力，也可以选择其它实现
            .UseEventLog<UserDbContext>();

        options.MaxRetryTimes = 5;//设置最大重试次数
    });
```

## 源码解读

首先我们先要知道的基础知识点:

* IIntegrationEvent：集成事件接口，继承 IEvent（本地事件接口）、ITopic（订阅接口，发布订阅的主题）、ITransaction（事务接口）
* IIntegrationEventBus：集成事件总线接口、用于提供发送集成事件的功能
* IIntegrationEventLogService：集成事件日志服务的接口（提供保存本地日志、修改状态为进行中、成功、失败、删除过期日志、获取等待重试日志列表的功能）
* IntegrationEventLog：集成事件日志，提供本地消息表的模型
* IHasConcurrencyStamp：并发标记接口 (实现此接口的类会自动为 RowVersion 赋值)

### Masa.Contrib.Dispatcher.IntegrationEvents

提供了集成事件接口的实现类，并支持了[发件箱模式](https://www.kamilgrzybek.com/design/the-outbox-pattern/)，其中:

* IPublisher：集成事件的发送者
* IProcessingServer：后台服务接口
* IProcessor：处理程序接口（后台处理程序中会获取所有的程序实现并执行）
    * DeleteLocalQueueExpiresProcessor：删除过期程序（从本地队列删除）
    * DeletePublishedExpireEventProcessor：删除已过期的发布成功的本地消息程序（从Db删除）
    * RetryByLocalQueueProcessor：重试本地消息记录（从本地队列中获取，条件: 发送状态为失败或进行中且重试次数小于最大重试次数且重试间隔大于最小重试间隔）
    * RetryByDataProcessor：重试本地消息记录（从Db获取，条件: 发送状态为失败或进行中且重试次数小于最大重试次数且重试间隔大于最小重试间隔，且不在本地重试队列中）
* IntegrationEventBus：IIntegrationEvent 的实现

在 `Masa.Contrib.Dispatcher.IntegrationEvents` 中有两个队列，它们分别是:

* 本地队列
    * 重试间隔短，支持秒级别的重试间隔
    * 从内存获取重试数据，速度更快
    * 系统崩溃后，本地队列不会重建，自动降级到持久化队列中进行重试
* 持久化数据源队列
    * 作为本地内存队列的降级方案，重试间隔相比本地队列重试间隔略长，对db或者其他数据源压力更低
    * 不受系统重启影响，确保集成事件被成功发送

虽然我们针对消息重发做了特殊处理，理论上不会出现消息会多次发布，但仍然不排除消息有可能被多次发送的情况，例如: 在消息发布后更新状态后还未来得及持久化就down机或者断网了，那么服务恢复正常后会继续重试重发，造成同一条消息被重复发送的情况，在消息的订阅方也是如此，大多数的消息队列都做到了 `At Least Once`，这意味着如果在订阅方没有做好幂等的话，可能会导致消费会被多次执行，因此强烈建议消费端做好幂等处理，以免造成严重后果

### Masa.Contrib.Dispatcher.IntegrationEvents.Dapr

* Publisher: 通过 [`Dapr`](https://docs.dapr.io/zh-hans/developing-applications/building-blocks/pubsub/pubsub-overview/) 提供的 [PubSub](https://zh.wikipedia.org/wiki/%E5%8F%91%E5%B8%83/%E8%AE%A2%E9%98%85) 能力实现了发送集成事件的能力

   > 它可以被替换成任何具有 [PubSub](https://zh.wikipedia.org/wiki/%E5%8F%91%E5%B8%83/%E8%AE%A2%E9%98%85) 能力的库

   `Dapr`提供了 PubSub 的抽象，它有多个实现方，根据自己的需要选择一个即可，[查看 Dapr 提供的 PubSub 实现](https://docs.dapr.io/zh-hans/reference/components-reference/supported-pubsub/)

### Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore

* IntegrationEventLogService: 基于 [EFCore](https://learn.microsoft.com/zh-cn/ef/core) 的集成事件日志服务的实现

   它提供了集成事件日志服务的实现，为集成事件提供[发件箱模式](https://www.kamilgrzybek.com/design/the-outbox-pattern/)提供了支持

   > 目前还未支持标准化的 Sub 能力，暂时使用实现方原生的写法

## 扩展

如果希望扩展集成事件的其它实现，存在两种情况

### 接入方支持发件箱模式

接入的提供者已经实现了[发件箱模式](https://www.kamilgrzybek.com/design/the-outbox-pattern/)，此时我们仅需要实现 `IIntegrationEventBus` 即可，在使用的时候，也仅需要安装这个`新的提供者`类库即可

> 要注意: 为确保消息的发布与本地任务的原子性，需要使用 `IUnitOfWork` 提供的 `Transaction` 来确保两者是在同一个事务

### 接入方不支持发件箱模式

接入的提供者未实现[发件箱模式](https://www.kamilgrzybek.com/design/the-outbox-pattern/)，仅提供了 `PubSub` 的能力，那么需要引用 `Masa.Contrib.Dispatcher.IntegrationEvents` ，并实现 `IPublisher` 即可，但在使用集成事件时，除了引用新的提供者之外，还需要额外引用`Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore`，以 `RabbitMq` 为例

1. 新建类库 `Masa.Contrib.Dispatcher.IntegrationEvents.RabbitMq` ，添加 `Masa.Contrib.Dispatcher.IntegrationEvents` 项目引用，并安装`RabbitMQ.Client`

   ``` shell 终端
   dotnet add package RabbitMQ.Client //使用RabbitMq
   ```

2. 新增类 `Publisher`，并实现 `IPublisher`

   ```csharp
   public class Publisher : IPublisher
   {
       public async Task PublishAsync<T>(string topicName, T @event, CancellationToken stoppingToken = default)
       {
           //todo: 通过 RabbitMQ.Client 发送消息到RabbitMq
           throw new NotImplementedException();
       }
   }
   ```

3. 新建类 `DispatcherOptionsExtensions`，将自定义 `Publisher` 注册到服务集合

   ```csharp l:6
   public static class DispatcherOptionsExtensions
   {
       public static DispatcherOptions UseRabbitMq(this Masa.Contrib.Dispatcher.IntegrationEvents.Options.DispatcherOptions options)
       {
            //todo: 注册 RabbitMq 信息
            dispatcherOptions.Services.TryAddSingleton<IPublisher, Publisher>();
            return dispatcherOptions;
       }
   }
   ```

4. 如何使用自定义实现 `RabbitMq`

   ```csharp l:3
   builder.Services.AddIntegrationEventBus(option =>
   {
       option.UseRabbitMq();//修改为使用 RabbitMq
       option.UseUoW<UserDbContext>(optionBuilder => optionBuilder.UseSqlite($"Data Source=./Db/{Guid.NewGuid():N}.db;"));
       option.UseEventLog<UserDbContext>();
   });
   ```