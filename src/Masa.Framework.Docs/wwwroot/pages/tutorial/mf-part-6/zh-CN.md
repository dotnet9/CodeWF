# 实战教程 - 第六章: 领域驱动设计

## 概述

本章将使用[领域驱动设计](/framework/building-blocks/ddd/overview)，并使用`MASA Framework`提供的基础设施帮助我们去改造项目

## 开始

1. 选中 `Masa.EShop.Service.Catalog` 项目并安装 `Masa.Contrib.Ddd.Domain`、`Masa.Contrib.Ddd.Domain.Repository.EFCore`、`Masa.Contrib.Data.UoW.EFCore`、`Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore`、`Masa.Contrib.Dispatcher.IntegrationEvents.Dapr`

   ```shell 终端
   dotnet add package Masa.Contrib.Ddd.Domain -v 1.0.0
   dotnet add package Masa.Contrib.Ddd.Domain.Repository.EFCore -v 1.0.0
   ```
   
2. 注册[领域事件](/framework/building-blocks/ddd/domain-event)、[仓储](/framework/building-blocks/ddd/repository)、工作单元

   :::: code-group
   ::: code-group-item 注册领域事件总线并使用仓储
   
   ```csharp Program.cs l:5-8
   using Masa.BuildingBlocks.Ddd.Domain.Repositories;
   
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddDomainEventBus(options =>
   {
       options.UseRepository<CatalogDbContext>();
   });
   
   -----Ignore the rest of the service registration-----
   
   var app = builder.AddServices();//`var app = builder.Build();` for projects not using MinimalAPis
   
   -----Ignore the use of middleware, Swagger, etc.-----
   
   app.Run();
   ```
   
   :::
   ::: code-group-item 可简写 （修改原注册事件总线）
   
   ```csharp Program.cs l:5-12
   using Masa.BuildingBlocks.Ddd.Domain.Repositories;
   
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddDomainEventBus(options =>
   {
       options
           .UseIntegrationEventBus(eventOptions => eventOptions.UseDapr().UseEventLog<CatalogDbContext>())
           .UseEventBus()
           .UseUoW<CatalogDbContext>()
           .UseRepository<CatalogDbContext>();
   });
   
   -----Ignore the rest of the service registration-----
   
   var app = builder.AddServices();//`var app = builder.Build();` for projects not using MinimalAPis
   
   -----Ignore the use of middleware, Swagger, etc.-----
   
   app.Run();
   ```
   
   :::
   ::::
   
   > 领域事件使用需要注册`本地事件`、`集成事件`


3. 修改商品品牌、商品类型、商品信息模型

   :::: code-group
   ::: code-group-item CatalogBrand

   ```csharp Domain/Entities/CatalogBrand.cs
   using Masa.BuildingBlocks.Data;
   using Masa.BuildingBlocks.Ddd.Domain.Entities.Full;
   
   namespace Masa.EShop.Service.Catalog.Domain.Entities;
   
   public class CatalogBrand : FullAggregateRoot<Guid, int>
   {
       public string Brand { get; set; }
   
       private CatalogBrand()
       {
       }
   
       public CatalogBrand(Guid? id, string brand) : this()
       {
           Id = id ?? IdGeneratorFactory.SequentialGuidGenerator.NewId();
           Brand = brand;
       }
   }
   ```

   :::
   ::: code-group-item CatalogType

   ```csharp Domain/Entities/CatalogType.cs
   namespace Masa.EShop.Service.Catalog.Domain.Entities;
   
   public class CatalogType
   {
       public int Id { get; set; }
   
       public string Name { get; set; } = null!;
   
       private CatalogType()
       {
       }
   
       public CatalogType(int id, string name) : this()
       {
           Id = id;
           Name = name;
       }
   }
   ```

   :::
   ::: code-group-item CatalogItem

   ```csharp Domain/Entities/CatalogItem.cs
   using Masa.BuildingBlocks.Data;
   using Masa.BuildingBlocks.Ddd.Domain.Entities.Full;
   using Masa.EShop.Service.Catalog.Domain.Events;
   
   namespace Masa.EShop.Service.Catalog.Domain.Entities;
   
   public class CatalogItem : FullAggregateRoot<Guid, int>
   {
       public string Name { get; private set; } = null!;
   
       public decimal Price { get; private set; }
   
       public string PictureFileName { get; private set; } = "";
   
       public int CatalogTypeId { get; private set; }
   
       public CatalogType CatalogType { get; private set; } = null!;
   
       public Guid CatalogBrandId { get; private set; }
   
       public CatalogBrand CatalogBrand { get; private set; } = null!;
   
       public int Stock { get; private set; }
   
       private CatalogItem()
       {
       }
   
       public CatalogItem(string name, decimal price, string pictureFileName) : this()
       {
           Id = IdGeneratorFactory.SequentialGuidGenerator.NewId();
           Name = name;
           Price = price;
           PictureFileName = pictureFileName;
           AddCatalogDomainIntegrationEvent();
       }
   
       private void AddCatalogDomainIntegrationEvent()
       {
           var catalogCreatedIntegrationDomainEvent = new CatalogCreatedIntegrationDomainEvent(this);
           this.AddDomainEvent(catalogCreatedIntegrationDomainEvent);
       }
   
       public void SetCatalogType(int catalogTypeId)
       {
           CatalogTypeId = catalogTypeId;
       }
   
       public void SetCatalogBrand(Guid catalogBrand)
       {
           CatalogBrandId = catalogBrand;
       }
   
       public void AddStock(int stock)
       {
           Stock += stock;
       }
   }
   ```

   :::
   ::::

   > `IdGeneratorFactory.SequentialGuidGenerator.NewId();`需安装并注册 [有序Guid生成器](/framework/building-blocks/id-generator/sequential-guid)

