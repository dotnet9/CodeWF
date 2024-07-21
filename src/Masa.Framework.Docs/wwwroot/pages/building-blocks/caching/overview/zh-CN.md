# Caching（缓存）- 概述

缓存用于提升数据的访问速度，一般我们会在以下场景使用到它：
   * 不经常变动但访问频繁的数据。例如：系统中的权限数据

MASA Framework 中的缓存组件提供[分布式缓存](/framework/building-blocks/caching/stackexchange-redis)、[多级缓存](/framework/building-blocks/caching/multilevel-cache)的能力。并且分布式缓存和多级缓存有抽象的构建块和默认的实现，我们的程序只需要依赖抽象的构建块就可以了，默认的实现可以自主选择替换。

## 最佳实践

* 分布式缓存：
    * [Redis 缓存](/framework/building-blocks/caching/stackexchange-redis): 基于[StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)实现的分布式缓存
    * 更多……（敬请期待）
* [多级缓存](/framework/building-blocks/caching/multilevel-cache) : 基于分布式缓存 + 内存缓存实现的多级缓存，主要是在分布式缓存和系统中间在增加一层内存缓存，相比分布式缓存而言，它减少了一次网络传入与反序列化的消耗, 具有更好的性能优势

