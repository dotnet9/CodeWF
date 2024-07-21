# 服务调用 - HttpClient

## 概念

基于 [.NET Core 的 HttpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient) 实现的服务调用

## 使用

1. 在你的项目中安装 `Masa.Contrib.Service.Caller.HttpClient` 包

   ```shell 终端
   dotnet add package Masa.Contrib.Service.Caller.HttpClient

2. 使用 `AddAutoRegistrationCaller` 自动注册服务调用

   ```csharp Program.cs
   builder.Services.AddAutoRegistrationCaller(typeof(Program).Assembly);
3. 新建类 `UserCaller.cs`，并继承 **HttpClientCallerBase**。

   > 你也可以将 `BaseAddress` 的值放到 `appsettings.json` 配置文件中，请看[注册多个服务](#注册多个服务)

   ```csharp
   public class UserCaller : HttpClientCallerBase
   {
       protected override string Prefix { get; set; } = "api/users";
   
       protected override string BaseAddress { get; set; } = "https://backapi";
   
       public async Task<object?> GetAsync(int id)
       {
           return await Caller.GetAsync<object>($"{id}");
       }
   }
   ```

4. 在构造函数中注入 `UserCaller` 对象，就可以直接调用了

   ```csharp l:5,7,11
   [ApiController]
   [Route("api/[controller]/[action]")]
   public class OrderController : ControllerBase
   {
       private readonly UserCaller _userCaller;
   
       public OrderController(UserCaller userCaller) => _userCaller = userCaller;
   
       public async Task<IActionResult> CreateAsync(int userId)
       {
           var userObj = await _userCaller.GetAsync(userId);
           return Ok(userObj);
       }
   }
   ```

## 高阶用法

### 手动注册服务调用

我们也可以手动注册服务调用。

> 默认 AddCaller(func) 注册的 Name 为空字符串，可直接构造函数注入使用 ICaller。如果指定了 Name 名称，则需要通过 `ICallerFactory` 提供的 `Create` 方法获得

1. 使用 AddCaller 方法注册服务调用。不指定 Name 时，Name 为空

   :::: code-group
   ::: code-group-item 不指定 Name 注册
   ```csharp Program.cs
   builder.Services.AddCaller(clientBuilder =>
   {
       clientBuilder.UseHttpClient(httpClient =>
       {
           httpClient.BaseAddress = "https://ServiceAddress"; //指定API服务域名地址
           httpClient.Prefix = "api/users";//指定API服务前缀
       });
   });
   ```
   :::
   ::: code-group-item 指定 Name 注册
   ```csharp Program.cs
   builder.Services.AddCaller("ServiceName", callerBuilder =>
   {
       callerBuilder.UseHttpClient(httpClient =>
       {
           httpClient.BaseAddress = "https://ServiceAddress"; //指定API服务域名地址
           httpClient.Prefix = "api/users";//指定API服务前缀
       });
   });
   ```
   ::::

2. 在构造函数注入 ICaller 对象，或者使用 ICallerFactory 对象创建 ICaller 对象

   :::: code-group
   ::: code-group-item 直接注入 ICaller
   ```csharp
   [ApiController]
   [Route("api/[controller]/[action]")]
   public class UserController : ControllerBase
   {
       private readonly ICaller _caller;
   
       public UserController(ICaller caller) => _caller = caller;
   
       public async Task<IActionResult> CreateAsync(int userId)
       {
           var userObj = await _caller.GetAsync($"getlist?userId={userId}");
           return Ok(userObj);
       }
   }
   ```
   :::
   ::: code-group-item 使用 ICallerFactory
   ```csharp
   [ApiController]
   [Route("api/[controller]/[action]")]
   public class UserController : ControllerBase
   {
       private readonly ICallerFactory _callerFactory;
   
       public UserController(ICallerFactory callerFactory) => _callerFactory = callerFactory;
   
       public async Task<IActionResult> CreateAsync(int userId)
       {
           var caller = _callerFactory.Create("ServiceName");
           var userObj = await caller.GetAsync($"getlist?userId={userId}");
           return Ok(userObj);
       }
   }
   ```
   ::: 
   ::::

### 注册多个服务

如果我们的服务需要同时调用多个服务来完成某个功能时，那么我们可以通过 Options 模式来配置我们服务调用的 BaseAddress

1. 新增一个 `ServiceCallerOption.cs` 配置类，里面需要配置用户服务的地址和订单服务的地址

   ```csharp ServiceCallerOptions.cs
   public class ServiceCallerOptions
   {
       public string UserServiceBaseAddress { get; set; }
   
       public string OrderServiceBaseAddress { get; set; }
   }
   ```

2. 注册服务调用，并配置服务地址

   :::: code-group
   ::: code-group-item Program.cs
   ```csharp Program.cs
   builder.Services.Configure<ServiceCallerOptions>(builder.Configuration.GetSection("ServiceCaller"));
   builder.Services.AddAutoRegistrationCaller(typeof(Program).Assembly);
   ```
   :::
   ::: code-group-item appsettings.json
   ```json appsettings.json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
   
     "ServiceCaller": {
       "UserServiceBaseAddress": "https://userService",
       "OrderServiceBaseAddress": "https://orderService"
     }
   }
   ```
   :::
   ::::

3. 新增一个 `UserCaller.cs` 类，并在构造函数注入 `IOptions<ServiceCallerOptions>` 对象，然后在构造函数中初始化 BaseAddress 属性
   ```csharp UserCaller.cs
   public class UserCaller : HttpClientCallerBase
   {
       protected override string Prefix { get; set; } = "api/users";
   
       protected override string BaseAddress { get; set; }
   
       public UserCaller(IOptions<ServiceCallerOptions> options)
       {
           this.BaseAddress = options.Value.UserServiceBaseAddress;
       }
   
       public async Task<object?> GetAsync(int id)
       {
           return await Caller.GetAsync<object>($"{id}");
       }
   }
   ```

### 中间件

MASA Framework 的服务调用提供了中间件功能，在中间件中，你可以拦截所有的服务调用请求，也可以自定义逻辑阻断服务调用

![CallerMiddleware.png](https://cdn.masastack.com/framework/building-blocks/caller/CallerMiddleware.png)

1. 新增一个 TraceMiddleware 中间件并继承 ICallerMiddleware。这个中间件的作用是：将当前请求的 TraceId 传递到目标 `API` 服务器

   ```csharp TraceMiddleware.cs
   public class TraceMiddleware : ICallerMiddleware
   {
       private readonly string _traceId;
       private readonly IHttpContextAccessor _httpContextAccessor;
   
       public TraceMiddleware(IHttpContextAccessor httpContextAccessor)
       {
           _traceId = "trace-id";
           _httpContextAccessor = httpContextAccessor;
       }
   
       public Task HandleAsync(MasaHttpContext masaHttpContext, CallerHandlerDelegate next, CancellationToken cancellationToken = default)
       {
           if (!masaHttpContext.RequestMessage.Headers.Contains(_traceId) && _httpContextAccessor.HttpContext != null)
           {
               masaHttpContext.RequestMessage.Headers.Add(_traceId, _httpContextAccessor.HttpContext.Request.Headers[_traceId].ToString());
           }
           return next();
       }
   }
   ```

2. 使用中间件

   :::: code-group
   ::: code-group-item 自动注册Caller

      ```csharp
   public class UserCaller : HttpClientCallerBase
   {
       protected override string BaseAddress { get; set; } = "https://ServiceAddress";
       
       protected override string Prefix { get; set; } = "api/users";
   
       /// <summary>
       /// 重写UseHttpClientPost方法，使用自定义链路中间件
       /// </summary>
       /// <param name="masaHttpClientBuilder"></param>
       protected override void UseHttpClientPost(MasaHttpClientBuilder masaHttpClientBuilder)
       {
           masaHttpClientBuilder.AddMiddleware<TraceMiddleware>();
       }
   }
      ```

      :::
      ::: code-group-item 手动注册Caller

      ```csharp Program.cs
   builder.Services.AddCaller(clientBuilder =>
   {
       clientBuilder.UseHttpClient(httpClient =>
       {
           httpClient.BaseAddress = "https://ServiceAddress"; //指定API服务域名地址
           httpClient.Prefix = "api/users";//指定API服务前缀
       }).AddMiddleware<TraceMiddleware>();
   });
      ```

      :::
      ::::

### Xml 请求格式

对于接口传输是 Xml 协议的服务，MASA Framework 的服务调用可以直接帮你自动序列化请求数据和反序列化响应数据

1. 在项目中安装 `Masa.Contrib.Service.Caller.Serialization.Xml`

   ```shell 终端
   dotnet add package Masa.Contrib.Service.Caller.Serialization.Xml
   ```

2. 使用`Xml`请求格式
   
   :::: code-group
   ::: code-group-item 自动注册Caller
   ```csharp
   public class UserCaller : HttpClientCallerBase
   {
       protected override string BaseAddress { get; set; } = "https://ServiceAddress";
       
       protected override string Prefix { get; set; } = "api/users";
   
       /// <summary>
       /// 重写ConfigMasaCallerClient方法，并指定当前Caller使用Xml格式
       /// </summary>
       /// <param name="callerClient"></param>
       protected override void ConfigMasaCallerClient(MasaCallerClient callerClient)
       {
           callerClient.UseXml();
       }
   }
   ```
   :::
   ::: code-group-item 手动注册Caller
   ```csharp Program.cs
   builder.Services.AddCaller(clientBuilder =>
   {
       clientBuilder.UseHttpClient(httpClient =>
       {
           httpClient.BaseAddress = "https://ServiceAddress"; //指定API服务域名地址
           httpClient.Prefix = "api/users";//指定API服务前缀
           
           httpClient.UseXml();//使用Xml请求
       });
   });
   ```
   :::
   ::::

### 身份认证

当 **被调用的服务** 也需要身份认证的时候，我们可以使用服务调用的身份认证，将当前请求的认证信息传递到 **被调用的服务**。MASA Framework中服务调用的身份认证同时支持 **ASP.Net Core** 项目以及 **Blazor Server** 项目

![CallerAuthenticationMiddleware.png](https://s2.loli.net/2023/03/13/GyslLfw5O38dpKB.png)

#### AspNetCore 项目

1. 在我们的项目中安装 ```Masa.Contrib.Service.Caller.Authentication.AspNetCore``` 包

   ```shell 终端
   dotnet add package Masa.Contrib.Service.Caller.Authentication.AspNetCore
   ```

2. 使用认证

   :::: code-group
   ::: code-group-item 自动注册Caller
   ```csharp
   public class UserCaller : HttpClientCallerBase
   {
       protected override string BaseAddress { get; set; } = "https://ServiceAddress";
       
       protected override string Prefix { get; set; } = "api/users";
   
       /// <summary>
       /// 重写UseHttpClientPost方法，使用认证
       /// </summary>
       /// <param name="masaHttpClientBuilder"></param>
       protected override void UseHttpClientPost(MasaHttpClientBuilder masaHttpClientBuilder)
       {
           masaHttpClientBuilder.UseAuthentication();
       }
   }
   ```
   :::
   ::: code-group-item 手动注册Caller
   ```csharp Program.cs
   builder.Services.AddCaller(clientBuilder =>
   {
       clientBuilder.UseHttpClient(httpClient =>
       {
           httpClient.BaseAddress = "https://ServiceAddress"; //指定API服务域名地址
           httpClient.Prefix = "api/users";//指定API服务前缀
       }).UseAuthentication();
   });
   ```
   :::
   ::::

#### Balzor 项目

1. 在我们的Blazor项目中安装 ```Masa.Contrib.Service.Caller.Authentication.Standard``` 包

   ``` shell 终端
   dotnet add package Masa.Contrib.Service.Caller.Authentication.Standard
   ```

2. 注册服务调用，并使用身份认证服务

   :::: code-group
   ::: code-group-item 自动注册Caller
   ```csharp
   public class UserCaller : HttpClientCallerBase
   {
       protected override string BaseAddress { get; set; } = "https://ServiceAddress";
       
       protected override string Prefix { get; set; } = "api/users";
   
       /// <summary>
       /// 重写UseHttpClientPost方法，使用认证
       /// </summary>
       /// <param name="masaHttpClientBuilder"></param>
       protected override void UseHttpClientPost(MasaHttpClientBuilder masaHttpClientBuilder)
       {
           masaHttpClientBuilder.UseAuthentication();
       }
   }
   ```
   :::
   ::: code-group-item 手动注册Caller
   ```csharp Program.cs
   builder.Services.AddCaller(clientBuilder =>
   {
       clientBuilder.UseHttpClient(httpClient =>
       {
           httpClient.BaseAddress = "https://ServiceAddress"; //指定API服务域名地址
           httpClient.Prefix = "api/users";//指定API服务前缀
       }).UseAuthentication();
   });
   ```
   :::
   ::::

3. 修改 `_Host.cshtml` 和 `App.razor` 文件

   :::: code-group
   ::: code-group-item _Host.cshtml
   ```cshtml _Host.cshtml
   @{
       var tokenExpiry = await HttpContext.GetTokenAsync("expires_at");
       DateTimeOffset.TryParse(tokenExpiry, out var expiresAt);
   
       var accessToken = await HttpContext.GetTokenAsync("access_token");
       var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
   
       var tokenShouldBeRefreshed = accessToken != null && expiresAt < DateTime.UtcNow.AddSeconds(5);//5 Seconds is set ClockSkew,default 5 Minutes
       if (tokenShouldBeRefreshed)
       {
           await RefreshAccessTokenAsync();
       }
   
       async Task RefreshAccessTokenAsync()
       {
           var auth = await HttpContext.AuthenticateAsync();
   
           if (!auth.Succeeded)
           {
               await HttpContext.SignOutAsync();
               return;
           }
   
           if (refreshToken == null)
           {
               await HttpContext.SignOutAsync();
               return;
           }
       }
   }
   <component type="typeof(App)" render-mode="ServerPrerendered" param-Token="accessToken" />
   ```
   :::
   ::: code-group-item App.razor
   ```razor App.razor
   @using Masa.Contrib.Service.Caller.Authentication.Standard;
   @inject TokenProvider TokenProvider
   
   <Router AppAssembly="@typeof(App).Assembly">
       <Found Context="routeData">手动注册Caller
           <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
           <FocusOnNavigate RouteData="@routeData" Selector="h1" />
       </Found>
       <NotFound>
           <PageTitle>Not found</PageTitle>
           <LayoutView Layout="@typeof(MainLayout)">
               <p role="alert">Sorry, there's nothing at this address.</p>
           </LayoutView>
       </NotFound>
   </Router>
   
   @code {
       [Parameter]
       public string? Token { get; set; }
   
       protected override async Task OnInitializedAsync()
       {
           await base.OnInitializedAsync();
   
           TokenProvider.Authorization = Token;
       }
   }
   ```
   :::
   ::::

### 自定义身份认证提供服务

如果说你的应用程序有自己的身份认证逻辑，那么你可以自定义服务调用的身份认证提供服务

1. 新增认证服务，并实现 `IAuthenticationService`

   ```csharp
   public class XXXAuthenticationService: IAuthenticationService
   {
       public Task ExecuteAsync(HttpRequestMessage requestMessage)
       {
           //custom Authentication logic
   
           return Task.CompletedTask;
       }
   } 
   ```

2. 使用自定义认证服务

   :::: code-group
   ::: code-group-item 自动注册Caller
   ```csharp
   public class UserCaller : HttpClientCallerBase
   {
       protected override string BaseAddress { get; set; } = "https://ServiceAddress";
       
       protected override string Prefix { get; set; } = "api/users";
   
       /// <summary>
       /// 重写UseHttpClientPost方法，使用认证
       /// </summary>
       /// <param name="masaHttpClientBuilder"></param>
       protected override void UseHttpClientPost(MasaHttpClientBuilder masaHttpClientBuilder)
       {
           masaHttpClientBuilder.UseAuthentication(serviceProvider => new XXXAuthenticationService());
       }
   }
   ```
   :::
   ::: code-group-item 手动注册Caller
   ```csharp
   builder.Services.AddCaller(clientBuilder =>
   {
       clientBuilder.UseHttpClient(httpClient =>
       {
           httpClient.BaseAddress = "https://ServiceAddress"; //指定API服务域名地址
       }).UseAuthentication(serviceProvider => new XXXAuthenticationService());
   });
   ```
   :::
   ::::



### 配置 HttpClient

如果你希望设置超时时间，默认请求头等信息，则可通过重写 `HttpClientCallerBase` 提供的 `ConfigureHttpClient` 方法，例如:

:::: code-group
::: code-group-item 自动注册Caller
```csharp
public class GithubCaller : HttpClientCallerBase
{
    protected override string Prefix { get; set; } = "repos/masastack/MASA.Framework";

    protected override string BaseAddress { get; set; } = "https://api.github.com";
    
    /// <summary>
    /// 配置HttpClient
    /// </summary>
    /// <param name="httpClient"></param>
    protected override void ConfigureHttpClient(System.Net.Http.HttpClient httpClient)
    {
        httpClient.Timeout = TimeSpan.FromSeconds(30);//30s超时
    }
}
```
:::
::: code-group-item 手动注册Caller

```csharp
builder.Services.AddCaller(clientBuilder =>
{
    clientBuilder.UseHttpClient(httpClient =>
    {
        httpClient.BaseAddress = "https://api.github.com"; //指定API服务域名地址
        httpClient.Prefix = "repos/masastack/MASA.Framework"; //指定API服务前缀

        //配置 HttpClient
        httpClient.Configure = httpClient =>
        {
            httpClient.Timeout = TimeSpan.FromSeconds(30);//30s超时
        };
    });
});
```
:::
::::

### 扩展其它自定义处理程序

以 Yaml 为例:

1. 新建支持 Yaml 的 RequestMessage、ResponseMessage，并分别实现 IRequestMessage、IResponseMessage

   ```csharp
   public class YmlRequestMessage : IRequestMessage
   {
       public void ProcessHttpRequestMessage(HttpRequestMessage requestMessage)
       {
           //custom logic
       }
   
       public void ProcessHttpRequestMessage<TRequest>(HttpRequestMessage requestMessage, TRequest data)
       {
           //custom logic
       }
   }
   
   public class YmlResponseMessage : DefaultResponseMessage
   {
       protected override Task<TResponse?> FormatResponseAsync<TResponse>(
           HttpContent httpContent,
           CancellationToken cancellationToken = default) where TResponse : default
       {
           TResponse? response = default;
           //custom logic
   
           return Task.FromResult(response);
       }
   }
   ```

> DefaultResponseMessage 继承 `IResponseMessage`，通过继承 `DefaultResponseMessage`，我们仅需要实现将流通过 Yaml 解析为对象即可

2. 新增`MasaCallerClientExtensions`

   ```csharp
   public static class MasaCallerClientExtensions
   {
       /// <summary>
       /// Set the request handler and response handler for the specified Caller
       /// </summary>
       /// <param name="masaCallerOptions"></param>
       /// <returns></returns>
       public static MasaCallerClient UseYaml(this MasaCallerClient masaCallerClient)
       {
           masaCallerClient.RequestMessageFactory = _ => new YmlRequestMessage();
           masaCallerClient.ResponseMessageFactory = serviceProvider =>
           {
               return new YmlResponseMessage(loggerFactory);
           };
           return masaCallerClient;
       }
   }
   ```