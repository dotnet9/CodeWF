# 事件总线 - 概述

事件总线是一种事件发布/订阅结构，通过发布订阅模式可以解耦不同架构层级, 它提供了一种松耦合的通信方式, 我们可以使用它来解决业务之间的耦合

## 最佳实践

根据事件类型我们将事件总线划分为:

* [进程内事件总线](/framework/building-blocks/dispatcher/local-event)
* [集成事件总线](/framework/building-blocks/dispatcher/integration-event)