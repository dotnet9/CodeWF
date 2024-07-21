# ID Generator（ID 生成器） - 有序Guid生成器

## 概述

通常我们喜欢使用 `GUID` 类型，因为它可以在数据保存之前就得到**主键**，而不是必须要提交保存后才可以知道**主键**，这对于一些场景是十分有效的。

但默认 `GUID` 生成的是不连续的，当用它作为数据库的主键id使用时会带来严重的性能问题，因此我们提供了 `ISequentialGuidGenerator` ，用于生成连续的 `GUID`

## 使用

1. 安装 `Masa.Contrib.Data.IdGenerator.SequentialGuid`

   ```shell 终端
   dotnet add package Masa.Contrib.Data.IdGenerator.SequentialGuid
   ```

2. 注册 `GUID` 生成器

   ```csharp 终端
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddSequentialGuidGenerator();
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
           Id = IdGeneratorFactory.SequentialGuidGenerator.NewId();
       }
   }
   ```
   :::
   ::: code-group-item 通过 DI 获取

   ```csharp Program.cs
   app.MapGet("/getid", (ISequentialGuidGenerator generator) => { return generator.NewId(); });
   ```
   :::
   ::::

## 其它

在使用有序 `GUID` 生成器时，根据使用不同的数据库选择传入不同的策略配置（**SequentialGuidType**）:

* SequentialAsString: MySQL、PostgreSql
* SequentialAsBinary: Oracle
* SequentialAtEnd: SqlServer (默认)

以 `MySQL` 数据库为例：

```csharp Program.cs l:2
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSequentialGuidGenerator(SequentialGuidType.SequentialAsString);
```

