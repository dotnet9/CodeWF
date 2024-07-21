# 隔离性 - 概述

MASA Framework 提供了隔离性的功能，它支持了 **物理隔离**、**逻辑隔离** 两种方案

> 隔离性需要与 **数据**、**缓存**、**存储** 等构建块共同使用，单独使用没有意义

## 最佳实践

* [多租户](/framework/building-blocks/isolation/multi-tenant)：提供了创建多租户应用程序的基本功能
* [多环境](/framework/building-blocks/isolation/multi-environment)：提供了创建多环境应用程序的基本功能

## 能力介绍

| 最佳实践                                                          | 数据上下文  | 缓存  | 存储  | 调用者 | 规则引擎 | 自动补全 | 集成事件 | 配置 |
|:--------------------------------------------------------------|:------:|:---:|:---:|:---:|:--:|:----: |:----: |:----: |
| [多租户](/framework/building-blocks/isolation/multi-tenant)      |   ✅    |  ✅  |  ✅  |  ✅  |  ✅ | ✅ | - | - |
| [多环境](/framework/building-blocks/isolation/multi-environment) |   ✅    |  ✅ |  ✅  |  ✅  |  ✅ | ✅ | - | - |

> ✅：支持。 ❌：不支持。 -：暂不支持。
