# 实战教程 - 第五章: 缓存

## 概述

本章将使用[多级缓存](/framework/building-blocks/caching/multilevel-cache)技术，相比[分布式缓存](https://learn.microsoft.com/zh-cn/aspnet/core/performance/caching/distributed)，它有着更好的数据读取能力

> 教程中分布式缓存使用的是 [Redis缓存](/framework/building-blocks/caching/stackexchange-redis)，请确保有可用的 Redis 缓存服务器以供使用



以查询商品为例：

<div>
  <img alt="query product" src="https://cdn.masastack.com/framework/tutorial/mf-part-5/caching.png"/>
</div>


## 开始

1. 选中 `Masa.EShop.Service.Catalog` 项目并安装 `Masa.Contrib.Caching.Distributed.StackExchangeRedis`、`Masa.Contrib.Caching.MultilevelCache`

   ```shell 终端
   dotnet add package Masa.Contrib.Caching.Distributed.StackExchangeRedis -v 1.0.0
   dotnet add package Masa.Contrib.Caching.MultilevelCache -v 1.0.0
   ```
   
2. 配置内存缓存的有效期及分布式 Redis 缓存

   ```json appsettings.json
   {
     "RedisConfig": {
       "Servers": [
         {
           "Host": "localhost",
           "Port": 6379
         }
       ],
       "DefaultDatabase": 0
     },
     "MultilevelCache": {
       "CacheEntryOptions": {
         "AbsoluteExpirationRelativeToNow": "00:05:00",
         "SlidingExpiration": "00:00:30"
       }
     }
   }
   ```

   > 以上代码块未显示除缓存之外的配置

3. 注册多级缓存

   ```csharp Program.cs
   using Masa.BuildingBlocks.Caching;
   
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services
       .AddMultilevelCache(cacheBuilder => cacheBuilder.UseStackExchangeRedisCache());
   
   -----Ignore the rest of the service registration-----
   
   var app = builder.AddServices();//`var app = builder.Build();` for projects not using MinimalAPis
   
   -----Ignore the use of middleware, Swagger, etc.-----
   
   app.Run();
   ```

4. 修改**创建产品**、**删除产品**、**查询产品**处理程序

   :::: code-group
   ::: code-group-item ProductCommandHandler (创建、删除产品)

   ```csharp Application/Catalogs/ProductCommandHandler.cs
   using Masa.BuildingBlocks.Caching;
   using Masa.BuildingBlocks.Dispatcher.Events;
   using Masa.EShop.Service.Catalog.Infrastructure;
   using Masa.Contrib.Dispatcher.Events;
   using Masa.EShop.Contracts.Catalog.IntegrationEvents;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   using Masa.EShop.Service.Catalog.Domain.Entities;
   using Microsoft.EntityFrameworkCore;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs;
   
   public class ProductCommandHandler
   {
       private readonly CatalogDbContext _dbContext;
       private readonly IEventBus _eventBus;
       private readonly IMultilevelCacheClient _multilevelCacheClient;
   
       public ProductCommandHandler(CatalogDbContext dbContext, IEventBus eventBus, IMultilevelCacheClient multilevelCacheClient)
       {
           _dbContext = dbContext;
           _eventBus = eventBus;
           _multilevelCacheClient = multilevelCacheClient;
       }
   
       [EventHandler]
       public async Task CreateHandleAsync(CreateProductCommand command)
       {
           var catalogItem = new CatalogItem()
           {
               Id = Guid.NewGuid(),
               CatalogBrandId = command.CatalogBrandId,
               CatalogTypeId = command.CatalogTypeId,
               Name = command.Name,
               PictureFileName = command.PictureFileName ?? "default.png",
               Price = command.Price
           };
   
           await _dbContext.Set<CatalogItem>().AddAsync(catalogItem);
           
           await _multilevelCacheClient.SetAsync(catalogItem.Id.ToString(), catalogItem);
   
           await _eventBus.PublishAsync(new CatalogCreatedIntegrationEvent()
           {
               Id = catalogItem.Id,
               Name = catalogItem.Name,
               PictureFileName = command.PictureFileName,
               CatalogBrandId = command.CatalogBrandId,
               CatalogTypeId = command.CatalogTypeId
           });
       }
   
   
       [EventHandler]
       public async Task DeleteHandlerAsync(DeleteProductCommand command)
       {
           var catalogItem =
               await _dbContext.Set<CatalogItem>().FirstOrDefaultAsync(item => item.Id == command.ProductId) ??
               throw new UserFriendlyException("Product doesn't exist");
           _dbContext.Set<CatalogItem>().Remove(catalogItem);
           
           await _multilevelCacheClient.RemoveAsync<CatalogItem>(catalogItem.Id.ToString());
       }
   }
   ```
   
   :::
   ::: code-group-item ProductQueryHandler (查询产品详情)
   
   ```csharp Application/Catalogs/ProductQueryHandler.cs
   using System.Linq.Expressions;
   using Masa.BuildingBlocks.Caching;
   using Masa.BuildingBlocks.Data;
   using Masa.Contrib.Dispatcher.Events;
   using Masa.EShop.Contracts.Catalog.Dto;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Queries;
   using Masa.EShop.Service.Catalog.Domain.Entities;
   using Masa.EShop.Service.Catalog.Infrastructure;
   using Masa.Utils.Models;
   using Microsoft.EntityFrameworkCore;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs;
   
   public class ProductQueryHandler
   {
       private readonly CatalogQueryDbContext _dbContext;
   
       public ProductQueryHandler(CatalogQueryDbContext dbContext)
       {
           _dbContext = dbContext;
       }
   
       // -------- Ignore get product list method --------
   
       [EventHandler]
       public async Task ProductHandleAsync(ProductQuery query, IMultilevelCacheClient multilevelCacheClient)
       {
           TimeSpan? memoryTimeSpan = null;
           var catalogItem = await multilevelCacheClient.GetOrSetAsync(query.ProductId.ToString(),
               async () =>
               {
                   var item = await _dbContext.Set<CatalogItem>()
                       .Where(item => item.Id == query.ProductId)
                       .Select(item => new CatalogItemDto()
                       {
                           Id = item.Id,
                           Name = item.Name,
                           Price = item.Price,
                           PictureFileName = item.PictureFileName,
                           CatalogTypeId = item.CatalogTypeId,
                           CatalogBrandId = item.CatalogBrandId
                       }).FirstOrDefaultAsync();
                   
                   memoryTimeSpan = item == null ? TimeSpan.FromSeconds(5) :TimeSpan.FromSeconds(60);
                   
                   return new CacheEntry<CatalogItemDto>(item);
               }, memoryOptions => memoryOptions.AbsoluteExpirationRelativeToNow = memoryTimeSpan);
   
           if (catalogItem == null)
               throw new UserFriendlyException("Product doesn't exist");
           query.Result = catalogItem;
       }
   }
   ```
   
   :::
   ::::
   
   > 获取详情：当数据库不存在当前产品时，内存缓存的有效期为`5s`，但当数据库存在当前产品时，内存缓存的有效期为`60s`

## 问题

1. 使用缓存后，获取缓存失败，陷入死循环，不使用缓存后正常

   暂不支持`.NET 7.0`及更高版本，此问题与[issues-560](https://github.com/masastack/MASA.Framework/issues/560)一致

## 总结

多级缓存技术将带来更加优秀的读取性能的提升，通过它不仅可以使得响应更快，并且可以降低热点数据对`Redis`服务器的压力