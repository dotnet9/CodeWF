# 数据 - Entity Framework Core (EFCore)

提供了基于 [`Entity Framework Core`](https://learn.microsoft.com/zh-cn/ef/core/) 的数据访问技术，它不依赖任何的 DBMS ，根据实际使用的 DBMS 引用安装对应的包即可，目前支持：

* [SqlServer](#SqlServer)
* [Pomelo.MySql](#Pomelo.MySql)：如果您使用的是mysql，建议使用
* [MySql](#MySql)
* [Sqlite](#Sqlite)
* [Cosmos](#Cosmos)
* [InMemory](#InMemory)
* [Oracle](#Oracle)
* [PostgreSql](#PostgreSql)

## 使用

1. 安装 **Masa.Contrib.Data.EFCore.XXX** ，以 **SqlServer** 数据库为例：

   ```shell 终端
   dotnet add package Masa.Contrib.Data.EFCore.SqlServer
   ```

   > 不同的数据库在使用上差别不大，仅需要更换引用的包以及替换注册数据上下文时使用数据库代码即可

2. 创建 DbContext 

   与直接使用`DbContext`类似，但它<font color=Red>需要继承 MasaDbContext\<TDbContext\></font> 或 <font color=Red>MasaDbContext</font>

     :::: code-group
     ::: code-group-item 方案1：MasaDbContext<TDbContext> (推荐)

   ```csharp Infrastructure/CatalogDbContext.cs
   public class CatalogDbContext : MasaDbContext<CatalogDbContext>
   {
       public CatalogDbContext(MasaDbContextOptions<CatalogDbContext> masaDbContextOptions) : base(masaDbContextOptions)
       {
       }
   }
   ```

     :::
     ::: code-group-item 方案2：MasaDbContext

   ```csharp Infrastructure/CatalogDbContext.cs
   public class CatalogDbContext : MasaDbContext
   {
       public CatalogDbContext(MasaDbContextOptions<CatalogDbContext> masaDbContextOptions)
           : base(masaDbContextOptions)
       {
   
       }
   }
   ```

     :::
     ::::

   <app-alert type="warning" content="在最新版本中，MasaDbContext支持使用无参构造函数，但必须要重载OnConfiguring"></app-alert>

   <font Color=Red>继承 MasaDbContext 的数据上下文查询默认不跟踪</font>，如需修改为全局跟踪，需[自行配置](/framework/building-blocks/data/faq#efcore)

   > 默认查询不跟踪，可以提高查询性能

3. 注册DbContext

   注册数据上下文通常使用以下两种方式：

   * 不指定数据库连接字符串地址

     :::: code-group
     ::: code-group-item 1. 配置 appsettings.json

     ```json appsettings.json
     {
       "ConnectionStrings": {
         "DefaultConnection": "server=localhost;uid=sa;pwd=P@ssw0rd;database=catalog"
       }
     }
     ```

     :::
     ::: code-group-item 2. 注册MasaDbContext

     ```csharp Program.cs l:3-6
     var builder = WebApplication.CreateBuilder(args);
     
     builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder =>
     {
         optionsBuilder.UseSqlServer();
     });
     
     var app = builder.Build();
     
     app.Run();
     ```

     :::
     ::::

   * 指定数据库连接字符串地址

     ```csharp Program.cs l:3-6
     var builder = WebApplication.CreateBuilder(args);
     
     builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder =>
     {
         optionsBuilder.UseSqlServer("{Replace-Your-ConnectionString}");
     });
     
     var app = builder.Build();
     
     app.Run();
     ```

## 高阶用法

### 无参构造函数

新版本的 `MasaDbContext` 支持其<font color=Red>派生类（子类）使用无参数的构造函数</font>，但其派生类必须<font color=Red>重写 OnConfiguring</font>方法，以 **SqlServer** 数据库为例，完整代码如下：

1. 安装 `Masa.Contrib.Data.EFCore.SqlServer`

   ```shell 终端
   dotnet add package Masa.Contrib.Data.EFCore.SqlServer
   ```

2. 创建 `CatalogDbContext`

   ```csharp Infrastructure/CatalogDbContext.cs l:3-6
   public class CatalogDbContext : MasaDbContext<CatalogDbContext>
   {
       protected override void OnConfiguring(MasaDbContextOptionsBuilder optionsBuilder)
       {
           optionsBuilder.UseSqlite("Data Source=test.db;");
       }
   }
   ```

3. 注册 `MasaDbContext`

   ```csharp Program.cs l:3
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddMasaDbContext<CatalogDbContext>();
   
   var app = builder.Build();
   
   app.Run();
   ```

虽然 `MasaDbContext` 支持无参数构造函数创建数据上下文，但我们不推荐使用，原因如下：

1. OnConfiguring 方法中不支持启用软删除等，如果需要使用软删除，除了自定义数据上下文<font Color=Red>需要重载 OnConfiguring</font>，还需要在<font Color=Red>注册MasaDbContext时启用过滤</font>

   ```csharp Program.cs l:3
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder => optionsBuilder.UseFilter());
   
   var app = builder.Build();
   
   app.Run();
   ```

   > 软删除、数据过滤等功能由 `Masa.Contrib.Data.Contracts` 提供

2. 在集成事件总线、隔离性等组件中需要得到默认数据库连接字符串地址，还需额外配置:

   ```csharp Program.cs
   builder.Services.Configure<ConnectionStrings>(connectionString =>
   {
       connectionString.DefaultConnection = "{Replace-Your-ConnectionString}";
   });
   ```

### 选项模式

如果不希望使用 **本地配置文件** 保存数据库地址，也不希望将地址在代码中硬编码，那通过选项模式将会使得它们变得更简单

```csharp Program.cs l:3-6
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ConnectionStrings>(connectionString =>
{
    connectionString.DefaultConnection = "{Replace-Your-ConnectionString}";
});

builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder =>
{
    optionsBuilder.UseSqlServer();
});

var app = builder.Build();

app.Run();
```

> [分布式配置中心](/framework/building-blocks/configuration/dcc) 通过几行代码就能实现选项模式，通过这个能力我们可以将数据库地址等信息存储到 [分布式配置中心](/framework/building-blocks/configuration/dcc) ，由运维统一管理所有配置

## 其它数据库

不同数据库的连接字符串略有差别，可参考[文档](https://www.connectionstrings.com)选择对应的数据库字符串即可

### SqlServer

  :::: code-group
  ::: code-group-item 1. 安装包
  ``` shell 终端
  dotnet add package Masa.Contrib.Data.EFCore.SqlServer
  ```
  :::
  ::: code-group-item 2. 配置 appsettings.json
  ```json appsettings.json l:2-4
  {
    "ConnectionStrings": {
      "DefaultConnection": "server=localhost;uid=sa;pwd=P@ssw0rd;database=catalog"
    }
  }
  ```
  :::
  ::: code-group-item 3. 注册 MasaDbContext

  ```csharp Program.cs l:3
  var builder = WebApplication.CreateBuilder(args);
  
  builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder => optionsBuilder.UseSqlServer());
  
  var app = builder.Build();
  
  app.Run();
  ```
  :::
  ::::

### Pomelo.MySql

  :::: code-group
  ::: code-group-item 1. 安装包
  ``` shell 终端
  dotnet add package Masa.Contrib.Data.EFCore.Pomelo.MySql
  ```
  :::
  ::: code-group-item 2. 配置 appsettings.json
  ```json appsettings.json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;port=3306;Database=identity;Uid=myUsername;Pwd=P@ssw0rd;"
    }
  }
  ```
  :::
  ::: code-group-item 3. 注册 MasaDbContext
  ```csharp Program.cs l:3
  var builder = WebApplication.CreateBuilder(args);
  
  builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder => optionsBuilder.UseMySql(new MySqlServerVersion("5.7.26"));
  
  var app = builder.Build();
  
  app.Run();
  ```
  :::
  ::::

> 基于 [`Pomelo.EntityFrameworkCore.MySql`](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql) 的扩展，如果您使用的是 mysql ，建议使用它

### MySql

  :::: code-group
  ::: code-group-item 1. 安装包
  ``` shell 终端
  dotnet add package Masa.Contrib.Data.EFCore.MySql
  ```
  :::
  ::: code-group-item 2. 配置 appsettings.json
  ```json appsettings.json l:2-4
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;port=3306;Database=identity;Uid=myUsername;Pwd=P@ssw0rd;"
    }
  }
  ```
  :::
  ::: code-group-item 3. 注册 MasaDbContext
  ```csharp Program.cs l:3
  var builder = WebApplication.CreateBuilder(args);
  
  builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder => optionsBuilder.UseMySQL());
  
  var app = builder.Build();
  
  app.Run();
  ```
  :::
  ::::

基于[`MySql.EntityFrameworkCore`](https://www.nuget.org/packages/MySql.EntityFrameworkCore)的扩展，不推荐使用

### Sqlite

  :::: code-group
  ::: code-group-item 1. 安装包
  ``` shell 终端
  dotnet add package Masa.Contrib.Data.EFCore.Sqlite
  ```
  :::
  ::: code-group-item 2. 配置 appsettings.json
  ```json appsettings.json l:2-4
  {
    "ConnectionStrings": {
      "DefaultConnection": "Data Source=test.db;"
    }
  }
  ```
  :::
  ::: code-group-item 3. 注册 MasaDbContext
  ```csharp Program.cs l:3
  var builder = WebApplication.CreateBuilder(args);
  
  builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder => optionsBuilder.UseSqlite());
  
  var app = builder.Build();
  
  app.Run();
  ```
  :::
  ::::

### Cosmos

  :::: code-group
  ::: code-group-item 1. 安装包
  ``` shell 终端
  dotnet add package Masa.Contrib.Data.EFCore.Cosmos
  ```
  :::
  ::: code-group-item 2. 配置 appsettings.json

  ```json appsettings.json l:2-4
  {
    "ConnectionStrings": {
      "DefaultConnection": "AccountKey=AccountKey;AccountEndpoint=AccountEndpoint;Database=Database"
    }
  }
  ```
  :::
  ::: code-group-item 3. 注册 MasaDbContext
  ```csharp Program.cs l:3
  var builder = WebApplication.CreateBuilder(args);
  
  builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder => optionsBuilder.UseCosmos());
  
  var app = builder.Build();
  
  app.Run();
  ```
  :::
  ::::

### InMemory

  :::: code-group
  ::: code-group-item 1. 安装包
  ``` shell 终端
  dotnet add package Masa.Contrib.Data.EFCore.InMemory
  ```
  :::
  ::: code-group-item 2. 配置 appsettings.json
  ```json appsettings.json l:2-4
  {
    "ConnectionStrings": {
      "DefaultConnection": "identity"
    }
  }
  ```
  :::
  ::: code-group-item 3. 注册 MasaDbContext
  ```csharp Program.cs l:3
  var builder = WebApplication.CreateBuilder(args);
  
  builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase());
  
  var app = builder.Build();
  
  app.Run();
  ```
  :::
  ::::

### Oracle

  :::: code-group
  ::: code-group-item 1. 安装包
  ``` shell 终端
  dotnet add package Masa.Contrib.Data.EFCore.Oracle
  ```
  :::
  ::: code-group-item 2. 配置 appsettings.json
  ```json appsettings.json l:2-4
  {
    "ConnectionStrings": {
      "DefaultConnection": "Data Source=MyOracleDB;Integrated Security=yes;"
    }
  }
  ```
  :::
  ::: code-group-item 3. 注册 MasaDbContext
  ```csharp Program.cs l:3
  var builder = WebApplication.CreateBuilder(args);
  
  builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder => optionsBuilder.UseOracle());
  
  var app = builder.Build();
  
  app.Run();
  ```
  :::
  ::::

### PostgreSql

  :::: code-group
  ::: code-group-item 1. 安装包
  ``` shell 终端
  dotnet add package Masa.Contrib.Data.EFCore.PostgreSql
  ```
  :::
  ::: code-group-item 2. 配置 appsettings.json
  ```json appsettings.json l:2-4
  {
    "ConnectionStrings": {
      "DefaultConnection": "Host=myserver;Username=sa;Password=P@ssw0rd;Database=identity;"
    }
  }
  ```
  :::
  ::: code-group-item 3. 注册 MasaDbContext
  ```csharp Program.cs l:3
  var builder = WebApplication.CreateBuilder(args);
  
  builder.Services.AddMasaDbContext<CatalogDbContext>(optionsBuilder => optionsBuilder.UseNpgsql());
  
  var app = builder.Build();
  
  app.Run();
  ```
  :::
  ::::

## 友情提示

* 本地配置中，`ConnectionStrings` 仅支持 字符串类型的键值对集合

  正确：

  ```json appsettings.json l:2-5
  {
    "ConnectionStrings": {
      "DefaultConnection": "server=localhost;uid=sa;pwd=P@ssw0rd;database=identity",
      "ReadConnection": "server=localhost;uid=sa;pwd=P@ssw0rd;database=identity2"
    }
  }
  ```

  <font Color=Red>错误写法：</font>

  ```json appsettings.json l:4-6
  {
    "ConnectionStrings": {
      "DefaultConnection": "server=localhost;uid=sa;pwd=P@ssw0rd;database=identity",
      "ReadConnection": [
        "server=localhost;uid=sa;pwd=P@ssw0rd;database=identity2"
      ]
    }
  }
  ```
  
  or
  
  ```json appsettings.json l:4-6
  {
    "ConnectionStrings": {
      "DefaultConnection": "server=localhost;uid=sa;pwd=P@ssw0rd;database=identity",
      "ReadConnection": {
        "ConnectionString": "server=localhost;uid=sa;pwd=P@ssw0rd;database=identity2"
      }
    }
  }
  ```
  
  