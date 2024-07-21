# 实战教程 - 第三章: 事件总线和读写分离

## 概述

本章将使用 [事件总线](/framework/building-blocks/dispatcher/overview) 和 [读写分离](/framework/building-blocks/cqrs)，并接入 `FluentValidation` 调整参数验证

> 示例中不再创建读模型，使用与写模型完全一致的数据库，会创建一个 `CatalogQueryDbContext` 用来标记当前使用的是读模型

以创建商品为例：

<div>
  <img alt="Create Product" src="https://cdn.masastack.com/framework/tutorial/mf-part-3.png"/>
</div>

## 开始

1. 选中 `Masa.EShop.Service.Catalog`项目并安装 `Masa.Contrib.Dispatcher.Events`、 `Masa.Contrib.Dispatcher.Events.FluentValidation`、`Masa.Contrib.Data.UoW.EFCore`、 `Masa.Contrib.Dispatcher.IntegrationEvents.Dapr`、`Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore`、`FluentValidation.AspNetCore`

   ```shell 终端
   dotnet add package Masa.Contrib.Dispatcher.Events -v 1.0.0
   dotnet add package Masa.Contrib.Dispatcher.Events.FluentValidation -v 1.0.0
   dotnet add package Masa.Contrib.Data.UoW.EFCore -v 1.0.0
   dotnet add package Masa.Contrib.Dispatcher.IntegrationEvents.Dapr -v 1.0.0
   dotnet add package Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EFCore -v 1.0.0
   dotnet add package FluentValidation.AspNetCore
   ```

   > `FluentValidation.AspNetCore`、`Masa.Contrib.Dispatcher.Events.FluentValidation`提供了基于`FluentValidation`的验证中间件，分离关注点
   >
   > 使用 `Masa.Contrib.Data.UoW.EFCore` 后，EventBus 会自动保存提交

