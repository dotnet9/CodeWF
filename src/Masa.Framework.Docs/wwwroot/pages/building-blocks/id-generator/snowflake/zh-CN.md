# ID Generator（ID 生成器）- 雪花ID

## 概述

是 Twitter 开源的分布式 `ID` 生成算法，结果是 64bit 的 `Long` 类型的 `ID` ，有着 **全局唯一** 和 **有序递增** 的特点。

在 MASA Framework 中提供了 雪花 `ID` 的默认实现，通过它可以得到唯一有序的 `Long`类型 `ID`

## 使用

1. 安装 `Masa.Contrib.Data.IdGenerator.Snowflake`

   ```shell 终端
   dotnet add package Masa.Contrib.Data.IdGenerator.Snowflake
   ```

2. 注册雪花 ID 生成器

   ```csharp Program.cs
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddSnowflake();
   ```

3. 获取 ID

   :::: code-group
   ::: code-group-item 通过 雪花 ID 生成器工厂创建（静态）

   ```csharp Domain/Entities/CatalogBrand.cs
   using Masa.BuildingBlocks.Data;
   
   namespace Masa.EShop.Service.Catalog.Domain.Entities;
   
   public class CatalogBrand
   {
       public long Id { get; set; }
       
       public string Brand { get; set; }
   
       private CatalogBrand()
       {
           Id = IdGeneratorFactory.SnowflakeGenerator.NewId();
       }
   }
   ```
   :::
   ::: code-group-item 通过 DI 获取

   ```csharp Program.cs
   app.MapGet("/getid", (ISnowflakeGenerator generator) => { return generator.NewId(); });
   ```
   :::
   ::::

## 高阶用法

### 分布式部署

默认提供雪花 `ID` 的方案对分布式部署场景并不友好，它的工作机器id是通过获取环境变量 `WORKER_ID` 来得到的，尽管在使用非容器化部署时可通过手动指定环境变量的方式使用，但在容器化部署场景时，它将毫无作用，因此我们采用了 `Redis` 服务来记录已经被使用的 `WorkerId` ，确保不会生成重复的 `ID`

1. 安装 `Masa.Contrib.Data.IdGenerator.Snowflake.Distributed.Redis`

   ```shell 终端
   dotnet add package Masa.Contrib.Data.IdGenerator.Snowflake.Distributed.Redis
   ```

2. 注册雪花 `ID` 生成器，并使用 `Reids`

   ```csharp Program.cs
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddSnowflake(distributedIdGeneratorOptions =>
   {
       distributedIdGeneratorOptions.UseRedis(
           option => option.GetWorkerIdMinInterval = 5000,
           redisOptions =>
           {
               redisOptions.Servers = new List<RedisServerOptions>()
               {
                   new("localhost", 6379)
               };
           });
   });
   ```

### 配置

#### 基础

| 参数名             | 描述                                                      | 详细                                                                                     |
| ------------------ | --------------------------------------------------------- |----------------------------------------------------------------------------------------|
| BaseTime           | 基准时间，小于当前时间（时区：UTC +0）                    | 建议选用现在更近的固定时间，一经使用，不可更变（更改可能导致: 重复 `ID`）                                               |
| SequenceBits       | 序列号, 默认: **12**，支持0-4095 (2^12-1)                 | 每毫秒每个工作机器最多产生 4095 个请求                                                                 |
| WorkerIdBits       | 工作机器id，默认: **10**，支持0-1023个机器 (2^10-1)       | 默认不支持在 `k8s` 集群中使用，在一个 `Pod` 中多副本获取到的 `WorkerId` 是一样的，可能会出现重复 `ID`                     |
| EnableMachineClock | 启用时钟锁，默认: **false**                               | 启用时钟锁后，生成的 `ID` 不再与当前时间有绝对关系，生成的 `ID` 以项目启动时的时间作为初始时间，项目运行后时钟回拨不会影响 `ID` 的生成           |
| TimestampType      | 时间戳类型，默认: **1** (毫秒: `Milliseconds`, 秒: `Seconds`) | `TimestampType` 为 `Milliseconds` 时，`SequenceBits` + `WorkerIdBits` 最大长度为22             |
| MaxCallBackTime    | 最大回拨时间，默认: 3000 (毫秒)                           | 当不启用时钟锁时，如果出现时间回拨小于 `MaxCallBackTime`，则会等待时间大于最后一次生成id的时间后，再次生成 `ID`，如果大于最大回拨时间，则会抛出异常 |

> WorkerId的值默认从环境变量`WORKER_ID`中获取，如未设置则会返回0 （多机部署时请确保每个服务的WorkerId是唯一的）

| 参数名             | 描述                                                         | 详细                                                                                                        |
| ------------------ | ------------------------------------------------------------ |-----------------------------------------------------------------------------------------------------------|
| SupportDistributed | 支持分布式部署，默认: **false** (由 `WorkerId` 的提供类库赋值) |                                                                                                           |
| HeartbeatInterval  | 心跳周期，默认: **3000ms（3s）**                             | 用于定期检查刷新服务的状态，确保 `WorkerId` 不会被回收                                                                           |
| MaxExpirationTime  | 最大过期时间: 默认: **10000ms（10s）**                       | 当刷新服务状态失败时，检查当前时间与第一次刷新服务失败的时间差超过最大过期时间后，主动放弃当前的 `WorkerId` ，并拒绝提供生成 `ID` 的服务，直到可以获取到新的 `WokerId` 后再次提供服务 |

#### 分布式雪花id（Redis）


