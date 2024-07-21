# 隔离性 - 多租户

## 概述

是一种软件体系结构，其中<font Color=Red> 单个软件实例 </font>可以 <font Color=Red>为多个租户提供服务 </font>。租户可以自定义应用程序的某个部分，例如用户界面或业务规则的颜色，但他们无法自定义应用程序的代码。查看在维基百科中的 [定义](https://zh.wikipedia.org/wiki/Multitenancy)

## 使用

1. 安装多租户

   ```shell 终端
   dotnet add package Masa.Contrib.Isolation.MultiTenant
   ```

2. 注册多租户

   ```csharp Program.cs l:2-5,9
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddIsolation(isolationBuilder =>
   {
       isolationBuilder.UseMultiTenant();
   });
   
   var app = builder.Build();
   
   app.UseIsolation();
   
   app.Run();
   ```

3. 获取当前租户

   ```csharp Program.cs l:11-14
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddIsolation(isolationBuilder =>
   {
       isolationBuilder.UseMultiTenant();
   });
   
   var app = builder.Build();
   
   app.UseIsolation();
   
   app.MapGet("/", (IMultiTenantContext multiTenantContext) =>
   {
       return multiTenantContext.CurrentTenant?.Id ?? "Empty";
   });
   
   app.Run();
   ```

4. 设置当前租户

   ```csharp Program.cs l:11-19
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddIsolation(isolationBuilder =>
   {
       isolationBuilder.UseMultiTenant();
   });
   
   var app = builder.Build();
   
   app.UseIsolation();
   
   app.MapGet("/", (IMultiTenantSetter multiTenantSetter, IMultiTenantContext multiTenantContext) =>
   {
       var oldTenantId = multiTenantContext.CurrentTenant?.Id ?? "空";
   
       multiTenantSetter.SetTenant(new Tenant(Guid.NewGuid().ToString()));//设置当前租户id, 仅对当前请求生效
   
       var newTenantId = multiTenantContext.CurrentTenant?.Id ?? "空";
       return $"old: {oldTenantId}, new: {newTenantId}";
   });
   
   app.Run();
   ```

## 高阶用法

### 自定义多租户类型

默认多租户 `ID` 的类型为 `Guid`，如果希望更改多租户 `ID` 的类型，有以下两种方式，例如：将租户 `ID` 改为 `int` 类型

:::: code-group
::: code-group-item 方案1
```csharp Program.cs l:7
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddIsolation(isolationBuilder =>
{
    isolationBuilder.UseMultiTenant();
}, options =>
{
    options.MultiTenantIdType = typeof(int);
});

var app = builder.Build();

app.UseIsolation();

app.Run();
```
:::
::: code-group-item 方案2
```csharp Program.cs l:5
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IsolationOptions>(options =>
{
    options.MultiTenantIdType = typeof(int);
});

builder.Services.AddIsolation(isolationBuilder =>
{
    isolationBuilder.UseMultiTenant();
});

var app = builder.Build();

app.UseIsolation();

app.Run();
```
:::
::::

<app-alert type="warning" content="修改多租户 `ID` 类型后，在定义实体时，需要将继承的 `IMultiTenant` 改为 `IMultiTenant<TMultiTenantIdType>`，更多信息可查看[文档](/framework/building-blocks/data/orm-efcore)"></app-alert>

### 自定义多租户解析器

```csharp
using Masa.Contrib.Isolation.Parser;

namespace WebApplication1.Parser;

public class CustomMultiTenantParserProvider : IParserProvider
{
    public string Name => "CustomMultiTenantParser";

    public Task<bool> ResolveAsync(HttpContext? httpContext, string key, Action<string> action)
    {
        var multiTenantId = "The value of the multi-tenant id";//The value of the multi-tenant id can be obtained by parsing the httpContext
        action.Invoke(multiTenantId);

        if (multiTenantId.IsNullOrWhiteSpace())
            return Task.FromResult(false);

        return Task.FromResult(true);
    }
}
```

### 编排多租户解析器

```csharp Program.cs l:5-8
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIsolation(isolationBuilder =>
{
    isolationBuilder.UseMultiTenant(new List<IParserProvider>()
    {
        new CustomMultiTenantParserProvider()
    });
});

var app = builder.Build();

app.UseIsolation();

app.Run();
```

> 使用 **UseMultiTenant** 方法时可重新编排解析器，并且需要传入完整的解析器集合，它将覆盖默认解析器

### 自定义多租户参数名

```csharp Program.cs l:5
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIsolation(isolationBuilder =>
{
    isolationBuilder.UseMultiTenant("{Custom MultiTenant Name}");
});

var app = builder.Build();

app.UseIsolation();

app.Run();
```

### 配置规则

```json appsettings.json l:2-19
{
  "Isolation":{
    "ConnectionStrings":[
      {
        "TenantId":"00000000-0000-0000-0000-000000000002",
        "Score": 100,
        "Data":{
          "ConnectionString": "server=localhost,1674;uid=sa;pwd=P@ssw0rd;database=identity;"
        }
      },
      {
        "TenantId":"00000000-0000-0000-0000-000000000003",
        "Score": 100,
        "Data":{
          "ConnectionString": "server=localhost,1672;uid=sa;pwd=P@ssw0rd;database=identity;"
        }
      }
    ]
  }
}
```

* ConnectionStrings：Db连接字符串配置 (节点名与组件有关，比如使用 **分布式 Redis 缓存**时，此节点名默认为： **RedisConfig**，支持修改节点名)

* TenantId：支持具体租户 `ID` 或者 `*`，不使用多租户时可删除此节点

  * 当其值为*时，代表无论租户 `ID` 是多少，当前数据都满足

* Score：分值 (当多个配置都满足条件时，选择使用分值最高的配置，当分值也相同时则取第一条满足配置的数据，默认：100)

* Data：组件的配置信息 (它是一个对象，其对象的配置信息与组件的配置信息一致)

  > 根据使用的构建块，查询对应的文档来修改读取组件的配置节点名

## 源码解读

### 多租户解析器

<font Color=Red>默认多租户提供了7个解析器</font>，其中租户参数名默认为: <font Color=Red>__tenant</font>，执行顺序为：

* CurrentUserTenantParseProvider: 通过从当前登录用户信息中获取租户信息
* HttpContextItemParserProvider: 通过请求的 `HttpContext` 的 `Items` 属性获取租户信息
* QueryStringParserProvider: 通过请求的 `QueryString` 获取租户信息
* FormParserProvider: 通过 `Form` 表单获取租户信息
* RouteParserProvider: 通过路由获取租户信息
* HeaderParserProvider: 通过请求头获取租户信息
* CookieParserProvider: 通过 `Cookie` 获取租户信息

> 多租户将根据以上解析器顺序依次执行解析，直到解析成功
