# 隔离性 - 多环境 

## 概述

仅需极少数代码就可以帮助开发者部署一份应用服务后支持被多个环境使用

> 数据隔离，共享应用服务

## 使用

1. 安装多环境

   ```shell 终端
   dotnet add package Masa.Contrib.Isolation.MultiEnvironment
   ```

2. 注册多环境

   ```csharp Program.cs l:2-5,9
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddIsolation(isolationBuilder =>
   {
       isolationBuilder.UseMultiEnvironment();
   });
   
   var app = builder.Build();
   
   app.UseIsolation();
   
   app.Run();
   ```

3. 获取当前环境

   ```csharp Program.cs l:11-14
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddIsolation(isolationBuilder =>
   {
       isolationBuilder.UseMultiEnvironment();
   });
   
   var app = builder.Build();
   
   app.UseIsolation();
   
   app.MapGet("/", (IMultiEnvironmentContext multiEnvironmentContext) =>
   {
       return multiEnvironmentContext.CurrentEnvironment ?? "Empty";
   });
   
   app.Run();
   ```

4. 设置当前环境

   ```csharp Program.cs l:11-19
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddIsolation(isolationBuilder =>
   {
       isolationBuilder.UseMultiEnvironment();
   });
   
   var app = builder.Build();
   
   app.UseIsolation();
   
   app.MapGet("/", (IMultiEnvironmentSetter multiEnvironmentSetter, IMultiEnvironmentContext multiEnvironmentContext) =>
   {
       var oldEnvironment = multiEnvironmentContext.CurrentEnvironment ?? "empty";
   
       multiEnvironmentSetter.SetEnvironment("dev");//Set the current environment to dev, only valid for the current request
   
       var newEnvironment = multiEnvironmentContext.CurrentEnvironment ?? "empty";
       return $"old: {oldEnvironment}, new: {newEnvironment}";
   });
   
   app.Run();
   ```

## 高阶用法

### 自定义环境解析器

```csharp
public class CustomMultiEnvironmentParserProvider : IParserProvider
{
    public string Name => "CustomMultiEnvironment";

    public Task<bool> ResolveAsync(HttpContext? httpContext, string key, Action<string> action)
    {
        var multiEnvironment = "The value of the multi-environment id";//The value of multiple environments can be parsed and obtained according to httpContext or other methods
        action.Invoke(multiEnvironment);

        if (multiEnvironment.IsNullOrWhiteSpace())
            return Task.FromResult(false);

        return Task.FromResult(true);
    }
}
```

### 编排环境解析器

```csharp Program.cs l:5-8
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIsolation(isolationBuilder =>
{
    isolationBuilder.UseMultiEnvironment(new List<IParserProvider>()
    {
        new CustomMultiEnvironmentParserProvider()
    });
});

var app = builder.Build();

app.UseIsolation();

app.Run();
```

> 使用 **UseMultiEnvironment** 方法时可重新编排解析器，并且需要传入完整的解析器集合，它将覆盖默认解析器

### 自定义多环境参数名

```csharp Program.cs l:5
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIsolation(isolationBuilder =>
{
    isolationBuilder.UseMultiEnvironment("{Custom multi-environment parameter names}");
});

var app = builder.Build();

app.UseIsolation();

app.Run();
```

### 配置规则

针对支持通过本地配置文件的构建块，以数据上下文为例，其配置满足以下规则：

```json appsettings.json l:2-19
{
  "Isolation":{
    "ConnectionStrings":[
      {
        "Environment":"*",
        "Score": 100,
        "Data":{
          "ConnectionString": "server=localhost,1674;uid=sa;pwd=P@ssw0rd;database=identity;"
        }
      },
      {
        "Environment":"development",
        "Score": 100,
        "Data":{
          "ConnectionString": "server=localhost,1672;uid=sa;pwd=P@ssw0rd;database=identity;"
        }
      }
    ]
  }
}
```

* ConnectionStrings：Db 连接字符串配置（节点名与组件有关，比如使用 **分布式 Redis 缓存** 时，此节点名默认为：**RedisConfig** ，支持修改节点名）
* Environment：支持具体环境的值或者*，不使用多环境时可删除此节点
* Score: 分值（当多个配置都满足条件时，选择使用分值最高的配置，当分值也相同时则取第一条满足配置的数据，默认：100）

> 根据使用的构建块，查询对应的文档来修改读取组件的配置节点名

## 源码解读

### 多环境解析器

<font Color=Red>默认多环境提供了 9 个解析器</font>，其中环境参数名默认为：<font Color=Red>ASPNETCORE_ENVIRONMENT</font>，执行顺序为：

* CurrentUserEnvironmentParseProvider：通过从当前登录 **用户身份** 信息中获取环境信息 

  > 使用用户身份解析器时，配置多环境的参数名对其不生效

* HttpContextItemParserProvider：通过请求的 HttpContext 的 Items 属性获取环境信息

* QueryStringParserProvider：通过请求的 QueryString 获取环境信息

* FormParserProvider：通过 Form 表单获取环境信息

* RouteParserProvider：通过路由获取环境信息

* HeaderParserProvider：通过请求头获取环境信息

* CookieParserProvider：通过 Cookie 获取环境信息

* MasaAppConfigureParserProvider：通过全局配置参数（**MasaAppConfigureOptions**）中获取当前环境信息

  > 使用全局配置参数时，配置多环境的参数名对其不生效

* EnvironmentVariablesParserProvider：通过环境变量提供程序获取当前环境信息

  > 多环境将根据以上解析器顺序依次执行解析，直到解析成功
