# 实战教程 - 第一章: 服务端

## 概述

本章将通过 **MASA Framework** 提供的[**MinimalAPIs（最小API）**](/framework/building-blocks/minimal-apis)对外提供产品的增删改查服务（基于内存数据源）

## 开始

1. 新建<font color=Red>ASP.NET Core 空项目</font>`Masa.EShop.Service.Catalog`用于<font color=Red>提供 API 服务</font>

   ```shell 终端
   dotnet new web -o Masa.EShop.Service.Catalog
   dotnet new classlib -o Masa.EShop.Contracts.Catalog
   dotnet sln EShop-MinimalAPIs-Blazor.sln add Masa.EShop.Service.Catalog/Masa.EShop.Service.Catalog.csproj
   dotnet sln EShop-MinimalAPIs-Blazor.sln add Masa.EShop.Contracts.Catalog/Masa.EShop.Contracts.Catalog.csproj
   ```

2. 选中 `Masa.EShop.Service.Catalog` 项目并安装 `Masa.Contrib.Service.MinimalAPIs`

   ```shell 终端
   cd Masa.EShop.Service.Catalog
   dotnet add package Masa.Contrib.Service.MinimalAPIs -v 1.0.0
   ```

   或者直接修改项目文件为:

   ```xml Masa.EShop.Service.Catalog.csproj
   <Project Sdk="Microsoft.NET.Sdk.Web">
   
     <PropertyGroup>
       <TargetFramework>net6.0</TargetFramework>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
     </PropertyGroup>
   
     <ItemGroup>
       <PackageReference Include="Masa.Contrib.Service.MinimalAPIs" Version="$(MasaFrameworkPackageVersion)" />
     </ItemGroup>
   
   </Project>
   ```

   > 后续教程中如果不特殊标记类库名称，均指的是`Masa.EShop.Service.Catalog`

3. 注册 [MinimalAPIs (最小API)](/framework/building-blocks/minimal-apis)

   ```csharp Program.cs l:3
   var builder = WebApplication.CreateBuilder(args);
   
   var app = builder.AddServices();
   
   app.MapGet("/", () => "Hello World!");
   
   app.Run();
   ```
   
4. 创建`CatalogItemService` (产品服务), 并需 <font Color=Red>继承</font> `ServiceBase`

   > 提供产品的增删改查
   
   ```csharp Services/CatalogItemService.cs
   using System.Linq.Expressions;
   using Masa.EShop.Contracts.Catalog.Dto;
   using Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   using Masa.Utils.Models;
   
   namespace Masa.EShop.Service.Catalog.Services;
   
   public class CatalogItemService : ServiceBase
   {
       private readonly List<CatalogListItemDto> _data = new();
   
       public Task<IResult> GetAsync(Guid id)
       {
           if (id == Guid.Empty)
               throw new UserFriendlyException("Please enter the ProductId");
   
           var catalogItem = _data.FirstOrDefault(item => item.Id == id && !item.IsDeleted);
           if (catalogItem == null)
               throw new UserFriendlyException("Product doesn't exist");
   
           return Task.FromResult(Results.Ok(catalogItem));
       }
   
       /// <summary>
       /// `PaginatedListBase` is provided by **Masa.Utils.Models.Config**, if you want to use it, please install `Masa.Utils.Models.Config`
       /// </summary>
       /// <param name="name"></param>
       /// <param name="page"></param>
       /// <param name="pageSize"></param>
       /// <returns></returns>
       public Task<IResult> GetItemsAsync(
           string? name = null,
           int page = 1,
           int pageSize = 10)
           => GetItemsAsync(name, page, pageSize, false);
   
       /// <summary>
       /// `PaginatedListBase` is provided by **Masa.Utils.Models.Config**, if you want to use it, please install `Masa.Utils.Models.Config`
       /// </summary>
       /// <param name="name"></param>
       /// <param name="page"></param>
       /// <param name="pageSize"></param>
       /// <returns></returns>
       public Task<IResult> GetRecycleItemsAsync(
           string? name = null,
           int page = 1,
           int pageSize = 10)
           => GetItemsAsync(name, page, pageSize, true);
   
       private Task<IResult> GetItemsAsync(
           string? name,
           int page,
           int pageSize,
           bool isDeleted)
       {
           if (page <= 0)
               throw new UserFriendlyException("Page must be greater than 0");
   
           if (pageSize <= 0)
               throw new UserFriendlyException("PageSize must be greater than 0");
   
           Expression<Func<CatalogListItemDto, bool>> condition = item => item.IsDeleted == isDeleted;
           condition = condition.And(!name.IsNullOrWhiteSpace(), item => item.Name.Contains(name!));
   
           var total = _data.Where(condition.Compile()).LongCount();
           var list = _data.Where(condition.Compile()).Skip((page - 1) * pageSize).Take(pageSize).ToList();
   
           var pageData = new PaginatedListBase<CatalogListItemDto>()
           {
               Total = total,
               TotalPages = (int)Math.Ceiling((double)total / pageSize),
               Result = list
           };
           return Task.FromResult(Results.Ok(pageData));
       }
   
       public Task<IResult> CreateProductAsync(CreateProductCommand command)
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
   
           _data.Add(new CatalogListItemDto()
           {
               Id = Guid.NewGuid(),
               Name = command.Name,
               Price = command.Price,
               PictureFileName = command.PictureFileName ?? "default.png",
               CatalogBrandId = command.CatalogBrandId,
               CatalogTypeId = command.CatalogTypeId,
               Stock = command.Stock
           });
           
           //todo: Notify warehouse clerks of new products
           
           return Task.FromResult(Results.Accepted());
       }
   
       public Task<IResult> DeleteProductAsync(Guid id)
       {
           if (id == Guid.Empty)
               throw new UserFriendlyException("Please enter the ProductId");
   
           var catalogItem = _data.FirstOrDefault(item => item.Id == id && !item.IsDeleted);
           if (catalogItem == null)
               throw new UserFriendlyException("Product doesn't exist");
   
           catalogItem.IsDeleted = true;
           return Task.FromResult(Results.Accepted());
       }
   }
   ```
   
   > `Masa.EShop.Service.Catalog`需引用类库`Masa.EShop.Contracts.Catalog`
   >
   > `Masa.EShop.Service.Catalog`需安装`Masa.Utils.Extensions.Expressions`
   
   CreateProductCommand（创建商品）、CatalogItemDto（商品详情）、CatalogListItemDto（商品列表）:
   
   :::: code-group
   ::: code-group-item CreateProductCommand
   
   ```csharp Application/Catalogs/Commands/CreateProductCommand.cs
   namespace Masa.EShop.Service.Catalog.Application.Catalogs.Commands;
   
   public record CreateProductCommand
   {
       public string Name { get; set; } = default!;
   
       public Guid CatalogBrandId { get; set; }
   
       public int CatalogTypeId { get; set; }
   
       public decimal Price { get; set; }
   
       public string? PictureFileName { get; set; }
   
       public int Stock { get; set; }
   }
   ```
   :::
   ::: code-group-item CatalogItemDto
   ```csharp Masa.EShop.Contracts.Catalog/Dto/CatalogItemDto.cs
   namespace Masa.EShop.Contracts.Catalog.Dto;
   
   public class CatalogItemDto
   {
       public Guid Id { get; set; }
   
       public string Name { get; set; } = string.Empty;
   
       public Guid CatalogBrandId { get; set; }
   
       public int CatalogTypeId { get; set; }
   
       public decimal Price { get; set; }
   
       public string PictureFileName { get; set; } = "default.png";
   }
   ```
   :::
   ::: code-group-item CatalogListItemDto
   ```csharp Masa.EShop.Contracts.Catalog/Dto/CatalogListItemDto.cs
   namespace Masa.EShop.Contracts.Catalog.Dto;
   
   public class CatalogListItemDto
   {
       public Guid Id { get; set; }
   
       public string Name { get; set; } = null!;
   
       public decimal Price { get; set; }
   
       public string PictureFileName { get; set; } = "";
   
       public int CatalogTypeId { get; set; }
   
       public Guid CatalogBrandId { get; set; }
   
       public int Stock { get; set; }
   
       public bool IsDeleted { get; set; }
   }
   ```
   :::
   ::::
   
