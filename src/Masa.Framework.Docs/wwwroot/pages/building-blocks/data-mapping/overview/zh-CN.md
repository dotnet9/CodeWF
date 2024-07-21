# Data Mapping（数据映射）- 概述

提供对象映射的能力，通过添加提供者的引用并注册，即可轻松完成对象映射的能力

## 最佳实践

* [Mapster](/framework/building-blocks/data-mapping/mapster): 基于 [Mapster](https://github.com/MapsterMapper/Mapster) 的扩展，轻松完成对象映射的能力

## 源码解读

提供了映射的抽象 `IMapper`，它支持：

* Map：根据目标类型将源类型对象转换为目标类型并返回
