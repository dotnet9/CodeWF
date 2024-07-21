# 读写分离

## 概述

[CQRS](https://learn.microsoft.com/zh-cn/azure/architecture/patterns/cqrs)是一种与领域驱动设计和事件溯源相关的架构模式，它将事件（Event）划分为 命令端（Command）和 查询端（Query）

* 命令端:
   * 关注各种业务如何处理，更新状态进行持久化
   * 不返回任何结果（void）
* 查询端:
   * 仅做查询操作

![image-20230428110923203](https://cdn.masastack.com/framework/framework/building-blocks/cqrs.png)

通过 `MASA Framework` 提供的 [事件总线](/framework/building-blocks/dispatcher/overview)，我们可以很轻松的实现 `CQRS` 模式。根据业务需求，我们可以创建并维护读模型，将读操作和写操作进行分离，从而提高应用程序的可扩展性和性能。

## 使用

1. 安装 `Masa.BuildingBlocks.Dispatcher.Events`、`Masa.Contrib.ReadWriteSplitting.Cqrs`

   ```shell 终端
   dotnet add package Masa.Contrib.Dispatcher.Events
   dotnet add package Masa.Contrib.ReadWriteSplitting.Cqrs
   dotnet add package Masa.Contrib.Service.MinimalAPIs
   ```

2. 注册 **事件总线**

   ```csharp
   builder.Services.AddEventBus();
   ```

3. 新增加 **命令**、**查询** 事件

   :::: code-group
   ::: code-group-item CreateProductCommand

   ```csharp Application/Catalogs/Commands/CreateProductCommand.cs
   using Masa.BuildingBlocks.ReadWriteSplitting.Cqrs.Commands;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   
   public record CreateProductCommand : Command
   {
       public string Name { get; set; } = default!;
   
       /// <summary>
       /// seed data：31b1c60b-e9c3-4646-ac70-09354bdb1522
       /// </summary>
       public Guid CatalogBrandId { get; set; }
   
       /// <summary>
       /// seed data：1
       /// </summary>
       public int CatalogTypeId { get; set; } 
   
       public decimal Price { get; set; }
   
       public string? PictureFileName { get; set; }
   
       public int Stock { get; set; }
   }
   ```
   :::
   ::: code-group-item ProductsQuery

   ```csharp Application/Catalogs/Queries/ProductsQuery.cs
   using Masa.BuildingBlocks.ReadWriteSplitting.Cqrs.Queries;
   using Masa.EShop.Contracts.Catalog.Dto;
   using Masa.Utils.Models;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs.Queries;
   
   public record ProductsQuery : Query<PaginatedListBase<CatalogListItemDto>>
   {
       public int PageSize { get; set; } = default!;
   
       public int Page { get; set; } = default!;
   
       public string? Name { get; set; }
   
       public bool IsRecycle { get; set; } = false;
   
       public override PaginatedListBase<CatalogListItemDto> Result { get; set; } = default!;
   }
   ```
   :::
   ::::

4. 新增加 **命令端**  处理程序、**查询端** 处理程序

   :::: code-group
   ::: code-group-item ProductCommandHandler

   ```csharp Application/Catalogs/ProductCommandHandler.cs l:11-16
   using Masa.BuildingBlocks.Caching;
   using Masa.BuildingBlocks.Ddd.Domain.Repositories;
   using Masa.Contrib.Dispatcher.Events;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   using Masa.EShop.Service.Catalog.Domain.Entities;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs;
   
   public class ProductCommandHandler
   {
       [EventHandler]
       public Task CreateHandleAsync(CreateProductCommand command)
       {
           //todo: 创建商品处理逻辑 
           return Task.CompletedTask;
       }
   }
   ```

   :::
   ::: code-group-item ProductQueryHandler

   ```csharp Application/Catalogs/ProductQueryHandler.cs l:16-23
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
       [EventHandler]
       public Task ProductsHandleAsync(ProductsQuery query, IDataFilter dataFilter)
       {
           //todo: 根据业务得到查询结果并将结果赋值给 Result
           
           query.Result = new PaginatedListBase<CatalogListItemDto>();
           return Task.CompletedTask;
       }
   }
   ```

   :::
   ::::

5. 发布 **命令**、**查询** 事件

   :::: code-group
   ::: code-group-item 发布 命令

   ```csharp Services/CatalogItemService.cs l:10,14
   using Masa.BuildingBlocks.Data;
   using Masa.BuildingBlocks.Dispatcher.Events;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Queries;
   
   namespace Masa.EShop.Service.Catalog.Services;
   
   public class CatalogItemService : ServiceBase
   {
       private IEventBus EventBus => GetRequiredService<IEventBus>();
   
       public async Task<IResult> CreateProductAsync(CreateProductCommand command)
       {
           await EventBus.PublishAsync(command);
           return Results.Accepted();
       }
   }
   ```
   :::
   ::: code-group-item 发布 查询

   ```csharp Services/CatalogItemService.cs l:10,23
   using Masa.BuildingBlocks.Data;
   using Masa.BuildingBlocks.Dispatcher.Events;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Queries;
   
   namespace Masa.EShop.Service.Catalog.Services;
   
   public class CatalogItemService : ServiceBase
   {
       private IEventBus EventBus => GetRequiredService<IEventBus>();
   
       public async Task<IResult> GetItemsAsync(
           string? name,
           int page = 1,
           int pageSize = 10)
       {
           var query = new ProductsQuery()
           {
               Name = name,
               Page = page,
               PageSize = pageSize
           };
           await EventBus.PublishAsync(query);
           return Results.Ok(query.Result);
       }
   }
   ```
   :::
   ::::

## 相关链接

* [.NET现代化应用开发 - CQRS&类目管理代码剖析](https://www.bilibili.com/video/BV1D24y1R7jE/?spm_id_from=333.788&vd_source=63b84556cca0923b5818e72403993eb2)