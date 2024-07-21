# 领域驱动设计 - 仓储

## 概述

屏蔽业务逻辑和持久化基础设施的差异，针对不同的存储设施，会有不同的实现方式，但这些不会对我们的业务产生影响，它是领域驱动设计的一部分，提供了一些列用于管理领域对象的方法，例如：添加、删除、更新和查询

## 使用 

1. 安装 `Masa.Contrib.Ddd.Domain.Repository.EFCore`

   ```shell 终端
   dotnet add package Masa.Contrib.Ddd.Domain.Repository.EFCore
   ```

2. 注册 `Repository`

   ```csharp Program.cs l:3
   builder.Services.AddDomainEventBus(options =>
   {
       options.UseRepository<CatalogDbContext>();
   });
   ```
   > AddDomainEventBus 由 `Masa.Contrib.Ddd.Domain` 提供

3. 使用 **仓储** 

   ```csharp l:5-8,20
   public class ProductCommandHandler
   {
       private readonly IRepository<CatalogItem, int> _repository;
   
       public ProductCommandHandler(IRepository<CatalogItem, int> repository)
       {
           _repository = repository;
       }
   
       [EventHandler]
       public async Task CreateHandleAsync(CreateProductCommand command)
       {
           var catalogItem = new CatalogItem(
               command.CatalogBrandId, 
               command.CatalogTypeId, 
               command.Name,
               command.Description,
               PictureFileName = command.PictureFileName ?? "default.png",
               command.Price);
           await _repository.AddAsync(catalogItem);
       }
   }
   ```

## 高级

### 自定义仓储

可通过继承 ``IRepository<TEntity, TKey>` 扩展仓储

1. 新建 `ICatalogItemRepository`

   ```csharp
   public interface ICatalogItemRepository: IRepository<CatalogItem, int>
   {
   
   }
   ```

2. 新建 `CatalogItemRepository` 实现类

   ```csharp
   public class CatalogItemRepository: Repository<CatalogDbContext, CatalogItem, Guid>, ICatalogItemRepository
   {
       public CatalogItemRepository(CatalogDbContext context, IUnitOfWork unitOfWork) 
           : base(context, unitOfWork)
       {
       }
   }
   ```

3. 使用 **自定义仓储** 

   ```csharp l:5-8,20
   public class ProductCommandHandler
   {
       private readonly ICatalogItemRepository _repository;
   
       public ProductCommandHandler(ICatalogItemRepository repository)
       {
           _repository = repository;
       }
   
       [EventHandler]
       public async Task CreateHandleAsync(CreateProductCommand command)
       {
           var catalogItem = new CatalogItem(
               command.CatalogBrandId, 
               command.CatalogTypeId, 
               command.Name,
               command.Description,
               PictureFileName = command.PictureFileName ?? "default.png",
               command.Price);
           await _repository.AddAsync(catalogItem);
       }
   }
   ```

## 功能

* AddAsync：添加实体
* AddRangeAsync：批量添加实体
* UpdateAsync：更新实体
* UpdateRangeAsync：批量更新实体
* RemoveAsync：移除指定实体
* RemoveRangeAsync：批量移除指定实体集合
* FindAsync：根据主键查询满足条件的实体，不存在则返回`null` 
* GetListAsync：获取实体列表
* GetCountAsync：获取实体数量
* GetPaginatedListAsync：根据指定排序字段进行降序或者升序排序并获取分页数据

对于指定主键的 `IRepository<TEntity, TKey>` 仓储，除了支持上述方法之外，还提供了:

* FindAsync：获取指定主键`id`的实体
* RemoveAsync：移除指定主键`id`的实体
* RemoveRangeAsync：移除指定`id`集合的实体

## 原理剖析

* 为何自定义仓储不需要注册就可以直接使用?

  基于 **约定大于配置**，我们约定好继承 `IRepository<TEntity>` 的接口属于自定义仓储，它是针对默认仓储的扩展，在项目启动时会通过反射找到所有实体类以及继承 `IRepository<TEntity>` 的自定义仓储接口进行注册，开发者只需要按照约定注册使用即可

  ```csharp l:3
  builder.Services.AddDomainEventBus(options =>
  {
      options.UseRepository<CatalogDbContext>();
  });
  ```

  > 默认使用全局配置中设置的程序集，并查询其中对应实体类进行服务注册