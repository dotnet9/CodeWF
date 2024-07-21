# 缓存 - 分布式 Redis 缓存

## 概述

什么是[`分布式缓存`](https://learn.microsoft.com/zh-cn/aspnet/core/performance/caching/distributed)

## 使用

1. 安装 `Masa.Contrib.Caching.Distributed.StackExchangeRedis`

   ```shell 终端
   dotnet add package Masa.Contrib.Caching.Distributed.StackExchangeRedis
   ```

2. 添加 `Redis` 的配置信息

   ```json appsettings.json
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

3. 注册分布式缓存，并使用 Redis 缓存

   ```csharp Program.cs
   builder.Services.AddDistributedCache(distributedCacheOptions =>
   {
       distributedCacheOptions.UseStackExchangeRedisCache();//使用分布式 Redis 缓存，默认使用本地 `RedisConfig` 节点的配置
   });
   ```

4. 使用分布式缓存，在构造函数中注入 `IDistributedCacheClient` 对象

   ```csharp
   [ApiController]
   [Route("[controller]/[action]")]
   public class HomeController : ControllerBase
   {
       private readonly IDistributedCacheClient _distributedCacheClient;
       public HomeController(IDistributedCacheClient distributedCacheClient) => _distributedCacheClient = distributedCacheClient;
   
       [HttpGet]
       public async Task<string?> GetAsync()
       {
           var cacheData = await _distributedCacheClient.GetAsync<string>("key");
           if (string.IsNullOrEmpty(cacheData))
           {
               cacheData = "value";
               await _distributedCacheClient.SetAsync<string>("key", "value");
           }
   
           return cacheData;
       }
   }
   ```

## 高阶用法

### 分布式缓存 Redis注册方式

我们提供了多种方法来初始化 Redis 的配置。我们推荐采用 **选项模式** 使用 `Configure<RedisConfigurationOptions>` 来设置 Redis 的配置信息。

#### 通过选项模式注册

> 我们还可以借助 [`MasaConfiguration`](/framework/building-blocks/configuration/overview) 完成选项模式支持

:::: code-group
::: code-group-item 1. 支持选项模式

```csharp Program.cs
builder.Services.Configure<RedisConfigurationOptions>(redisConfigurationOptions =>
{
    redisConfigurationOptions.Servers = new List<RedisServerOptions>()
    {
        new("localhost", 6379)
    };
    redisConfigurationOptions.DefaultDatabase = 3;
    redisConfigurationOptions.GlobalCacheOptions = new CacheOptions()
    {
        CacheKeyType = CacheKeyType.None
    };
});
```
:::
::: code-group-item 2. 注册分布式 Redis 缓存
```csharp Program.cs
builder.Services.AddDistributedCache(distributedCacheOptions =>
{
    distributedCacheOptions.UseStackExchangeRedisCache();
});
```
:::
::::

#### 通过本地配置文件注册

在指定的本地配置文件中的指定节点配置 Redis 信息, 完成注册

> 如果本地配置的指定节点下不存在 Redis 配置, 则仍然尝试从 `IOptionsMonitor<RedisConfigurationOptions>` 获取, 如果获取失败则使用 `localhost:6379`

:::: code-group
::: code-group-item 1. 修改本地配置文件
```json appsettings.json
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
:::
::: code-group-item 2. 注册分布式 Redis 缓存
```csharp Program.cs
builder.Services.AddDistributedCache(distributedCacheOptions =>
{
    distributedCacheOptions.UseStackExchangeRedisCache();
});
```
:::
::::

#### 通过委托指定 Redis 配置注册

```csharp Program.cs
builder.Services.AddDistributedCache(distributedCacheOptions =>
{
    distributedCacheOptions.UseStackExchangeRedisCache(options =>
    {
        options.Servers = new List<RedisServerOptions>()
        {
            new("localhost", 6379)
        };
        options.DefaultDatabase = 3;
        options.GlobalCacheOptions = new CacheOptions()
        {
            CacheKeyType = CacheKeyType.None //可以全局禁用缓存Key格式化处理
        };
    });
});
```

#### 通过指定 Configuration 注册

:::: code-group
::: code-group-item 1. 修改appsettings.json
```json appsettings.json
{
    "RedisConfig":{
        "Servers":[
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
::: code-group-item 2. 指定 Configuration 注册分布式 Redis 缓存
```csharp Program.cs
builder.Services.AddDistributedCache(distributedCacheOptions =>
{
    distributedCacheOptions.UseStackExchangeRedisCache(builder.Configuration.GetSection("RedisConfig"));
});
```
:::
::::

### Redis 配置参数

* `RedisConfigurationOptions` 类

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
       <td colspan=3>AbsoluteExpiration</td>
       <td colspan=2>绝对过期时间：到期后就失效</td>
       <td>DateTimeOffset?</td>
       <td>null (永不过期)</td>
      </tr>
      <tr>
       <td colspan=3>AbsoluteExpirationRelativeToNow</td>
       <td colspan=2>相对于现在的绝对到期时间 (与AbsoluteExpiration共存时，优先使用AbsoluteExpirationRelativeToNow)</td>
       <td>TimeSpan?</td>
       <td>null (永不过期)</td>
      </tr>
      <tr>
       <td colspan=3>SlidingExpiration</td>
       <td colspan=2>滑动过期时间：只要在窗口期内访问，它的过期时间就一直向后顺延一个窗口长度</td>
       <td>TimeSpan?</td>
       <td>null (永不过期)</td>
      </tr>
      <tr>
       <td colspan=3>AbortOnConnectFail</td>
       <td colspan=2>是否应通过 TimeoutException 显式通知连接/配置超时</td>
       <td>bool</td>
       <td>false</td>
      </tr>
      <tr>
       <td colspan=3>AllowAdmin</td>
       <td colspan=2>是否应允许管理操作</td>
       <td>bool</td>
       <td>false</td>
      </tr>
      <tr>
       <td colspan=3>AsyncTimeout</td>
       <td colspan=2>允许异步操作的时间 (ms)</td>
       <td>int</td>
       <td>5000</td>
      </tr>
      <tr>
       <td colspan=3>ClientName</td>
       <td colspan=2>用于所有连接的客户端名称</td>
       <td>string</td>
       <td>空字符串</td>
      </tr>
      <tr>
       <td colspan=3>ChannelPrefix</td>
       <td colspan=2>自动编码和解码频道</td>
       <td>string</td>
       <td>空字符串</td>
      </tr>
      <tr>
       <td colspan=3>ConnectRetry</td>
       <td colspan=2>链接重试</td>
       <td>int</td>
       <td>3</td>
      </tr>
      <tr>
       <td colspan=3>ConnectTimeout</td>
       <td colspan=2>链接超时 (ms)</td>
       <td>int</td>
       <td>5000</td>
      </tr>
      <tr>
       <td colspan=3>DefaultDatabase</td>
       <td colspan=2>默认数据库</td>
       <td>int</td>
       <td>0</td>
      </tr>
      <tr>
       <td colspan=3>Password</td>
       <td colspan=2>密码</td>
       <td>string</td>
       <td>空字符串</td>
      </tr>
      <tr>
       <td colspan=3>Proxy</td>
       <td colspan=2>代理</td>
       <td>Enum</td>
       <td>0</td>
      </tr>
      <tr>
       <td colspan=3>Ssl</td>
       <td colspan=2>是否应加密连接</td>
       <td>bool</td>
       <td>false</td>
      </tr>
      <tr>
       <td colspan=3>SyncTimeout</td>
       <td colspan=2>允许同步操作的时间 (ms)</td>
       <td>int</td>
       <td>5000</td>
      </tr>
      <tr>
       <td colspan=3>GlobalCacheOptions</td>
       <td colspan=2>缓存全局配置</td>
       <td> </td>
      </tr>
      <tr>
       <td rowspan=1></td>
       <td colspan=2>CacheKeyType（<a href='#section-7f135b58key7684914d7f6e8bf4660e'>查看详情</a>）</td>
       <td colspan=2>缓存Key类型</td>
       <td>None、TypeName、TypeAlias</td>
       <td>TypeName</td>
      </tr>
      <tr>
       <td colspan=3>Servers（redis 配置集合）</td>
       <td colspan=2>Redis配置信息</td>
       <td> </td>
      </tr>
      <tr>
       <td rowspan=12></td>
       <td colspan=2>Host</td>
       <td colspan=2>ip地址</td>
       <td>string</td>
       <td>localhost</td>
      </tr>
      <tr>
       <td colspan=2>Port</td>
       <td colspan=2>端口</td>
       <td>int</td>
       <td>6379</td>
      </tr>
     </table>
   </div>

### 缓存Key的规则

在MASA Framework中缓存组件中，我们会给缓存 Key 默认增加前缀（也可以手动配置取消）：前缀+key，这么做的好处有：
1. 提高易用性：不同系统中缓存相同 id 作为 Key，但是数据却不同的时候，我们给每个 Key 增加数据类型前缀，就可以达到互不干扰的作用。

#### 缓存Key的配置说明

* None: 1 (不处理，不对传入的 Key 作任何处理)
* TypeName: 2 (由缓存值的类型与传入缓存 Key 组合而成 **默认**)
    * 实际的缓存 Key = $"{GetTypeName(T)}.{传入缓存Key}"
* TypeAlias: 3 (TypeName 的升级版, 为每个 TypeName 指定 `别名`, 缩减最后形成的 `缓存Key` 长度)
    * 实际的缓存 Key = ${TypeAliasName}{:}{key}


#### 缓存 Key 规则优先级

自定义规则 > 全局缓存 Key 规则。当我们使用 Get 或 Set 方法是主动传入 Key 的规则时，则以传入的规则为主，这个时候全局 Key 规则无效。

1. 注册分布式缓存时指定`CacheKeyType`为`None`

注册分布式缓存指定的 `CacheKeyType` 为全局缓存 Key 规则, 设置使用当前分布式缓存客户端时, 默认传入的缓存 key 即为最终的缓存 Key

```csharp Program.cs
builder.Services.AddDistributedCache(distributedCacheOptions =>
{
    distributedCacheOptions.UseStackExchangeRedisCache(options =>
    {
        options.Servers = new List<RedisServerOptions>()
        {
            new("localhost", 6379)
        };
        options.DefaultDatabase = 3;
        options.ConnectionPoolSize = 10;
        options.GlobalCacheOptions = new CacheOptions()
        {
            CacheKeyType = CacheKeyType.None //可以全局禁用缓存Key格式化处理
        };
    });
});
```

2. 为当前调用使用指定缓存 Key 规则

```csharp Program.cs
app.MapGet("/get/{id}", async (IDistributedCacheClient distributedCacheClient, string id) =>
{
    var value = await distributedCacheClient.GetAsync<User>(id, options =>
    {
        options.CacheKeyType = CacheKeyType.TypeName;
    });
    return Results.Ok(value);
});
```

> 虽然设置了全局缓存 Key 规则为 `None`, 但由于当前方法指定了缓存 Key 规则, 则当前方法使用的全局缓存 Key 为: TypeName, 即最终的缓存 Key 为: $"{GetTypeName(T)}.{传入缓存Key}"

### 更多使用

#### 使用滑动过期
```csharp Program.cs
builder.Services.AddDistributedCache(opt =>
{
    opt.UseStackExchangeRedisCache(redisOptions =>
    {
        redisOptions.Servers = new List<RedisServerOptions>()
        {
            new("localhost", 6379)
        };
        //设置 key 的有效期是30分钟，超过30分钟缓存过期
        redisOptions.AbsoluteExpirationRelativeToNow= TimeSpan.FromMinutes(30);
        //设置滑动过期时间为10分钟，当该缓存Key在10分钟之内被访问了，则继续延长10分钟 （直到达到绝对过期时间后被删除）。若该缓存key在10分钟内未被访问，则会被移除。
        redisOptions.SlidingExpiration= TimeSpan.FromMinutes(10);
    });
});
```

#### 使用PubSub

分布式缓存还提供了 Pub/Sub 功能，如果我们的系统中需要轻量的发布订阅，那么则可以考虑使用它。

> 分布式缓存中的 Pub/Sub 功能，存在消息丢失的可能。如果发布一个消息没有订阅者，那么这个消息就会丢失。

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class HomeController : ControllerBase
{
    private readonly IDistributedCacheClient _distributedCacheClient;
    public HomeController(IDistributedCacheClient distributedCacheClient) => _distributedCacheClient = distributedCacheClient;

    [HttpGet]
    public async Task StartSubscribe()
    {
        await _distributedCacheClient.SubscribeAsync<string>("channel", opt =>
        {
            Console.WriteLine($"数据发生了改变：{opt.Value}");
        });
    }

    [HttpPost]
    public async Task Publish()
    {
        await _distributedCacheClient.PublishAsync("channel", opt =>
        {
            opt.Value = "new value";
            opt.Key = "key";
            opt.Operation = SubscribeOperation.Set;
        });
    }
}
```

## 原理剖析

### 滑动过期原理

MASA Framework 的分布式缓存是支持滑动过期的，但是 Redis 缓存仅支持绝对过期。那么这个滑动过期是怎么实现的呢？主要是通过以下三个步骤：

1. 数据类型改为 `Hash` 类型
2. 将绝对过期时间与滑动过期时间存储
3. 每次获取数据时会根据绝对过期时间与相对过期时间取最小值, 并通过 `EXPIRE` 为给定 `Key` 设置过期时间

简单来说：当设置缓存的时候，过期时间为绝对过期时间与滑动过期时间中取最小值，每次获取缓存数据的时候重新计算过期时间。因此当缓存超过设置的滑动过期时间后, 缓存会被删除, 当在滑动过期时间内时, 会重新计算过期时间并更新。

|  Hash 字段   | 描述  | 详细  | 特殊 |
|  ----  | ----  | ----  | ----  |
|   absexp    | 绝对过期时间的[Ticks](https://learn.microsoft.com/zh-cn/dotnet/api/system.datetime.ticks?view=net-6.0) | 自公历 `0001-01-01 00:00:00:000` 到绝对过期时间的计时周期数 (1周期 = 100ns 即 1/10000 ms) | -1 为永不过期 |
|   sldexp   | 滑动过期时间的[Ticks](https://learn.microsoft.com/zh-cn/dotnet/api/system.datetime.ticks?view=net-6.0)  |  自公历 `0001-01-01 00:00:00:000` 到滑动过期时间的计时周期数 (1周期 = 100ns 即 1/10000 ms，每次获取数据时会刷新滑动过期时间) | -1 为永不过期 |
|   data   | 数据 | 存储用户设置的缓存数据 |

### 内容压缩规则

在 MASA Framework 的分布式缓存中，我们会对数据进行压缩存储。但是需要注意以下事项：

1. 当存储值类型为以下类型时，不对数据进行压缩：

* Byte
* SByte
* UInt16
* UInt32
* UInt64
* Int16
* Int32
* Int64
* Double
* Single
* Decimal

2. 当存储值类型为字符串时，对数据进行压缩
3. 当存储值类型不满足以上条件时，对数据进行序列化并进行压缩