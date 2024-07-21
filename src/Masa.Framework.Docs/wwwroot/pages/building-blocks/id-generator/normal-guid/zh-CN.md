# ID Generator（ID 生成器） - 无序Guid生成器

## 概述

无序 `Guid` 生成器，用于生成全局唯一 `ID`，不过它不适用于用作数据库的主键 `ID`，如果希望作为数据库主键 `ID`，推荐使用 [sequential-guid](/framework/building-blocks/id-generator/sequential-guid)

## 使用

1. 安装`Masa.Contrib.Data.IdGenerator.NormalGuid`

   ```shell 终端
   dotnet add package Masa.Contrib.Data.IdGenerator.NormalGuid
   ```

2. 注册 `GUID` 生成器

   ```csharp 终端
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddSimpleGuidGenerator();
   ```

3. 使用 `GUID` 生成器生成 **有序的 ID**

   :::: code-group
   ::: code-group-item 通过 ID 生成器工厂创建（静态）

   ```csharp Domain/Entities/CatalogBrand.cs
   using Masa.BuildingBlocks.Data;
   
   namespace Masa.EShop.Service.Catalog.Domain.Entities;
   
   public class CatalogBrand
   {
       public Guid Id { get; set; }
       
       public string Brand { get; set; }
   
       private CatalogBrand()
       {
           Id = IdGeneratorFactory.GuidGenerator.NewId();
       }
   }
   ```
   :::
   ::: code-group-item 通过 DI 获取

   ```csharp Program.cs
   app.MapGet("/getid", (IGuidGenerator generator) => { return generator.NewId(); });
   ```
   :::
   ::::