4. 新增集成领域事件并继承商品创建集成事件 `CatalogCreatedIntegrationEvent`

   ```csharp Domain/Events/CatalogCreatedIntegrationDomainEvent.cs
   using Masa.BuildingBlocks.Ddd.Domain.Events;
   using Masa.EShop.Contracts.Catalog.IntegrationEvents;
   using Masa.EShop.Service.Catalog.Domain.Entities;
   
   namespace Masa.EShop.Service.Catalog.Domain.Events;
   
   public record CatalogCreatedIntegrationDomainEvent : CatalogCreatedIntegrationEvent, IIntegrationDomainEvent
   {
       private readonly CatalogItem _catalog;
   
       private int? _catalogTypeId;
   
       public override int CatalogTypeId
       {
           get => _catalogTypeId ??= _catalog.CatalogTypeId;
           set => _catalogTypeId = value;
       }
   
       private Guid? _catalogBrandId;
   
       public override Guid CatalogBrandId
       {
           get => _catalogBrandId ??= _catalog.CatalogBrandId;
           set => _catalogBrandId = value;
       }
   
       public override string Topic { get; set; } = nameof(CatalogCreatedIntegrationEvent);
   
       public CatalogCreatedIntegrationDomainEvent(CatalogItem catalog) : base()
       {
           _catalog = catalog;
           Id = catalog.Id;
           Name = catalog.Name;
           PictureFileName = catalog.PictureFileName;
       }
   }
   ```

   > 集成领域事件支持跨服务订阅

5. 新建[仓储](/framework/building-blocks/ddd/repository)接口与实现

   :::: code-group
   ::: code-group-item ICatalogItemRepository

   ```csharp Domain/Repositories/ICatalogItemRepository.cs
   using Masa.BuildingBlocks.Ddd.Domain.Repositories;
   using Masa.EShop.Service.Catalog.Domain.Entities;
   
   namespace Masa.EShop.Service.Catalog.Domain.Repositories;
   
   public interface ICatalogItemRepository : IRepository<CatalogItem, Guid>
   {
       
   }
   ```

   :::
   ::: code-group-item CatalogItemRepository

   ```csharp Infrastructure/Repositories/CatalogItemRepository.cs
   using Masa.BuildingBlocks.Data.UoW;
   using Masa.Contrib.Ddd.Domain.Repository.EFCore;
   using Masa.EShop.Service.Catalog.Domain.Entities;
   using Masa.EShop.Service.Catalog.Domain.Repositories;
   
   namespace Masa.EShop.Service.Catalog.Infrastructure.Repositories;
   
   public class CatalogItemRepository : Repository<CatalogDbContext, CatalogItem, Guid>, ICatalogItemRepository
   {
       public CatalogItemRepository(CatalogDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
       {
       }
   }
   ```

   :::
   ::::

   > 通过自定义[仓储](/framework/building-blocks/ddd/repository)，可扩展基类仓储能力 （仓储按约定继承无需手动注册）

6. 更改`ProductCommandHandler`，通过仓储操作[聚合根](/framework/building-blocks/ddd/aggregate-root)

   ```csharp Application/Catalogs/ProductCommandHandler.cs
   using Masa.BuildingBlocks.Caching;
   using Masa.BuildingBlocks.Ddd.Domain.Repositories;
   using Masa.Contrib.Dispatcher.Events;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   using Masa.EShop.Service.Catalog.Domain.Entities;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs;
   
   public class ProductCommandHandler
   {
       private readonly IRepository<CatalogItem, Guid> _repository;
       private readonly IMultilevelCacheClient _multilevelCacheClient;
   
       public ProductCommandHandler(
           IRepository<CatalogItem, Guid> repository,
           IMultilevelCacheClient multilevelCacheClient)
       {
           _repository = repository;
           _multilevelCacheClient = multilevelCacheClient;
       }
   
       [EventHandler]
       public async Task CreateHandleAsync(CreateProductCommand command)
       {
           var catalogItem = new CatalogItem(command.Name, command.Price, command.PictureFileName ?? "default.png");
           catalogItem.SetCatalogType(command.CatalogTypeId);
           catalogItem.SetCatalogBrand(command.CatalogBrandId);
           await _repository.AddAsync(catalogItem);
           
           await _multilevelCacheClient.SetAsync(catalogItem.Id.ToString(), catalogItem);
       }
   
       [EventHandler]
       public async Task DeleteHandlerAsync(DeleteProductCommand command)
       {
           await _repository.RemoveAsync(command.ProductId);
           await _multilevelCacheClient.RemoveAsync<CatalogItem>(command.ProductId.ToString());
       }
   }
   ```

## 问题

1. 创建商品提示`System.InvalidOperationException: No service for type 'Masa.BuildingBlocks.Data.ISequentialGuidGenerator' has been registered.`

   未安装并注册 [有序Guid生成器](/framework/building-blocks/id-generator/sequential-guid)

## 总结

[领域驱动设计](/framework/building-blocks/ddd/overview)是为了更聚焦业务，将复杂的设计放在领域模型上，模型反映的动作与实际业务一致，它使得后续迭代升级更为简单