2. 注册[进程内事件总线](/framework/building-blocks/dispatcher/local-event)、[跨进程事件总线](/framework/building-blocks/dispatcher/integration-event)、工作单元（UoW），并使用 `FluentValidation` 进行参数验证`

   ```csharp Program.cs l:11-18,29-33
   using System.Reflection;
   using FluentValidation;
   using Masa.BuildingBlocks.Data.UoW;
   using Masa.BuildingBlocks.Dispatcher.Events;
   using Masa.BuildingBlocks.Dispatcher.IntegrationEvents;
   
   -----ignore other namespaces-----
   
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services
       .AddIntegrationEventBus(options=>
       {
           options.UseDapr().UseEventLog<CatalogDbContext>()
               .UseEventBus(eventBusBuilder=> eventBusBuilder.UseMiddleware(typeof(ValidatorEventMiddleware<>)))
               .UseUoW<CatalogDbContext>();
       })
       .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
   
   -----Ignore the rest of the service registration-----
   
   var app = builder.AddServices();//`var app = builder.Build();` for projects not using MinimalAPis
   
   -----Ignore the use of middleware, Swagger, etc.-----
   
   app.UseRouting();
   
   // Subscription event must be added
   app.UseCloudEvents();
   app.UseEndpoints(endpoint =>
   {
       endpoint.MapSubscribeHandler();
   });
       
   app.Run();
   ```

   > 注册事件总线在 **AddServices** 之前即可，集成事件总线由 `dapr` 提供，确保开发环境已经成功配置 [dapr](https://docs.dapr.io/zh-hans/getting-started/) 环境

3. 安装 `Dapr Starter` 并注册 （提供 `dapr sidecar` 管理）

   :::: code-group
   ::: code-group-item 安装 nuget 包

   ```shell 终端
   dotnet add package Masa.Contrib.Development.DaprStarter.AspNetCore -v 1.0.0-rc.1
   ```

   :::
   ::: code-group-item 注册DaprStarter

   ```csharp Program.cs l:9
   var builder = WebApplication.CreateBuilder(args);
   
   -----Ignore the rest of the service registration-----
   
   #region Use DaprStarter
   
   if (builder.Environment.IsDevelopment())
   {
       builder.Services.AddDaprStarter();
   }
   
   #endregion
   
   var app = builder.AddServices();//`var app = builder.Build();` for projects not using MinimalAPis
   
   -----Ignore the use of middleware, Swagger, etc.-----
   ```

   :::
   ::::

   > 线上环境不需要使用 [DaprStarter](/framework/building-blocks/development/dapr-starter) ，它仅在开发环境下使用

4. 新建读库上下文 `CatalogQueryDbContext` 并注册

   :::: code-group
   ::: code-group-item CatalogQueryDbContext（读模型）

   ```csharp Infrastructure/CatalogQueryDbContext.cs
   using Masa.EShop.Service.Catalog.Infrastructure.EntityConfigurations;
   using Microsoft.EntityFrameworkCore;
   
   namespace Masa.EShop.Service.Catalog.Infrastructure;
   
   public class CatalogQueryDbContext : MasaDbContext<CatalogQueryDbContext>
   {
       public CatalogQueryDbContext(MasaDbContextOptions<CatalogQueryDbContext> dbContextOptions) : base(dbContextOptions)
       {
       }
   
       protected override void OnModelCreatingExecuting(ModelBuilder builder)
       {
           builder.ApplyConfigurationsFromAssembly(typeof(CatalogBrandEntityTypeConfiguration).Assembly);
           base.OnModelCreatingExecuting(builder);
       }
   }
   ```
   ::: 
   ::: code-group-item 注册 CatalogQueryDbContext
   ```csharp Program.cs l:12-17
   var builder = WebApplication.CreateBuilder(args);
   
   -----Ignore the rest of the service registration-----
   
   builder.Services.AddMasaDbContext<CatalogDbContext>(contextBuilder =>
   {
       contextBuilder
           .UseSqlite()
           .UseFilter();
   });
   
   builder.Services.AddMasaDbContext<CatalogQueryDbContext>(contextBuilder =>
   {
       contextBuilder
           .UseSqlite()
           .UseFilter();
   });
   
   var app = builder.AddServices();//`var app = builder.Build();` for projects not using MinimalAPis
   
   -----Ignore the use of middleware, Swagger, etc.-----
   
   app.Run();
   ```
   :::
   ::::

   > 在 [CQRS](/framework/building-blocks/cqrs) 架构下 读模型只支持查询，通过优化读模型提升查询效率，读模型的数据上下文与写模型的数据上下文可以完全不同，读模型用`缓存`、`ES` 或者其它数据源都可以

5. 修改用于接收新增产品、修改产品参数类，按照类型将其分为`XXXCommand`、`XXXQuery`，并为其创建对应的参数验证类`XXXCommandValidator`、`XXXQueryValidator`

   其中写命令事件、读命令事件分别为

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
   ::: code-group-item DeleteProductCommand

   ```csharp Application/Catalogs/Commands/DeleteProductCommand.cs
   using Masa.BuildingBlocks.ReadWriteSplitting.Cqrs.Commands;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   
   public record DeleteProductCommand : Command
   {
       public Guid ProductId { get; set; }
   }
   ```

   :::
   ::: code-group-item ProductQuery

   ```csharp Application/Catalogs/Queries/ProductQuery.cs
   using Masa.BuildingBlocks.ReadWriteSplitting.Cqrs.Queries;
   using Masa.EShop.Contracts.Catalog.Dto;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs.Queries;
   
   public record ProductQuery : Query<CatalogItemDto>
   {
       public Guid ProductId { get; set; } = default!;
       
       public override CatalogItemDto Result { get; set; } = default!;
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

   为对应事件添加参数验证类，用于验证参数是否合法

   :::: code-group
   ::: code-group-item CreateProductCommandValidator

   ```csharp Application/Catalogs/Commands/CreateProductCommandValidator.cs
   using FluentValidation;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   
   public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
   {
       public CreateProductCommandValidator()
       {
           RuleFor(cmd => cmd.Name).Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Product name cannot be empty");
           RuleFor(cmd => cmd.CatalogBrandId).NotEqual(Guid.Empty).WithMessage("Please select a product brand");
           RuleFor(cmd => cmd.CatalogTypeId)
               .NotEqual(0).WithMessage("Please select a product category")
               .GreaterThan(0).WithMessage("Product doesn't exist");
           RuleFor(cmd => cmd.Price)
               .NotEqual(0).WithMessage("Please enter product price")
               .GreaterThan(0).WithMessage("Price input error");
           RuleFor(cmd => cmd.Stock)
               .NotEqual(0).WithMessage("Please enter product inventory")
               .GreaterThan(0).WithMessage("Price input error");
       }
   }
   ```

   :::
   ::: code-group-item DeleteProductCommandValidator

   ```csharp Application/Catalogs/Commands/DeleteProductCommandValidator.cs
   using FluentValidation;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   
   public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
   {
       public DeleteProductCommandValidator()
       {
           RuleFor(cmd => cmd.ProductId).NotEqual(Guid.Empty).WithMessage("Please enter the ProductId");
       }
   }
   ```

   :::
   ::: code-group-item ProductQueryValidator

   ```csharp Application/Catalogs/Queries/ProductQueryValidator.cs
   using FluentValidation;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs.Queries;
   
   public class ProductQueryValidator : AbstractValidator<ProductQuery>
   {
       public ProductQueryValidator()
       {
           RuleFor(item => item.ProductId).NotEqual(Guid.Empty).WithMessage("Please enter the ProductId");
       }
   }
   ```

   :::
   ::: code-group-item ProductsQueryValidator

   ```csharp Application/Catalogs/Queries/ProductsQueryValidator.cs
   using FluentValidation;
   
   namespace Masa.EShop.Service.Catalog.Application.Catalogs.Queries;
   
   public class ProductsQueryValidator : AbstractValidator<ProductsQuery>
   {
       public ProductsQueryValidator()
       {
           RuleFor(item => item.Page).GreaterThan(0);
           RuleFor(item => item.PageSize).GreaterThan(0);
       }
   }
   ```

   :::
   ::::

   > 参数验证类并非是必须的，如果当前事件不需要验证参数，则可跳过不创建对应的验证程序

6. 新建`ProductCommandHandler`、`ProductQueryHandler`用于处理产品的增删改查

   * `ProductCommandHandler`：**新增产品**、**删除产品**
   * `ProductQueryHandler`：**查询产品详情**、**查询产品列表**、**查询已被删除的产品列表**

   :::: code-group
   ::: code-group-item ProductCommandHandler

   ```csharp Application/Catalogs/ProductCommandHandler.cs
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
   
       public ProductCommandHandler(CatalogDbContext dbContext, IEventBus eventBus)
       {
           _dbContext = dbContext;
           _eventBus = eventBus;
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
       }
   }
   ```

   :::
   ::: code-group-item ProductQueryHandler

   ```csharp Application/Catalogs/ProductQueryHandler.cs
   using System.Linq.Expressions;
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
   
       [EventHandler]
       public async Task ProductsHandleAsync(ProductsQuery query, IDataFilter dataFilter)
       {
           Expression<Func<CatalogItem, bool>> condition = item => true;
           condition = condition.And(!query.Name.IsNullOrWhiteSpace(), item => item.Name.Contains(query.Name!));
   
           if (!query.IsRecycle)
           {
               await GetItemsAsync();
           }
           else
           {
               using (dataFilter.Disable<ISoftDelete>())
               {
                   condition = condition.And(item => item.IsDeleted); //Only query the data of the recycle bin
                   await GetItemsAsync();
               }
           }
   
           async Task GetItemsAsync()
           {
               var queryable = _dbContext.Set<CatalogItem>().Where(condition);
   
               var total = await queryable.LongCountAsync();
   
               var totalPages = (int)Math.Ceiling((double)total / query.PageSize);
   
               var list = await queryable.Where(condition)
                   .Select(item => new CatalogListItemDto()
                   {
                       Id = item.Id,
                       Name = item.Name,
                       Price = item.Price,
                       PictureFileName = item.PictureFileName,
                       CatalogTypeId = item.CatalogTypeId,
                       CatalogBrandId = item.CatalogBrandId,
                       Stock = item.Stock,
                   })
                   .Skip((query.Page - 1) * query.PageSize)
                   .Take(query.PageSize)
                   .ToListAsync();
   
               query.Result = new PaginatedListBase<CatalogListItemDto>()
               {
                   Total = total,
                   TotalPages = totalPages,
                   Result = list
               };
           }
       }
   
       [EventHandler]
       public async Task ProductHandleAsync(ProductQuery query)
       {
           var catalogItem = await _dbContext.Set<CatalogItem>()
               .Where(item => item.Id == query.ProductId)
               .Select(item => new CatalogItemDto()
               {
                   Id = item.Id,
                   Name = item.Name,
                   Price = item.Price,
                   PictureFileName = item.PictureFileName,
                   CatalogTypeId = item.CatalogTypeId,
                   CatalogBrandId = item.CatalogBrandId
               }).FirstOrDefaultAsync() ?? throw new UserFriendlyException("Product doesn't exist");
           query.Result = catalogItem;
       }
   }
   ```

   :::
   ::::

   > 事件处理程序所在类支持通过构造函数注入、也支持方法注入 (标记 `EventHandler` 特性的 **public** 方法)

7. 新增商品创建集成事件

   ```csharp Masa.EShop.Contracts.Catalog/IntegrationEvents/CatalogCreatedIntegrationEvent.cs
   using Masa.BuildingBlocks.Dispatcher.IntegrationEvents;
   
   namespace Masa.EShop.Contracts.Catalog.IntegrationEvents;
   
   public record CatalogCreatedIntegrationEvent : IntegrationEvent
   {
       public Guid Id { get; set; }
   
       public string Name { get; set; } = default!;
   
       public string? PictureFileName { get; set; }
   
       public virtual int CatalogTypeId { get; set; }
   
       public virtual Guid CatalogBrandId { get; set; }
   }
   ```

   > 用于创建商品完成后，通知仓库管理员
   >
   > 选中 `Masa.EShop.Contracts.Catalog` 并安装 **nuget** 包 `Masa.BuildingBlocks.Dispatcher.IntegrationEvents`

8. 修改 `CatalogItemService.cs`

   ```csharp Services/CatalogItemService.cs
   using Masa.BuildingBlocks.Dispatcher.Events;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Queries;
   
   namespace Masa.EShop.Service.Catalog.Services;
   
   public class CatalogItemService : ServiceBase
   {
       private IEventBus EventBus => GetRequiredService<IEventBus>();
   
       public async Task<IResult> GetAsync(Guid id)
       {
           var query = new ProductQuery() { ProductId = id };
           await EventBus.PublishAsync(query);
           return Results.Ok(query.Result);
       }
   
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
   
       /// <summary>
       /// Show only deleted listings
       /// </summary>
       public async Task<IResult> GetRecycleItemsAsync(
           string? name,
           int page = 1,
           int pageSize = 10)
       {
           var query = new ProductsQuery()
           {
               Name = name,
               IsRecycle = true,
               Page = page,
               PageSize = pageSize
           };
           await EventBus.PublishAsync(query);
           return Results.Ok(query.Result);
       }
   
       public async Task<IResult> CreateProductAsync(CreateProductCommand command)
       {
           await EventBus.PublishAsync(command);
           return Results.Accepted();
       }
   
       public async Task<IResult> DeleteProductAsync(Guid id)
       {
           await EventBus.PublishAsync(new DeleteProductCommand() { ProductId = id });
   
           return Results.Accepted();
       }
   }
   ```

9. 新增`IntegrationEventService`，用于订阅创建产品事件，同通知仓库管理员

   ```csharp Services/IntegrationEventService.cs
   using Dapr;
   using Masa.EShop.Contracts.Catalog.IntegrationEvents;
   
   namespace Masa.EShop.Service.Catalog.Services;
   
   public class IntegrationEventService : ServiceBase
   {
       private const string DAPR_PUBSUB_NAME = "pubsub";
   
       private ILogger<IntegrationEventService> _logger => GetRequiredService<ILogger<IntegrationEventService>>();
   
       [Topic(DAPR_PUBSUB_NAME, nameof(CatalogCreatedIntegrationEvent))]
       public Task NoticeWarehouseAdministratorByCatalogCreated(CatalogCreatedIntegrationEvent @event)
       {
           _logger.LogInformation("New product: {Name}, Id: {Id}", @event.Name, @event.Id);
           //todo: Notify warehouse clerks of new products
           return Task.CompletedTask;
       }
   }
   ```

## 相关文档

* [手把手教你学Dapr](https://www.cnblogs.com/doddgu/p/dapr-learning-1.html)

## 总结

通过事件总线、读写分离，它将使我们更聚焦业务。将关注点分离，使得我们在读场景时关注性能，写场景时关注业务逻辑，而在需要验证参数时在 `XXXValidator` 完成，避免过多不相关的操作影响到核心业务