5. 注册Swagger、并使用SwaggerUI

   :::: code-group
   ::: code-group-item 安装Swagger包

   ```shell 终端
   dotnet add package Swashbuckle.AspNetCore
   ```

   :::
   ::: code-group-item 修改 Program.cs，注册并使用Swagger

   ```csharp Program.cs l:5-6,16-17
   var builder = WebApplication.CreateBuilder(args);
   
   #region Register Swagger
   
   builder.Services.AddEndpointsApiExplorer();
   builder.Services.AddSwaggerGen();
   
   #endregion
   
   var app = builder.AddServices();
   
   #region Use Swagger
   
   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger();
       app.UseSwaggerUI();
   }
   
   #endregion
   
   app.MapGet("/", () => "Hello World!");
   
   app.Run();
   ```
   :::
   ::::

   最终的文件夹/文件结构应该如下所示：

   <div>
     <img alt="MinimalAPIs" src="https://cdn.masastack.com/framework/tutorial/mf-part-1/minimal-api.png"/>
   </div>

   
   > **Masa.EShop.Contracts.Catalog**类库是产品的规约，用于存放产品服务与其它项目共享的文件
   >
   > 解决方案文件夹架构可以通过Visual Studio调整得到（通过终端命令创建相对繁琐，不再赘述，因此命令行创建出的项目是并行排列）

## Swagger UI

启动模板配置为使用[Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)运行[swagger UI](https://swagger.io/tools/swagger-ui/)，运行应用程序 （Masa.EShop.Service.Catalog），并在浏览器中输入https://localhost:XXX/swagger/(用当前项目的端口替换XXX) 

你会看到当前项目所有的API服务,它们都是[RESTful](https://learn.microsoft.com/zh-cn/azure/architecture/best-practices/api-design)风格的API:

![Swagger UI](https://cdn.masastack.com/framework/tutorial/mf-part-1/swagger-ui.png)

我们可以通过Swagger的UI更方便的测试API

> 修改启动配置，增加配置: `"launchUrl": "swagger"`

```json Properties/launchSettings.json l:8
{
  "profiles": {
    "Masa.EShop.Service.Catalog": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7170;http://localhost:5193",
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## 总结

通过[**MinimalAPIs**](/framework/building-blocks/minimal-apis)快速创建一个API服务，它不仅使用方便，并且比**MVC**有着更高的性能