# MinimalAPI (最小API)

## 概述

什么是 [Minimal APIs](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/minimal-apis)

## 功能列表

* [服务分组](#服务分组): 将 API 服务分别写到不同的 `Service` 中
* [自动映射路由](#自动映射规则): 支持 [`RESTful`](https://docs.microsoft.com/zh-cn/azure/architecture/best-practices/api-design) 标准

## 使用

`Minimal APIs` 十分轻量，写法十分简单，可正因为如此，也给我们带来一些编码上的问题，下面我们来看一下原生 `Minimal APIs` 的写法与 `MASA Framework` 提供的 `Minimal APIs` 的写法的区别

### 原生写法

  ```csharp
  var builder = WebApplication.CreateBuilder(args);
  var app = builder.Build();
  
  app.MapGet("/api/v1/users/{id}", (Guid id)=>
  {
      // todo: Query user information
      var user = new User()
      {
          Id = id,
          Name = "Tony"
      };
      return Task.FromResult(Results.Ok(user));
  });
  
  app.MapPost("/api/v1/users", ([FromBody] UserRequest request)=>
  {
      //todo: Add user logic
      return Task.FromResult(Results.Accepted());
  });
  
  app.MapDelete("/api/v1/users/{id}",(Guid id)=>
  {
      //todo: remove user logic
      return Task.FromResult(Results.Accepted());
  });
  
  app.MapPut("/api/v1/users/{id}",(Guid id, [FromBody] EditUserRequest request)=>
  {
      //todo: modify user logic
      return Task.FromResult(Results.Accepted());
  });
  
  app.Run();
  ```

### 服务分组

原生写法会使得 `Program` 文件中充斥着大量的接口服务信息，它将不利于我们的开发工作以及后期的维护，为此 **MASA Framework** 提供了一个解决方案，它提供了`服务分组`、`路由自动注册`功能。在 `.NET7` 中将会支持 `MapGroup` ，其目的与 `BaseUri` 类似

1. 安装 Minimal API

    ```shell 终端
    dotnet add package Masa.Contrib.Service.MinimalAPIs
    ```

2. 注册 Minimal API

    :::: code-group
    ::: code-group-item 方案1
    ```csharp
    var builder = WebApplication.CreateBuilder(args);
    
    var app = builder.AddServices();//Register and map routes
    
    app.Run();
    ```
    :::
    ::: code-group-item 方案2
    ```csharp
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddMasaMinimalAPIs();//Register MinimalAPI
    
    var app = builder.Build();
    
    app.MapMasaMinimalAPIs();//Map MinimalAPI routes
    
    app.Run();
    ```
    :::
    ::::

3. 新建用户服务，**并继承`ServiceBase`**（注册路由）

    :::: code-group
    ::: code-group-item 自动注册路由(推荐)
    ```csharp
    public class UserService : ServiceBase
    {
        /// <summary>
        /// Get: /api/v1/users/{id}
        /// </summary>
        public Task<IResult> GetAsync(Guid id)
        {
            // todo: Query user information
            var user = new User()
            {
                Id = id,
                Name = "Tony"
            };
            return Task.FromResult(Results.Ok(user));
        }
    
        /// <summary>
        /// Post: /api/v1/users
        /// </summary>
        public Task<IResult> AddAsync([FromBody] UserRequest request)
        {
            //todo: Add user logic
            return Task.FromResult(Results.Accepted());
        }
    
        /// <summary>
        /// Delete: /api/v1/users/{id}
        /// </summary>
        public Task<IResult> DeleteAsync(Guid id)
        {
            //todo: remove user logic
            return Task.FromResult(Results.Accepted());
        }
    
        /// <summary>
        /// Put: /api/v1/users/{id}
        /// </summary>
        public Task<IResult> UpdateAsync(Guid id, [FromBody] EditUserRequest request)
        {
            //todo: 修改用户逻辑
            return Task.FromResult(Results.Accepted());
        }
    }
    ```
    :::
    ::: code-group-item 手动映射路由
    ```csharp
    public class UserService : ServiceBase
    {
        public UserService()
        {
            RouteOptions.DisableAutoMapRoute = true;//当前服务禁用自动注册路由
    
            App.MapGet("/api/v1/users/{id}", GetAsync);
            App.MapPost("/api/v1/users", AddAsync);
            App.MapDelete("/api/v1/users/{id}", DeleteAsync);
            App.MapPut("/api/v1/users/{id}", UpdateAsync);
        }
    
        public Task<IResult> GetAsync(Guid id)
        {
            // todo: 查询用户信息
            var user = new User()
            {
                Id = id,
                Name = "Tony"
            };
            return Task.FromResult(Results.Ok(user));
        }
    
        public Task<IResult> AddAsync([FromBody] UserRequest request)
        {
            //todo: 添加用户逻辑
            return Task.FromResult(Results.Accepted());
        }
    
        public Task<IResult> DeleteAsync(Guid id)
        {
            //todo: 删除用户逻辑
            return Task.FromResult(Results.Accepted());
        }
    
        public Task<IResult> UpdateAsync(Guid id, [FromBody] EditUserRequest request)
        {
            //todo: 修改用户逻辑
            return Task.FromResult(Results.Accepted());
        }
    }
    ```
    :::
    ::::
    
    > 默认开启自动映射，如果不需要则可通过全局配置或局部配置关闭自动映射

## 高阶用法

提供默认支持 [`RESTful`](https://docs.microsoft.com/zh-cn/azure/architecture/best-practices/api-design) 标准

### 路由配置

提供了全局配置以及服务内配置

> 优先级: 服务内(局部)配置 > 全局配置 (当服务内配置为 `null` 时，使用全局配置的值)

#### 全局配置

<div class="custom-table">

|  参数名   | 参数描述                                                                                               | 默认值                                                                               |
|  ----  |----------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------|
| DisableAutoMapRoute  | <font color=Red>禁用自动映射路由</font>                                                                    | `false`                                                                           |
| Prefix  | 前缀                                                                                                 | `api`                                                                             |
| Version  | 版本                                                                                                 | `v1`                                                                              |
| AutoAppendId  | <font color=Red>路由中是否包含 {Id} </font>，例如: /api/v1/user/{id}                                        | `true`                                                                            |
| PluralizeServiceName  | <font color=Red>服务名称是否启用复数</font>                                                                  | `true`                                                                            |
| GetPrefixes  | 用于识别当前方法类型为 `Get` 请求                                                                               | `new List<string> { "Get", "Select", "Find" }`                                    |
| PostPrefixes | 用于识别当前方法类型为 `Post` 请求                                                                              | `new List<string> { "Post", "Add", "Upsert", "Create", "Insert" }`                |
| PutPrefixes | 用于识别当前方法类型为 `Put` 请求                                                                               | `new List<string> { "Put", "Update", "Modify" }`                                  |
| DeletePrefixes | 用于识别当前方法类型为 `Delete` 请求                                                                            | `new List<string> { "Delete", "Remove" }`                                         |
| DisableTrimMethodPrefix | 禁用移除方法前缀(上方 `Get`、`Post`、`Put`、`Delete` 请求的前缀)                                                     | false                                                                             |
| MapHttpMethodsForUnmatched | 通过方法名前缀匹配请求方式失败后，路由将使用指定的HttpMethod发起请求                                                            | 支持`Post`、`Get`、`Delete`、`Put` 此方式<font color=Red> Swagger 不支持，无法正常显示 API </font> |
| Assemblies | 用于扫描服务所在的程序集                                                                                       | `MasaApp.GetAssemblies()`（全局 Assembly 集合，默认为当前域程序集集合）                             |
| RouteHandlerBuilder | 基于`RouteHandlerBuilder`的委托，可用于权限认证、[CORS](https://developer.mozilla.org/zh-CN/docs/Web/HTTP/CORS)等 | `null`                                                                            |
| EnableProperty | 启用公有属性映射                                                                                           | `false`                                                                           |

</div>

#### 服务内配置

<div class="custom-table">
  <table style='border-collapse: collapse;table-layout:fixed;width:100%'>
   <col span=6>
   <tr style="background-color:#f3f4f5; font-weight: bold">
    <td colspan=3>参数名</td>
    <td colspan=2>参数描述</td>
    <td>默认值(未赋值为null)</td>
   </tr>
   <tr>
    <td colspan=3><a id = "CustomBaseUri">BaseUri</a></td>
    <td colspan=2>根地址</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=3>ServiceName</td>
    <td colspan=2>服务名称</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=3>RouteHandlerBuilder</td>
    <td colspan=2>基于 RouteHandlerBuilder 的委托，可用于权限认证、<a href="https://developer.mozilla.org/zh-CN/docs/Web/HTTP/CORS"> CORS </a>等</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=3>RouteOptions（对象）</td>
    <td colspan=2>局部路由配置</td>
    <td> </td>
   </tr>
   <tr>
    <td rowspan=12></td>
    <td colspan=2>DisableAutoMapRoute</td>
    <td colspan=2><font color=Red>禁用自动映射路由</font></td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2>Prefix</td>
    <td colspan=2>前缀</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2>Version</td>
    <td colspan=2>版本</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2><a id ="AutoAppendId">AutoAppendId</a></td>
    <td colspan=2><font color=Red>路由中是否包含{Id}</font>，例如: /api/v1/user/{id}</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2><a id = "PluralizeServiceName">PluralizeServiceName</a></td>
    <td colspan=2><font color=Red>服务名称是否启用复数</font></td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2><a id = "GetPrefixes">GetPrefixes</a></td>
    <td colspan=2>用于识别当前方法类型为 `Get` 请求</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2>PostPrefixes</td>
    <td colspan=2>用于识别当前方法类型为 `Post` 请求</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2>PutPrefixes</td>
    <td colspan=2>用于识别当前方法类型为 `Put` 请求</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2>DeletePrefixes</td>
    <td colspan=2>用于识别当前方法类型为 `Delete` 请求</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2><a id = "DisableTrimMethodPrefix">DisableTrimMethodPrefix</a></td>
    <td colspan=2>禁用移除方法前缀(上方 `Get`、`Post`、`Put`、`Delete` 请求的前缀)</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2><a>EnableProperty</a></td>
    <td colspan=2>启用公有属性映射</td>
    <td></td>
   </tr>
   <tr>
    <td colspan=2><a id ="MapHttpMethodsForUnmatched">MapHttpMethodsForUnmatched</a></td>
    <td colspan=2>通过方法名前缀匹配请求方式失败后，路由将使用指定的 HttpMethod 发起请求 此方式<font color=Red>Swagger不支持，无法正常显示 API </font></td>
    <td></td>
   </tr>
  </table>
</div>

#### 如何使用？

  :::: code-group
  ::: code-group-item 全局配置
  ```csharp
  builder.AddServices(options =>
  {
      options.Prefix = "api";//自定义前缀
      options.DisableAutoMapRoute = true;//可通过配置true禁用全局自动路由映射或者删除此配置以启用全局自动路由映射
  })
  ```
  :::
  ::: code-group-item 服务内配置
  ```csharp
  public class ProjectService : ServiceBase
  {
      public ProjectService()
      {
          RouteOptions.Prefix = "v2";//自定义前缀
          RouteOptions.DisableAutoMapRoute = true;//可通过配置true禁用当前服务使用自动路由映射或者配置false以启用当前服务的自动路由映射
          ServiceName = "project";// 自定义服务名
      }
  }
  ```
  :::
  ::::

  > 针对早期已经使用 `Minimal API` 的开发者，不希望破坏原有的命名规则，则可以通过配置禁用自动映射或者重写路由规则以满足需要

### 特性

#### RoutePattern (路由)

用于`自定义完整路由`或自定义`路由方法名`或自定义`请求方式`

* 自定义路由

  ```csharp
  public class ProjectService : ServiceBase
  {
      [RoutePattern(pattern: "project/list")]
      public Task<List<string>> GetProjectListAsync()
      {
          var list = new List<string>()
          {
              "Auth",
              "DCC",
              "PM"
          };
          return Task.FromResult(list);
      }
  }
  ```

* 自定义方法名

  ```csharp
  public class ProjectService : ServiceBase
  {
      [RoutePattern(pattern: "list", true)]
      public Task<List<string>> GetProjectListAsync()
      {
          var list = new List<string>()
          {
              "Auth",
              "DCC",
              "PM"
          };
          return Task.FromResult(list);
      }
  }
  ```

* 自定义请求方式 

  ```csharp
  public class ProjectService : ServiceBase
  {
      [RoutePattern(HttpMethod = "Post")]
      public Task<List<string>> GetProjectListAsync()
      {
          var list = new List<string>()
          {
              "Auth",
              "DCC",
              "PM"
          };
          return Task.FromResult(list);
      }
  }
  ```

#### IgnoreRoute (忽略映射)

被标记的方法不能被自动映射为 `API` 服务

  ```csharp
  public class ProjectService : ServiceBase
  {
      public Task<List<string>> GetProjectListAsync()
      {
          var list = GetList();
          return Task.FromResult(list);
      }
  
      [IgnoreRoute]
      public List<string> GetList() => new List<string>()
      {
          "Auth",
          "DCC",
          "PM"
      };
  }
  ```

## 原理解剖

### 自动映射范围

当服务开启自动映射路由后，必须满足以下条件，方可支持自动映射

1. 方法的访问级别为 `Public`
2. 服务必须是非抽象类，抽象类将不被支持自动映射

<app-alert type="warning" content="在 ServiceBase 的派生类中，如果方法不需要对外提供 `API` 服务，则对应的访问级别建议使用 **private**、**protected** 或者 **internal**来代替 **public**，从而避免被自动映射为 `API` 服务"></app-alert>

### 自动映射规则

优先级: [自定义路由](#routepattern-8def7531) > 规则生成路由

自动映射路由生成规则: => <font color=Red>var Pattern(路由) = $"{BaseUri}/{RouteMethodName}";</font>

#### BaseUri (根地址)

* 根据规则<font Color=Red>自动生成根地址</font> (<font Color=Red>默认</font>)
  ```csharp
  根地址 = $"{前缀}/{版本}/{服务名(默认复数)}"
  ```
  * <font Color=Red>自定义根地址</font>
    ```csharp
    public class ProjectService : ServiceBase
    {
        public ProjectService() : base("/api/project")
        {
    
        }
    
        public Task<List<string>> GetProjectListAsync()
        {
            var list = new List<string>()
            {
                "Auth",
                "DCC",
                "PM"
            };
            return Task.FromResult(list);
        }
    }
    ```
  * 重写根地址规则（<font Color=Red>个性化</font>）
    ```csharp
    public class ProjectService : ServiceBase
    {
        public Task<List<string>> GetProjectListAsync()
        {
            var list = new List<string>()
            {
                "Auth",
                "DCC",
                "PM"
            };
            return Task.FromResult(list);
        }
    
        /// <summary>
        /// 重写根地址规则
        /// </summary>
        protected override string GetBaseUri(ServiceRouteOptions globalOptions, PluralizationService pluralizationService)
        {
            if (!string.IsNullOrWhiteSpace(BaseUri))
                return BaseUri;
    
            return GetType().Name.TrimEnd("Service", StringComparison.OrdinalIgnoreCase);
        }
    }
    ```

#### RouteMethodName (自定义路由方法名)

优先级: `自定义路由方法名` > `规则生成路由方法名`

  * 规则生成路由方法名（<font Color=Red>默认</font>）

    ```csharp
    var methodName = 原方法名.TrimStart("智能匹配到的请求方式前缀"，"").TrimEnd("Async"，"") + "{id}";
    ```
    
    * 可通过[`DisableTrimMethodPrefix`](#DisableTrimMethodPrefix) <font Color=Red>禁用移除前缀</font>
    * 根据参数名称匹配是否等于`id`，且未增加`[FromBodyAttribute]`、`[FromFormAttribute]`、`[FromHeaderAttribute]`、`[FromQueryAttribute]`、`[FromServicesAttribute]`特性，可通过[`AutoAppendId`](#AutoAppendId) <font Color=Red>禁用自动追加 {id}</font>

  * <a href="#routepattern-8def7531" style="color: red;">自定义路由方法名</a>
    
    ```csharp Services/ProjectService
    public class ProjectService : ServiceBase
    {
        [RoutePattern(pattern: "list", true)]
        public Task<List<string>> GetProjectListAsync()
        {
            var list = new List<string>()
            {
                "Auth",
                "DCC",
                "PM"
            };
            return Task.FromResult(list);
        }
    }
    ```

  * 重写方法名规则 (<font Color=Red>个性化</font>)
    
    ```csharp Services/ProjectService
    public class ProjectService : ServiceBase
    {
        public Task<List<string>> GetProjectListAsync()
        {
            var list = new List<string>()
            {
                "Auth",
                "DCC",
                "PM"
            };
            return Task.FromResult(list);
        }
    
        /// <summary>
        /// 重写并返回方法名
        /// </summary>
        /// <param name="methodInfo">方法信息</param>
        /// <param name="prefix">智能匹配请求方式前缀</param>
        /// <param name="globalOptions">全局配置</param>
        /// <returns></returns>
        protected override string GetMethodName(MethodInfo methodInfo, string prefix, ServiceRouteOptions globalOptions)
        {
            return methodInfo.Name;
        }
    }
    ```

### 请求方式

优先级：`自定义请求方式` > `根据方法名前缀智能匹配` > `智能匹配失败后默认配置`

#### 自定义请求方式

通过`RoutePattern`特性可自定义请求类型

```csharp Services/ProjectService
public class ProjectService : ServiceBase
{
    /// <summary>
    /// Post：/api/v1/Projects/ProjectList
    /// </summary>
    [RoutePattern(HttpMethod = "Post")]
    public Task<List<string>> ProjectListAsync()
    {
        var list = new List<string>()
        {
            "Auth",
            "DCC",
            "PM"
        };
        return Task.FromResult(list);
    }
}
```

#### 智能匹配

当方法未自定义请求类型时，我们将根据方法名前缀只能匹配请求类型，例如：

```csharp Services/ProjectService
public class ProjectService : ServiceBase
{
    /// <summary>
    /// Get: /api/v1/Projects/ProjectList
    /// </summary>
    public Task<List<string>> GetProjectListAsync()
    {
        var list = new List<string>()
        {
            "Auth",
            "DCC",
            "PM"
        };
        return Task.FromResult(list);
    }
}
```

> Get 请求默认不支持对象，如果希望支持对象，[请参考](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0#custom-binding)

由于 `GetAsync` 以 `Get` 开头，并且在 [Get 请求前缀配置](#GetPrefixes)中已经存在，因此智能匹配为`Get`请求

#### 智能匹配失败

当智能匹配失败后，我们将使用 [`MapHttpMethodsForUnmatched`](#MapHttpMethodsForUnmatched) 来映射当前 `API` 的请求类型，如果你希望当匹配失败后，默认使用 `Post` 请求，则

:::: code-group
::: code-group-item 全局配置
```csharp
builder.AddServices(options =>
{
    MapHttpMethodsForUnmatched = new[] { "Post" };//当请求类型匹配失败后，默认映射为Post请求 (当前项目范围内，除非范围配置单独指定)
})
```
:::
::: code-group-item 服务内配置
```csharp
public class ProjectService : ServiceBase
{
    public ProjectService()
    {
        RouteOptions.MapHttpMethodsForUnmatched = new[] { "Post" };//当请求类型匹配失败后，默认映射为Post请求 (当前服务范围内)
    }
}
```
:::
::::

## 常见问题

### 1. 在Swagger上不显示接口

  * 当前服务不映射为接口，无法被使用
    * 当前类<font color=Red>是抽象类</font>
    * 当前<font color=Red>方法访问级别不是 Public </font>
    * 方法上增加了特性 <font color=Red>IgnoreRoute</font>
  * 智能<font color=Red>匹配请求方式失败</font>
    * 通过[自定义路由](#自定义路由) 特性设置请求方式
        ```csharp
        public class ProjectService : ServiceBase
        {
            [RoutePattern(HttpMethod = "Get")]
            public Task<List<string>> ProjectListAsync()
            {
                var list = new List<string>()
                {
                    "Auth",
                    "DCC",
                    "PM"
                };
                return Task.FromResult(list);
            }
        }
        ```
    * 修改匹配失败后默认使用[XXX方式](#MapHttpMethodsForUnmatched)请求

        :::: code-group
        ::: code-group-item 方案1：全局配置匹配失败后使用 Get 请求
        
        ```csharp Program.cs l:4
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServices(globalRouteOptions =>
        {
            globalRouteOptions.MapHttpMethodsForUnmatched = new[] { "Get" };
        });
        ```
        :::
        ::: code-group-item 方案2：局部配置匹配失败后使用 Get 请求
        
        ```csharp Services/ProjectService.cs l:5
        public class ProjectService : ServiceBase
        {
            public ProjectService()
            {
                RouteOptions.MapHttpMethodsForUnmatched = new[] { "Get" };
            }
            
            public Task<List<string>> ProjectListAsync()
            {
                var list = new List<string>()
                {
                    "Auth",
                    "DCC",
                    "PM"
                };
                return Task.FromResult(list);
            }
        }
        ```
        :::
        ::::
    * 修改方法名或修改 [XXXPrefixes 规则](#GetPrefixes)使得方法被智能识别
    
        :::: code-group
        ::: code-group-item 方案1：修改方法名
        
        ```csharp Services/ProjectService.cs l:3
        public class ProjectService : ServiceBase
        {
            public Task<List<string>> GetProjectListAsync()
            {
                var list = new List<string>()
                {
                    "Auth",
                    "DCC",
                    "PM"
                };
                return Task.FromResult(list);
            }
        }
        ```
        :::
        ::: code-group-item 方案2：修改 `XXXPrefixes` 规则被识别为 `Get` 请求
        
        ```csharp Services/ProjectService.cs l:6-9
        public class ProjectService : ServiceBase
        {
            public ProjectService()
            {
        	    //或者通过全局配置使得对全局生效
                RouteOptions.GetPrefixes = new List<string>()
                {
                    "Project"
                };
            }
        
            public Task<List<string>> GetProjectListAsync()
            {
                var list = new List<string>()
                {
                    "Auth",
                    "DCC",
                    "PM"
                };
                return Task.FromResult(list);
            }
        }
        ```
        :::
        ::::
        
        > 针对匹配请求方式失败的方法，路由将指定为 `Map`，它支持通过 `Post`、`Get`、`Delete`、`Put` 访问，但 `Swagger` 不能识别它

### 2. 继承 ServiceBase 的派生类构造函数中获取获取到服务无法正常使用

继承 `ServiceBase` 服务不支持通过构造函数注入服务，如果你需要从 `DI` 获取指定服务，可通过<font color=Red>在方法上增加对应服务的参数类型</font>来使用、或者通过其父类 `ServiceBase` 提供的 <font color=Red>GetService<TService>()</font>、<font color=Red> GetRequiredService <TService>()</font> 来使用，例如：

  ```csharp 
public class CatalogItemService : ServiceBase
{
    private CatalogDbContext _dbContext => GetRequiredService<CatalogDbContext>();
    
    public async Task<IResult> GetListAsync()
        => Results.Ok(await _dbContext.Set<CatalogItem>().ToListAsync());
}
  ```

:::: code-group
::: code-group-item 方案1
```csharp Services/ProjectService.cs l:3
public class ProjectService : ServiceBase
{
    public Task<List<string>> GetProjectListAsync(ILogger<ProjectService> logger)
    {
        logger.LogDebug("------write log------");
        var list = new List<string>()
        {
            "Auth",
            "DCC",
            "PM"
        };
        return Task.FromResult(list);
    }
}
```
:::
::: code-group-item 方案2
```csharp Services/ProjectService.cs l:3
public class ProjectService : ServiceBase
{
    private ILogger<ProjectService> Logger => GetRequiredService<ILogger<ProjectService>>();

    public Task<List<string>> GetProjectListAsync()
    {
        Logger.LogDebug("------write log------");
        var list = new List<string>()
        {
            "Auth",
            "DCC",
            "PM"
        };
        return Task.FromResult(list);
    }
}
```
:::
::::

<app-alert type="warning" content="继承 ServiceBase 的派生类不建议从构造函数中注入服务，无论服务的生命周期是**单例**、**请求**还是**瞬态**"></app-alert>

> 继承 `ServiceBase` 类的派生类仅会在项目启动时被初始化一次，后续将不会被初始化，并且构建服务使用的 `RootServiceProvider` 与项目的 `RootServiceProvider` 不一致，部分服务对此会有影响，不推荐使用

### 3. 服务启动时出错: Body was inferred but the method does not allow inferred body parameters.

完整错误内容:

```csharp
System.InvalidOperationException: Body was inferred but the method does not allow inferred body parameters.
Below is the list of parameters that we found:

Parameter           | Source
---------------------------------------------------------------------------------
query               | Body (Inferred)


Did you mean to register the "Body (Inferred)" parameter(s) as a Service or apply the [FromService] or [FromBody] attribute?
```

<app-alert type="warning" content="检查 **ServiceBase** 的**派生类**中是否符合 **Get 请求使用对象来接收参数**，到目前为止，仅发现这一种情况会出现这类错误，它可能是因为手动映射路由、自动映射路由或者是在 `ServiceBase` 的派生类中不规范的创建方法导致"></app-alert>

我们可通过将 <font color=Red>参数信息平铺</font> 到方法上来或者 <font color=Red>增加 [FromBody] </font> 等特性来标记参数来源，例如:

:::: code-group
::: code-group-item 错误用法

```csharp
public class ProjectService : ServiceBase
{
    public Task<List<string>> GetProjectListAsync(ProjectItemQuery query)
    {
        var list = new List<string>()
        {
            "Auth",
            "DCC",
            "PM"
        };
        return Task.FromResult(list);
    }
}

public class ProjectItemQuery
{
    public string Name { get; set; }
}
```
:::
::: code-group-item 正确用法 (方案1)
```csharp
public class ProjectService : ServiceBase
{
    public Task<List<string>> GetProjectListAsync(string name)
    {
        var list = new List<string>()
        {
            "Auth",
            "DCC",
            "PM"
        };
        return Task.FromResult(list);
    }
}
```
:::
::: code-group-item 正确用法 (方案2)
```csharp
public class ProjectService : ServiceBase
{
    public Task<List<string>> GetProjectListAsync([FromBody]ProjectItemQuery query)
    {
        var list = new List<string>()
        {
            "Auth",
            "DCC",
            "PM"
        };
        return Task.FromResult(list);
    }
}

public class ProjectItemQuery
{
    public string Name { get; set; }
}
```
:::
::::

<app-alert type="warning" content="方案2与方案1的参数来源是不一致的，使用此方案时确保参数信息是通过 `Body` 进行传输"></app-alert>

> 除了以上方案之外，我们也可以通过 [自定义参数绑定](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/minimal-apis#custom-binding) 来处理 

## 相关Issues

[#2](https://github.com/masastack/MASA.Framework/issues/2)、[#241](https://github.com/masastack/MASA.Framework/issues/241)、[#428](https://github.com/masastack/MASA.Framework/issues/428)