| 参数名             | 描述                                                      | 详细                                                                                                                                                                                                    |
| ------------------ | --------------------------------------------------------- |-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|IdleTimeOut|闲置回收时间，默认: **120000ms (2min)**| 当无可用的 `WorkerId` 后会尝试从历史使用的 `WorkerId` 集合中获取活跃时间超过 `IdleTimeOut` 的 `WorkerId` ，并选取距离现在最远的一个 `WorkerId` 进行复用                                                                                           |
|GetWorkerIdMinInterval|获取WorkerId的时间间隔，默认: **5000ms (5s)**| 当前 `WorkerId` 可用时，会将 `WorkerId` 直接返回，不会有任何限制<br/>当服务刷新 `WorkerId` 失败，并持续时间超过指定时间后，会自动释放 `WorkerId`，当再次获取新的`ID` 时，会尝试重新获取新的 `WorkerId`，若最近一次获取`WorkerId` 时间与当前时间小于 `GetWorkerIdMinInterval` 时，会被拒绝提供服务 |
|RefreshTimestampInterval|默认 **500ms**| 选择启用时钟锁后，当获取到下次的时间戳与最近一次的时间戳超过 `RefreshTimestampInterval` 时，会将当前的时间戳与 `WorkerId` 对应关系保存在 `Redis` 中，用于后续继续使用，减少对当前系统时间的依赖                                                                              |
## 原理解剖

雪花id由4部分组成：

![image-20230424165654610](https://cdn.masastack.com/framework/202304241656648.png)

而时间戳、序列号均存在重复可能性，但为了保证 `ID` 不重复，因此需要确保工作机器 `ID` 不重复才可以达成目标，通常情况下会借助 外部存储用于记录当前剩余有效的工作机器编号，而 `MASA Framework` 默认提供支持分布式雪花 `ID` 使用的是基于 `Redis` 服务来完成的

## 性能测试

1. `TimestampType` 为1（毫秒）

   ```shell
   `BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1023 (21H1/May2021Update)
   11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
   .NET SDK=7.0.100-preview.4.22252.9
   [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT DEBUG
   Job-JPQDWN : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
   Job-BKJUSV : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
   Job-UGZQME : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT`
   
   `Runtime=.NET 6.0  RunStrategy=ColdStart`
   ```

   | Method                 | Job        | IterationCount |       Mean |     Error |     StdDev |     Median |        Min |          Max |
   | ---------------------- | ---------- | -------------- | ---------: | --------: | ---------: | ---------: | ---------: | -----------: |
   | SnowflakeByMillisecond | Job-JPQDWN | 1000           | 2,096.1 ns | 519.98 ns | 4,982.3 ns | 1,900.0 ns | 1,000.0 ns | 156,600.0 ns |
   | SnowflakeByMillisecond | Job-BKJUSV | 10000          |   934.0 ns |  58.44 ns | 1,775.5 ns |   500.0 ns |   200.0 ns | 161,900.0 ns |
   | SnowflakeByMillisecond | Job-UGZQME | 100000         |   474.6 ns |   5.54 ns |   532.8 ns |   400.0 ns |   200.0 ns | 140,500.0 ns |

2. `TimestampType` 为 2（秒）

   ```shell
   `BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1023 (21H1/May2021Update)
   11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
   .NET SDK=7.0.100-preview.4.22252.9
   [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
   Job-RVUKKG : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
   Job-JAUDMW : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
   Job-LOMSTK : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT`
   
   `Runtime=.NET 6.0  RunStrategy=ColdStart`
   ```

   
   |            Method |        Job | IterationCount |      Mean |      Error |       StdDev |    Median |       Min |          Max |
   |------------------ |----------- |--------------- |----------:|-----------:|-------------:|----------:|----------:|-------------:|
   | SnowflakeBySecond | Job-RVUKKG |           1000 |  1.882 us |  0.5182 us |     4.965 us | 1.5000 us | 0.9000 us |     158.0 us |
   | SnowflakeBySecond | Job-JAUDMW |          10000 | 11.505 us | 35.1131 us | 1,066.781 us | 0.4000 us | 0.3000 us | 106,678.8 us |
   | SnowflakeBySecond | Job-LOMSTK |         100000 | 22.097 us | 15.0311 us | 1,444.484 us | 0.4000 us | 0.2000 us | 118,139.7 us |

3. `TimestampType` 为1（毫秒）、启用时钟锁

   ```shell
   `BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1023 (21H1/May2021Update)
   11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
   .NET SDK=7.0.100-preview.4.22252.9
   [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
   Job-BBZSDR : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
   Job-NUSWYF : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
   Job-FYICRN : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT`
   
   `Runtime=.NET 6.0  RunStrategy=ColdStart`
   ```

   | Method                    | Job        | IterationCount |       Mean |     Error |     StdDev |     Median |         Min |          Max |
   | ------------------------- | ---------- | -------------- | ---------: | --------: | ---------: | ---------: | ----------: | -----------: |
   | MachineClockByMillisecond | Job-BBZSDR | 1000           | 1,502.0 ns | 498.35 ns | 4,775.1 ns | 1,100.0 ns | 700.0000 ns | 151,600.0 ns |
   | MachineClockByMillisecond | Job-NUSWYF | 10000          |   602.0 ns |  54.76 ns | 1,663.7 ns |   200.0 ns | 100.0000 ns | 145,400.0 ns |
   | MachineClockByMillisecond | Job-FYICRN | 100000         |   269.8 ns |   5.64 ns |   542.4 ns |   200.0 ns |   0.0000 ns | 140,900.0 ns |

<app-alert type="warning" content="雪花 `ID` 严重依赖时间，哪怕是启用时钟锁后，项目在启动时仍然需要获取一次当前时间作为基准时间，如果获取到的初始获取时间为已经过期的时间，那生成的 `ID` 仍然有重复的可能，因此需确保时间是正确的"></app-alert>
