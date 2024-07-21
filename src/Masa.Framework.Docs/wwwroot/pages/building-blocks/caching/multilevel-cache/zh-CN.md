# 缓存 - 多级缓存

## 概述

多级缓存是指在一个系统的不同架构层级进行数据缓存，以提升访问效率。 MASA Framework 的多级缓存是在分布式缓存的基础上，再加了一层内存缓存。使用多级缓存，可以降低请求穿透到分布式缓存，减少网络消耗以及序列化带来的性能影响，使用它可以大大缩减响应时间。并且 MASA Framework 的多级缓存是支持分布式部署的，当缓存数据在集群的某个节点被更新或删除时，其它集群节点也会同步更新或删除缓存数据。[查看原因](#同步更新)

> 使用多级缓存的时候需要注意：当内存中的缓存数据特别多的时候，可能会导致内存超负荷，这个时候我们推荐使用缓存[滑动过期](#滑动过期)。

![多级缓存结构图](https://cdn.masastack.com/framework/building-blocks/cache/multilevel_design.png)

## 使用

因为多级缓存基于分布式缓存的，所以我们需要安装 `Masa.Contrib.Caching.MultilevelCache` 和任意一个分布式缓存提供者 (例：[Masa.Contrib.Caching.Distributed.StackExchangeRedis](./stackexchange-redis.md))

1. 安装 `Masa.Contrib.Caching.MultilevelCache`、`Masa.Contrib.Caching.Distributed.StackExchangeRedis`

   ```shell 终端
   dotnet add package Masa.Contrib.Caching.MultilevelCache
   dotnet add package Masa.Contrib.Caching.Distributed.StackExchangeRedis
   ```

2. 注册多级缓存，并使用 [`分布式 Redis 缓存`](./stackexchange-redis.md) 

   ```csharp Program.cs
   builder.Services.AddMultilevelCache(distributedCacheOptions =>
   {
       distributedCacheOptions.UseStackExchangeRedisCache();
   });
   ```
   
   > 使用分布式 Redis 缓存，默认 localhost:6379

3. 添加分布式缓存 `Redis` 的配置信息

   ```json appsettings.json l:2-10
   {
       "RedisConfig":{
           "Servers":[
               {
                   "Host":"localhost",
                   "Port":6379
               }
           ],
           "DefaultDatabase":3
       }
   }
   ```

4. 使用多级缓存，在构造函数中注入 `IMultilevelCacheClient` 对象

   ```csharp Controllers/HomeController.cs l:5-6,12,17
   [ApiController]
   [Route("[controller]/[action]")]
   public class HomeController : ControllerBase
   {
       private readonly IMultilevelCacheClient _multilevelCacheClient;
       public HomeController(IMultilevelCacheClient multilevelCacheClient) => _multilevelCacheClient = multilevelCacheClient;
   
       [HttpGet]
       public async Task<string?> GetAsync()
       {
           //read
           var cacheData = await _multilevelCacheClient.GetAsync<string>("key");
           if (string.IsNullOrEmpty(cacheData))
           {
               cacheData = "value";
               //write
               await _multilevelCacheClient.SetAsync<string>("key", "value");
           }
           return cacheData;
       }
   }
   ```

## 高阶用法

### 多级缓存注册方式

我们提供了多种方法来初始化多级缓存的配置。我们推荐采用 **选项模式** 使用 `Configure<MultilevelCacheOptions>` 来设置多级缓存的配置信息。

#### 选项模式

> 我们还可以借助 [`MasaConfiguration`](/framework/building-blocks/configuration/overview) 完成选项模式支持

:::: code-group
::: code-group-item 1. 支持选项模式
```csharp Program.cs
builder.Services.Configure<MultilevelCacheOptions>(options =>
{
    options.SubscribeKeyType = SubscribeKeyType.ValueTypeFullNameAndKey;
});
```
:::
::: code-group-item 2. 添加多级缓存并使用分布式 Redis 缓存
```csharp Program.cs
builder.Services.AddMultilevelCache(distributedCacheOptions =>
{
    distributedCacheOptions.UseStackExchangeRedisCache(redisConfigurationOptions =>
    {
        redisConfigurationOptions.Servers = new List<RedisServerOptions>()
        {
            new("localhost", 6379)
        };
        redisConfigurationOptions.DefaultDatabase = 3;
    });
});
```
:::
::::

#### 通过本地配置文件注册

:::: code-group
::: code-group-item 1. 修改本地配置文件
```json appsettings.json
{
  // 多级缓存全局配置，非必填
  "MultilevelCache": {
    "SubscribeKeyPrefix": "masa",//默认订阅方key前缀，用于拼接channel
    "SubscribeKeyType": 3, //默认订阅方key的类型，默认ValueTypeFullNameAndKey，用于拼接channel
    "CacheEntryOptions": {
      "AbsoluteExpirationRelativeToNow": "00:00:30",//绝对过期时长（距当前时间）
      "SlidingExpiration": "00:00:50"//滑动过期时长（距当前时间）
    }
  },

  // Redis分布式缓存配置
  "RedisConfig": {
    "Servers": [
      {
        "Host": "localhost",
        "Port": 6379
      }
    ],
    "DefaultDatabase": 3
  }
}
```
:::
::: code-group-item 2. 添加多级缓存并使用分布式 Redis 缓存
```csharp Program.cs
builder.Services.AddMultilevelCache(distributedCacheOptions =>
{
    distributedCacheOptions.UseStackExchangeRedisCache();
});
```
:::
::::

#### 手动指定配置

使用默认配置, 并指定 Redis 配置信息

```csharp Program.cs l:5-9
builder.Services.AddMultilevelCache(distributedCacheOptions =>
{
    distributedCacheOptions.UseStackExchangeRedisCache(redisConfigurationOptions =>
    {
        redisConfigurationOptions.Servers = new List<RedisServerOptions>()
        {
            new("localhost", 6379)
        };
        redisConfigurationOptions.DefaultDatabase = 3;
    });
});
```
   
   
### 多级缓存配置参数说明

<div class="custom-table">
  <table style='border-collapse: collapse;table-layout:fixed;width:100%'>
   <col span=6>
   <tr style="background-color:#f3f4f5; font-weight: bold">
    <td colspan=3>参数名</td>
    <td colspan=2>参数描述</td>
    <td>类型</td>
    <td>默认值</td>
   </tr>
   <tr>
    <td colspan=3>SubscribeKeyType</td>
    <td colspan=2>订阅Key规则 (生成订阅Channel)</td>
    <td>Enum</td>
    <td>2</td>
   </tr>
   <tr>
    <td colspan=3>SubscribeKeyPrefix</td>
    <td colspan=2>订阅Key前缀 (生成订阅Channel)</td>
    <td>string</td>
    <td>空字符串</td>
   </tr>
   <tr>
    <td colspan=3>CacheEntryOptions</td>
    <td colspan=2>内存缓存有效期</td>
    <td>object</td>
    <td></td>
   </tr>
  <tr>
    <td rowspan=12></td>
    <td colspan=2>AbsoluteExpiration</td>
    <td colspan=2>绝对过期时间：到期后就失效</td>
    <td>DateTimeOffset?</td>
    <td>null (永不过期)</td>
   </tr>
   <tr>
    <td colspan=2>AbsoluteExpirationRelativeToNow</td>
    <td colspan=2>相对于现在的绝对到期时间 (与AbsoluteExpiration共存时，优先使用AbsoluteExpirationRelativeToNow)</td>
    <td>TimeSpan?</td>
    <td>null (永不过期)</td>
   </tr>
   <tr>
    <td colspan=2>SlidingExpiration</td>
    <td colspan=2>滑动过期时间：只要在窗口期内访问，它的过期时间就一直向后顺延一个窗口长度</td>
    <td>TimeSpan?</td>
    <td>null (永不过期)</td>
   </tr>
  </table>
</div>

### 更多使用

#### 滑动过期

滑动过期可以通过全局配置和单次操作直接指定的方式使用，当全局配置和直接指定都设置了滑动过期时间的时，以直接指定的时间为准

1. 全局配置
```csharp Program.cs
builder.Services.AddMultilevelCache(opt =>
{
    opt.UseStackExchangeRedisCache(redisOptions =>
    {
        redisOptions.Servers = new List<RedisServerOptions>()
        {
            new("localhost", 6379)
        };
        //设置key的有效期是30分钟，超过30分钟缓存过期
        redisOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
        //设置滑动过期时间为10分钟，当该缓存Key在10分钟之内被访问了，则继续延长10分钟 （直到达到绝对过期时间后被删除）。若该缓存 key 在10分钟内未被访问，则会被移除。
        redisOptions.SlidingExpiration = TimeSpan.FromMinutes(10);
    });
});
```

2. 单次操作直接指定

在调用 `Set` 方法时候，指定缓存过期策略。

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class HomeController : ControllerBase
{
    private readonly IMultilevelCacheClient _multilevelCacheClient;
    public HomeController(IMultilevelCacheClient multilevelCacheClient) => _multilevelCacheClient = multilevelCacheClient;

    [HttpGet]
    public async Task<string?> GetAsync()
    {
        var cacheData = await _multilevelCacheClient.GetAsync<string>("key");
        if (string.IsNullOrEmpty(cacheData))
        {
            cacheData = "value";

            var options = new CacheEntryOptions
            {
                //设置key的有效期是30分钟，超过30分钟缓存过期
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                //设置滑动过期时间为10分钟，当该缓存Key在10分钟之内被访问了，则继续延长10分钟 （直到达到绝对过期时间后被删除）。若该缓存key在10分钟内未被访问，则会被移除。
                SlidingExpiration = TimeSpan.FromMinutes(10),
            };
            // 设置的时候指定分布式是缓存和内存缓存的过期策略
            await _multilevelCacheClient.SetAsync<string>("key", "value", options, options);
        }
        return cacheData;
    }
}
```


## 原理剖析

   ### 同步更新
   
   为何多级缓存可以实现缓存发生更新后, 其它副本会随之更新, 而不需要等待缓存失效后重新加载?
   
   多级缓存中使用了分布式缓存提供的 [Pub/Sub](/framework/building-blocks/caching/stackexchange-redis#使用PubSub) 能力
   
   ![多级缓存原理流程图](https://cdn.masastack.com/framework/building-blocks/cache/multilevel_cache.png)
