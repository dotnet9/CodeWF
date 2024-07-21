# 分布式锁 - 概述

分布式锁是一种管理多个应用程序访问同一资源的技术，主要目的是防止因同一时间多个应用程序访问资源导致并发从而出现数据不一致的情况。

## 最佳实践

目前分布式锁有两个实现：

* [`Local`](/framework/building-blocks/distributed-lock/lock-local)：基于 [`SemaphoreSlim`](https://learn.microsoft.com/zh-cn/dotnet/api/system.threading.semaphoreslim) 实现的本地锁，建议在单个应用中使用
* [`Medallion`](/framework/contribs/data/distributed-lock/medallion)：基于 [DistributedLock](https://github.com/madelson/DistributedLock) 实现的分布式锁

尽管 [`Local`](/framework/building-blocks/distributed-lock/lock-local) 不支持分布式锁, 但它仍然是一个有用的实现

* 在开发或者测试场景中
* 当在生产环境中使用单个服务器时，但后期可能会使用多个服务器时，可通过快速更换注册分布式锁代码，从而实现真正的分布式锁

## 源码解读

`IDistributedLock` 是分布式锁抽象类, 它提供了

* TryGet：尝试获取锁
* TryGetAsync：尝试获取锁（异步）

其中 `Key` 是锁的唯一名称，不同的名称的锁可用于访问不同的资源，`timeout` 是等待获取锁的超时值，默认值为 `TimeSpan.Zero`（如果锁已经被另一个应用程序拥有，它不会等待，直接返回 `null`）