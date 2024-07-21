# 实战教程 - 第二章: 数据上下文

## 概述

本章将新增数据上下文，这里我们使用的是[`EFCore`](/framework/building-blocks/data/orm-efcore)作为ORM提供程序（使用`Sqlite`数据库代替`内存数据源`）

## 开始

1. 选中 `Masa.EShop.Service.Catalog` 项目并安装 `Masa.Contrib.Data.EFCore.Sqlite` 、`Masa.Contrib.Data.Contracts`

   ```shell 终端
   dotnet add package Masa.Contrib.Data.EFCore.Sqlite -v 1.0.0
   dotnet add package Masa.Contrib.Data.Contracts -v 1.0.0
   ```

   > **Masa.Contrib.Data.Contracts** 提供了软删除、数据过滤的功能

2. 新增 **商品品牌**、**商品类型**、**商品信息**模型:

   :::: code-group
     ::: code-group-item CatalogBrand
   
     ```csharp Domain/Entities/CatalogBrand.cs
     using Masa.BuildingBlocks.Data;
     
     namespace Masa.EShop.Service.Catalog.Domain.Entities;
     
     public class CatalogBrand : ISoftDelete
     {
         public Guid Id { get; set; }
     
         public string Brand { get; set; }
     
         public int Creator { get; set; }
     
         public DateTime CreationTime { get; set; }
     
         public int Modifier { get; set; } = default!;
     
         public DateTime ModificationTime { get; set; }
     
         public bool IsDeleted { get; private set; }
     
         private CatalogBrand()
         {
         }
     
         public CatalogBrand(Guid? id, string brand) : this()
         {
             Brand = brand;
             Id = id ?? Guid.NewGuid();
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
     
     namespace Masa.EShop.Service.Catalog.Domain.Entities;
     
     public class CatalogItem : ISoftDelete
     {
         public Guid Id { get; set; }
     
         public string Name { get; set; } = null!;
     
         public decimal Price { get; set; }
     
         public string PictureFileName { get; set; } = "";
     
         public int CatalogTypeId { get; set; }
     
         public CatalogType CatalogType { get; private set; } = null!;
     
         public Guid CatalogBrandId { get; set; }
     
         public CatalogBrand CatalogBrand { get; private set; } = null!;
     
         public int Stock { get; set; }
         
         public int Creator { get; set; }
         
         public DateTime CreationTime { get; set; }
     
         public int Modifier { get; set; } = default!;
     
         public DateTime ModificationTime { get; set; }
         
         public bool IsDeleted { get; private set; }
     }
     ```
     :::
     ::::
   
   > 确保实体需要有一个[无参构造函数](https://learn.microsoft.com/zh-cn/ef/core/modeling/constructors) （哪怕它是的私有构造函数）
   
3. 为商品品牌、商品分类、商品信息配置数据库映射关系

   :::: code-group
     ::: code-group-item CatalogBrandEntityTypeConfiguration
   
     ```csharp Infrastructure/EntityConfigurations/CatalogBrandEntityTypeConfiguration.cs
     using Masa.EShop.Service.Catalog.Domain.Entities;
     using Microsoft.EntityFrameworkCore;
     using Microsoft.EntityFrameworkCore.Metadata.Builders;
     
     namespace Masa.EShop.Service.Catalog.Infrastructure.EntityConfigurations;
     
     class CatalogBrandEntityTypeConfiguration
         : IEntityTypeConfiguration<CatalogBrand>
     {
         public void Configure(EntityTypeBuilder<CatalogBrand> builder)
         {
             builder.ToTable(nameof(CatalogBrand));
     
             builder.HasKey(cb => cb.Id);
     
             builder.Property(cb => cb.Id)
                .IsRequired();
     
             builder.Property(cb => cb.Brand)
                 .IsRequired()
                 .HasMaxLength(100);
         }
     }
     ```
   
     :::
     ::: code-group-item CatalogTypeEntityTypeConfiguration
   
     ```csharp Infrastructure/EntityConfigurations/CatalogTypeEntityTypeConfiguration.cs
     using Masa.EShop.Service.Catalog.Domain.Entities;
     using Microsoft.EntityFrameworkCore;
     using Microsoft.EntityFrameworkCore.Metadata.Builders;
     
     namespace Masa.EShop.Service.Catalog.Infrastructure.EntityConfigurations;
     
     class CatalogTypeEntityTypeConfiguration
         : IEntityTypeConfiguration<CatalogType>
     {
         public void Configure(EntityTypeBuilder<CatalogType> builder)
         {
             builder.ToTable(nameof(CatalogType));
             
             builder.HasKey(ct => ct.Id);
     
             builder.Property(ct => ct.Id)
                .IsRequired();
     
             builder.Property(ct => ct.Name)
                 .IsRequired()
                 .HasMaxLength(100);
         }
     }
     ```
   
     :::
     ::: code-group-item CatalogItemEntityTypeConfiguration
   
     ```csharp Infrastructure/EntityConfigurations/CatalogItemEntityTypeConfiguration.cs
     using Masa.EShop.Service.Catalog.Domain.Entities;
     using Microsoft.EntityFrameworkCore;
     using Microsoft.EntityFrameworkCore.Metadata.Builders;
     
     namespace Masa.EShop.Service.Catalog.Infrastructure.EntityConfigurations;
     
     class CatalogItemEntityTypeConfiguration
         : IEntityTypeConfiguration<CatalogItem>
     {
         public void Configure(EntityTypeBuilder<CatalogItem> builder)
         {
             builder.ToTable("Catalog");
     
             builder.Property(ci => ci.Id)
                 .IsRequired();
     
             builder.Property(ci => ci.Name)
                 .IsRequired(true)
                 .HasMaxLength(50);
     
             builder.Property(ci => ci.Price)
                 .IsRequired(true);
     
             builder.Property(ci => ci.PictureFileName)
                 .IsRequired(false);
     
             builder.HasOne(ci => ci.CatalogBrand)
                 .WithMany()
                 .HasForeignKey(ci => ci.CatalogBrandId);
     
             builder.HasOne(ci => ci.CatalogType)
                 .WithMany()
                 .HasForeignKey(ci => ci.CatalogTypeId);
         }
     }
     ```
   
     :::
     ::::
   
4. 创建数据上下文 `CatalogDbContext`, 并继承 `MasaDbContext<CatalogDbContext>`

   ```csharp Infrastructure/CatalogDbContext.cs
   using Masa.EShop.Service.Catalog.Infrastructure.EntityConfigurations;
   using Microsoft.EntityFrameworkCore;
   
   namespace Masa.EShop.Service.Catalog.Infrastructure;
   
   public class CatalogDbContext : MasaDbContext<CatalogDbContext>
   {
       public CatalogDbContext(MasaDbContextOptions<CatalogDbContext> dbContextOptions) : base(dbContextOptions)
       {
       }
   
       protected override void OnModelCreatingExecuting(ModelBuilder builder)
       {
           builder.ApplyConfigurationsFromAssembly(typeof(CatalogBrandEntityTypeConfiguration).Assembly);
           base.OnModelCreatingExecuting(builder);
       }
   }
   ```
   
   > [MasaDbContext](/framework/building-blocks/data/orm-efcore) 的更多用法
   
5. 配置数据库连接字符串

   ```json appsettings.Development.json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=Catalog.db;"
     }
   }
   ```

   <app-alert type="warning" content="推荐在appsettings.{环境变量}.json配置数据库连接字符串"></app-alert>

6. 注册数据上下文 `CatalogDbContext`

   ```csharp Program.cs
   using Masa.EShop.Service.Catalog.Infrastructure;
   using Microsoft.EntityFrameworkCore;
   
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddMasaDbContext<CatalogDbContext>(contextBuilder =>
   {
       contextBuilder
           .UseSqlite()
           .UseFilter();
   });
   
   -----Ignore the rest of the service registration-----
   
   var app = builder.AddServices();//`var app = builder.Build();` for projects not using MinimalAPis
   
   -----Ignore the use of middleware, Swagger, etc.-----
   
   app.Run();
   ```

   > **UseFilter** 方法由 **Masa.Contrib.Data.Contracts** 提供
   >
   > 注册数据上下文在 **AddServices** 之前即可

7. 数据库迁移，确保已安装 [EF Core 命令行工具](https://learn.microsoft.com/zh-cn/ef/core/cli/dotnet)

   1. 选中 `Masa.EShop.Service.Catalog` 项目并安装`Microsoft.EntityFrameworkCore.Tools`

      ```shell 终端
      dotnet add package Microsoft.EntityFrameworkCore.Tools --version 6.0.0
      ```
      
   2. 模型迁移
   
      :::: code-group
      ::: code-group-item .NET Core CLI
      ```shell 终端
      dotnet ef migrations add InitialCreate
      ```
      :::
      ::: code-group-item Visual Studio
   
      ```shell Visual Studio
      Add-Migration InitialCreate
      ```
      :::
      ::::
   
      > 需在 `Masa.EShop.Service.Catalog` 文件夹下执行迁移命令

   3. 更新数据库

      :::: code-group
      ::: code-group-item .NET Core CLI
      
      ```shell 终端
      dotnet ef database update
      ```
      :::
      ::: code-group-item Visual Studio
      ```shell Visual Studio
      Update-Database
      ```
      :::
      ::::
      
      > 模型迁移需要安装 `Microsoft.EntityFrameworkCore.Tools` ，请确保已正确安装
      >
      > 多数据上下文时请在命令行尾部增加 ` --context CatalogDbContext`
   
8. 种子数据迁移 （非必须）

   :::: code-group
   ::: code-group-item HostExtensions（迁移数据）

   ```csharp Infrastructure/Extensions/HostExtensions.cs
   using Microsoft.EntityFrameworkCore;
   
   namespace Masa.EShop.Service.Catalog.Infrastructure.Extensions;
   
   public static class HostExtensions
   {
       public static Task MigrateDbContextAsync<TContext>(this IHost host, Func<TContext, IServiceProvider, Task> seeder)
           where TContext : DbContext
       {
           using var scope = host.Services.CreateScope();
           var services = scope.ServiceProvider;
           
           var env = services.GetRequiredService<IWebHostEnvironment>();
           if (!env.IsDevelopment())
               return Task.CompletedTask;
           
           var context = services.GetRequiredService<TContext>();
           return seeder(context, services);
       }
   }
   ```

   :::
   ::: code-group-item CatalogContextSeed（初始化种子数据）

   ```csharp Infrastructure/Extensions/CatalogContextSeed.cs
   using Masa.EShop.Service.Catalog.Domain.Entities;
   
   namespace Masa.EShop.Service.Catalog.Infrastructure.Extensions;
   
   public class CatalogContextSeed
   {
       public static async Task SeedAsync(CatalogDbContext context)
       {
           if (!context.Set<CatalogBrand>().Any())
           {
               var catalogBrands = new List<CatalogBrand>()
               {
                   new(Guid.Parse("31b1c60b-e9c3-4646-ac70-09354bdb1522"), "LONSID")
               };
               await context.Set<CatalogBrand>().AddRangeAsync(catalogBrands);
   
               await context.SaveChangesAsync();
           }
   
           if (!context.Set<CatalogType>().Any())
           {
               var catalogTypes = new List<CatalogType>()
               {
                   new(1, "Water Dispenser")
               };
               await context.Set<CatalogType>().AddRangeAsync(catalogTypes);
               await context.SaveChangesAsync();
           }
       }
   }
   ```
   
   :::
   ::: code-group-item 使用迁移并完成种子数据初始化

   ```csharp Program.cs
   using Masa.EShop.Service.Catalog.Infrastructure.Extensions;
   
   -----Ignore other namespaces-----
   
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddMasaDbContext<CatalogDbContext>(contextBuilder =>
   {
       contextBuilder
           .UseSqlite()
           .UseFilter();
   });
   
   -----Ignore the rest of the service registration-----
   
   var app = builder.AddServices();//`var app = builder.Build();` for projects not using MinimalAPis
   
   -----Ignore the use of middleware, Swagger, etc.-----
   
   await app.MigrateDbContextAsync<CatalogDbContext>(async (context, services) =>
   {
       await CatalogContextSeed.SeedAsync(context);
   });
   
   app.Run();
   ```
   
   :::
   ::::
   
9. 修改`CatalogItemService`的数据源

   ```csharp Services/CatalogItemService.cs
   using System.Linq.Expressions;
   using Masa.EShop.Contracts.Catalog.Dto;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   using Masa.EShop.Service.Catalog.Domain.Entities;
   using Masa.EShop.Service.Catalog.Infrastructure;
   using Masa.Utils.Models;
   using Microsoft.EntityFrameworkCore;
   
   namespace Masa.EShop.Service.Catalog.Services;
   
   public class CatalogItemService : ServiceBase
   {
       private CatalogDbContext DbContext => GetRequiredService<CatalogDbContext>();
   
       public async Task<IResult> GetAsync(Guid id)
       {
           if (id == Guid.Empty)
               throw new UserFriendlyException("Please enter the ProductId");
   
           var catalogItem = await DbContext.Set<CatalogItem>().Where(item => item.Id == id).Select(item => new CatalogItemDto()
           {
               Id = item.Id,
               Name = item.Name,
               Price = item.Price,
               PictureFileName = item.PictureFileName,
               CatalogTypeId = item.CatalogTypeId,
               CatalogBrandId = item.CatalogBrandId
           }).FirstOrDefaultAsync();
           if (catalogItem == null)
               throw new UserFriendlyException("Product doesn't exist");
   
           return Results.Ok(catalogItem);
       }
       
       public Task<IResult> GetItemsAsync(
           string? name = null,
           int page = 1,
           int pageSize = 10)
           => GetItemsAsync(name, page, pageSize, false);
   
       public Task<IResult> GetRecycleItemsAsync(
           string? name = null,
           int page = 1,
           int pageSize = 10)
           => GetItemsAsync(name, page, pageSize, true);
   
       private async Task<IResult> GetItemsAsync(
           string name,
           int page,
           int pageSize,
           bool isDelete)
       {
           if (page <= 0)
               throw new UserFriendlyException("Page must be greater than 0");
   
           if (pageSize <= 0)
               throw new UserFriendlyException("PageSize must be greater than 0");
   
           Expression<Func<CatalogItem, bool>> condition = item => item.IsDeleted == isDelete;
           condition = condition.And(!name.IsNullOrWhiteSpace(), item => item.Name.Contains(name));
           var queryable = DbContext.Set<CatalogItem>().Where(condition);
           var total = await queryable.LongCountAsync();
           var list = await queryable.Where(condition).Select(item => new CatalogListItemDto()
           {
               Id = item.Id,
               Name = item.Name,
               Price = item.Price,
               PictureFileName = item.PictureFileName,
               CatalogTypeId = item.CatalogTypeId,
               CatalogBrandId = item.CatalogBrandId,
               Stock = item.Stock,
           }).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
   
           var pageData = new PaginatedListBase<CatalogListItemDto>()
           {
               Total = total,
               TotalPages = (int)Math.Ceiling((double)total / pageSize),
               Result = list
           };
           return Results.Ok(pageData);
       }
   
       public async Task<IResult> CreateProductAsync(CreateProductCommand command)
       {
           if (command.Name.IsNullOrWhiteSpace())
               throw new UserFriendlyException("Product name cannot be empty");
   
           if (command.CatalogBrandId == Guid.Empty)
               throw new UserFriendlyException("Please select a product brand");
           if (command.CatalogTypeId == 0)
               throw new UserFriendlyException("Please select a product category");
           if (command.CatalogTypeId < 0)
               throw new UserFriendlyException("Product doesn't exist");
           if (command.Price == 0)
               throw new UserFriendlyException("Please enter product price");
           if (command.Price < 0)
               throw new UserFriendlyException("Price input error");
           if (command.Stock == 0)
               throw new UserFriendlyException("Please enter product inventory");
           if (command.Stock < 0)
               throw new UserFriendlyException("Inventory input error");
   
           var catalogItem = new CatalogItem()
           {
               CatalogBrandId = command.CatalogBrandId,
               CatalogTypeId = command.CatalogTypeId,
               Name = command.Name,
               PictureFileName = command.PictureFileName ?? "default.png",
               Price = command.Price
           };
   
           await DbContext.Set<CatalogItem>().AddAsync(catalogItem);
           await DbContext.SaveChangesAsync();
           
           //todo: Notify warehouse clerks of new products
           
           return Results.Accepted();
       }
   
       public async Task<IResult> DeleteProductAsync(Guid id)
       {
           if (id == Guid.Empty)
               throw new UserFriendlyException("Please enter the ProductId");
   
           var catalogItem = await DbContext.Set<CatalogItem>().FirstOrDefaultAsync(item => item.Id == id);
           if (catalogItem == null)
               throw new UserFriendlyException("Product doesn't exist");
   
           DbContext.Set<CatalogItem>().Remove(catalogItem);
           await DbContext.SaveChangesAsync();
   
           return Results.Accepted();
       }
   }
   ```
   

## 问题

1. 模型更新时出错，错误信息：`Unable to create an object of type 'CatalogDbContext'. For the different patterns supported at design time, see https://go.microsoft.com/fwlink/?linkid=851728`

   检查并更改使用同一版本的 `Microsoft.EntityFrameworkCore.XXX` 

2. 通过 `Swagger` 界面调用接口出错，错误信息：`System.UserFriendlyException: Please select a product category at Masa.EShop.Service.Catalog.Services.CatalogItemService.CreateProductAsync(CCreateProductCommand command) in E:\Temp\EShop\Masa.EShop.Service.Catalog\Services\CatalogItemService.cs:line`

   检查传参是否正确，默认提供参数是无法通过参数验证的

3. 所有参数都非空后仍然出错，错误信息：` Error: response status is 500Response bodyDownload Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details. ---> Microsoft.Data.Sqlite.SqliteException (0x80004005): SQLite Error 1: 'no such table: Catalog'.` 

   外键约束出错，输入错误的`CatalogBrandId`或`CatalogTypeId` （`CatalogBrandId`: `31b1c60b-e9c3-4646-ac70-09354bdb1522`，`CatalogTypeId`: 1）

4. 运行时出错，错误信息：`SQLite Error 1: no such table: CatalogBrand. at Microsoft.Data.Sqlite.SqliteException.ThrowExceptionForRC(Int32 rc, sqlite3 db) at Microsoft.Data.Sqlite.SqliteCommand`

   未进行数据库迁移并更新数据库，请参考文档执行数据库迁移即可

## 总结

通过 `MasaDbContext` 我们做到了数据的持久化，也支持查询已删除的产品，已经完成了基本要求，后续教程所使用的技术可以帮助我们的项目有更好的读性能、维护更方便、关注点分离，聚焦核心领域等