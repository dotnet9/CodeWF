# MASA Stack接入文档

## 一、PM接入流程
项目管理：整合系统也是使用MASA Stack服务组件的基础，它将贯穿至所有基础服务。这是必须要存在的

### 1. 演示环境地址

- https://pm-demo.masastack.com

### 2. 添加环境

   - 选择打开**Overview**左导菜单
   - 点击**新建环境** ，名称输入’**Production**‘，关联集群选择**Defalut**

### 3. 新建项目
   - 选择**Production**环境**Default**集群

- 点击**新建项目** 

    > 名称：随意（唯一,如MASA.IoT）  
    > 团队：可选（配了团队可以按照团队进行筛选项目）  
    > ID：必填（业务标识Key，如MASA-IoT）  
    > 类型：Operator（类型数据可在DCC系统的标签管理中配置）

### 4. 新建应用

- 点击**新建应用**

    > 名称：输入`MASA.IoT.Admin`  
    > 类型：UI项目选择`UI`，Api服务选择`Service`  
    > ID：输入`masa-iot-admin`  
    > 其他输入选项默认即可。

### 5. SDK接入：

   `MASA.PM` 提供了 SDK 以支持获取 `PM` 系统的数据。通过引入 `Masa.Contrib.StackSdks.Pm` SDK，可以调用 `PM` 的 `EnvironmentService`、`ClusterService`、`ProjectService`、`AppService` 来获取环境数据、集群数据、项目数据和应用数据。

   ``` plain
   IPmClient
   ├── EnvironmentService                  环境服务
   ├── ClusterService                      集群服务
   ├── ProjectService                      项目服务
   ├── AppService                          应用服务
   ```

  **场景：**

   `MASA.Auth` 切换环境时需要调用 `MASA.PM` 的 SDK 去获取相应的环境列表数据。

   `MASA.Auth` 配置应用的权限时需要通过 `MASA.PM` 的 SDK 去获取相应的应用数据。

  **示例：**

   ``` ps
   dotnet add package Masa.Contrib.StackSdks.Pm
   ```

   ``` csharp
   builder.Services.AddPmClient("Pm服务地址");
   
   var app = builder.Build();
   
   app.MapGet("/GetProjectApps", ([FromServices] IPmClient pmClient) =>
   {
       return pmClient.ProjectService.GetProjectAppsAsync("development");
   });
   
   app.Run();
   ```

## 二、DCC接入流程

###  1. 演示环境地址

- https://dcc-demo.masastack.com

### 2. 新建配置对象

- 点击项目下的应用进入配置详情页
    
- 点击**新建** ，填写配置对象信息
    
> 名称  ：`唯一`  
> 其他选项默认即可。
    
- 新建完成之后编辑配置对象，填入内容，并保存

### 3. 发布配置对象

- 点击`发布`按钮，填写发布信息

> 名称  ：必填  
> 其他选项默认即可。
> 只有发布了的配置对象才能通过sdk去获取

### 4. SDK接入
 - `MASA.DCC` 提供了两个 `SDK`，一个是 `Masa.Contrib.Configuration.ConfigurationApi.Dcc` 用来获取和管理你的配置信息。另一个是 `Masa.Contrib.StackSdks.Dcc` 用来获取标签信息。
 - 通过 `DCC` 扩展 `IConfiguration` 管理远程配置的能力。而这不单依赖于 `DCC` 的 `SDK`，还需要依赖`MasaConfiguration`。`MasaConfiguration` 把配置分为本地节点和远程节点，而 `DCC` 就是远程节点。

```csharp
IConfiguration
├── Local                                本地节点（固定）
├── ConfigurationApi                     远程节点（固定 Dcc扩展其能力）
│   ├── AppId                            Replace-With-Your-AppId
│   ├── AppId ├── Redis                  自定义节点
│   ├── AppId ├── Redis ├── Host         参数
```

安装包

```shell 终端
dotnet add package Masa.Contrib.Configuration //MasaConfiguration的核心
dotnet add package Masa.Contrib.Configuration.ConfigurationApi.Dcc //由 DCC 提供远程配置的能力
```

1. 配置 `DCC` 所需参数（远程能力）

   ```json appsettings.json
   {
     "DccOptions": {
       //Dcc服务地址
       "ManageServiceAddress": "",
       //Redis节点（因为Dcc通过Redis来提供远程配置的能力）
       "RedisOptions": {
         "Servers": [
           {
             "Host": "",
             "Port": ""
           }
         ],
         "DefaultDatabase": 0,
         "Password": ""
       },
       //应用Id
       "AppId": "Masa_Dcc_Test",
       //环境
       "Environment": "Development",
       //配置访问的对象列表
       "ConfigObjects": [ "AppSettings" ],
       //加密秘钥
       "Secret": "",
       //集群
       "Cluster": "Default"
     }
   }
   
   ```

2. 注册 `MasaConfiguration`，并使用 `DCC`

   ```csharp Program.cs
   builder.AddMasaConfiguration(configurationBuilder =>
   {
       configurationBuilder.UseDcc()
   });
   ```

3. 获取配置

   ```csharp
   using Masa.BuildingBlocks.Configuration;
   using Masa.Contrib.Configuration.ConfigurationApi.Dcc;
   
   /// <summary>
   /// 一个测试的获取配置的Controller
   /// </summary>
   [ApiController]
   [Route("[controller]/[action]")]
   public class DccConfigController : ControllerBase
   {
       private readonly IMasaConfiguration _configuration;
       public DccConfigController(IMasaConfiguration configuration)
       {
           _configuration = configuration;
       }
   
       [HttpGet]
       public string GetConfig()
       {
           //获取当前App的配置(如果想获取PublicConfig可以通过GetPublic())
           IConfiguration configuration = _configuration.ConfigurationApi.GetDefault();
           //AppSettings：配置项名称，ConnectionStrings:Db：json path
           string config = configuration.GetSection("AppSettings:ConnectionStrings:Db").Get<string>();
           return config;
       }
   }
   ```

## 三、Auth接入流程

权限认证：可接管所有服务的单点登录、菜单权限、Token权限认证等。它是很强大的存在

### 1. 演示环境地址

- https://auth-demo.masastack.com

### 2. 新建应用菜单权限

- 2.1 选择左道菜单依次选择打开**角色权限**-> **权限**页面

- 2.2 点击**PM项目下拉菜单** ，选择PM项目中刚刚新建的**MASA.IoT**选项

- 2.3 鼠标移到**MASA.IoT.Admin**点击`新建` 然后愉快的创建菜单权限和元素权限

    > Key   ：`显示名称`   
    > Code  ：`全局唯一`  
    > Icon  ：可选   
    > Url   ：可选（菜单元素需要填写跳转地址）  
    > 其他选项默认即可。

### 3. 新建单点登录客户端

- 选择左道菜单依次选择打开**单点登录**-> **客户端**页面
          

    > ClientId ：`全局唯一`  
    > ClientName ：`显示名称`    
    > ClientUri  ：可选   
    > CallBackRedirectUri ：授权回调**登录成功**地址，可配置多环境  
    > *如本机开发*：**http://localhost:19000/signout-oidc**  
    > *如线上测试*：**https://masastack.com/signout-oidc**      
    > PostLogoutRedirectUri ：授权回调**退出登录**地址，可配置多环境  
    > *如本机开发*：**http://localhost:19000/signout-callback-oidc**  
    > *如线上测试*：**https://masastack.com/signout-callback-oidc**  
    > 其他选项默认即可。

### 4. SDK接入

通过注入 `IAuthClient` 接口，调用对应 `Service` 获取 `Auth SDK` 提供的能力。`SDK` 依赖 `IMultiEnvironmentUserContext` 获取当前用户，所有用到当前用户 `ID` 的方法均不用传递改值，通过 `IMultiEnvironmentUserContext` 解析获取用户信息。添加 `Auth SDK` 前应确保添加了 `Masa.Contrib.Authentication.Identity.XXXX`，否则会抛出异常。

```csharp
IAuthClient
├── UserService                     用户服务
├── SubjectService                  全局搜索用户、角色、团队、组织架构
├── TeamService                     团队服务
├── PermissionService               权限、菜单服务
├── CustomLoginService              自定义登录服务
├── ThirdPartyIdpService            获取支持的第三方平台数据
└── ProjectService                  全局导航服务
```
> 所有接口均通过 `HTTP` 封装的方式实现，后期可能会调整部分接口直接读取 `Redis` 缓存。
安装包：
```c#
builder.Services.AddAuthClient("http://authservice.com");
```
> `http://authservice.com` 为 `Auth` 后台服务地址
使用示例：
```csharp
var app = builder.Build();

app.MapGet("/GetTeams", ([FromServices] IAuthClient authClient) =>
{
    return authClient.TeamService.GetAllAsync();
});

app.Run();
```

 ## 四、使用Masa.Stack.Components全局托管BlazorUI项目

使用Masa.Stack.Components可托管左道菜单-顶部导航-I18语言切换-用户个人中心

### 1. 安装包

```csharp
dotnet add package Masa.Stack.Components
```

### 2. 添加appsettings.json配置

```json
"OIDC": {
    "Service": "https://auth-service-demo.masastack.com/",//auth服务地址
    "Authority": "https://sso-demo.masastack.com",//sso地址
    "ClientId": "masa-iot-admin",//客户端Id
    "ClientSecret": ""
}
```
```json
"DccOptions": {
    "ManageServiceAddress": "http://localhost:8890",
    "RedisOptions": {
    "Servers": [
        {
        "Host": "replace-your-host",
        "Port": "3306"
        }
    ],
    "DefaultDatabase": 0,
    "Password": "password"
    },
    "ConfigObjectSecret": "Secret"
}
```

### 3. 添加Program.cs引用

``` csharp
//截止到1.0.0-preview.22版本“AddMasaStackComponentsForServer”内置启用Dcc，默认读取环境变量内的Dcc配置，这里开发环境强制本地配置文件，线上任何环境都需要在k8s里面增加Dcc环境变量配置

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddMasaConfiguration(builder => builder.UseDcc());
    builder.Services.AddDaprStarter(builder.Configuration.GetSection("DaprOptions"));
}
builder.Services.AddMasaOpenIdConnect(AppSettings.GetModel<MasaOpenIdConnectOptions>("OIDC"));

string? authHost = builder.Environment.IsProduction() ? null : AppSettings.Get("OIDC:Service");

builder.AddMasaStackComponentsForServer("wwwroot/i18n", authHost); //注册Masa.Stack.Components服务
```

### 4. 修改MainLayout.razor

**替换以下代码**
```csharp
@using Masa.Stack.Components
@using Microsoft.AspNetCore.Components.Rendering
@inherits LayoutComponentBase

<SLayout AppId="masastack.com" ShowBreadcrumbs WhiteUris="_whiteUrls"
        Logo="logo_full.svg"
        MiniLogo="logo_full.svg">
    <MContainer Fluid Class="pa-6">
        @Body
    </MContainer>
</SLayout>
```

### 5. 修改_Layout.cshtml

   在**head**标签引入css包

   ```html
   <link href="_content/Masa.Blazor/css/masa-blazor.min.css" rel="stylesheet">
   <link href="css/materialdesign/v6.x/css/materialdesignicons.min.css" rel="stylesheet">
   <link href="_content/Masa.Stack.Components/css/app.css" rel="stylesheet">
   <link href="css/material/icons.css" rel="stylesheet">
   <link href="css/fontawesome/v5.15.4/css/all.min.css" rel="stylesheet">
   <link href="https://cdn.masastack.com/npm/animate.css/4.1.1/animate.min.css" rel="stylesheet" />
   <link href="css/masa-blazor-pro.css" rel="stylesheet" />
   ```

   在**body**标签引入js包

   ```html
   <script src="_content/BlazorComponent/js/blazor-component.js"></script>
   <script src="js/echarts/5.1.1/echarts.min.js"></script>
   <script src="_content/Masa.Stack.Components/js/components.js"></script>
   <script src="_content/MASA.Blazor.Experimental.Components/js/experimental.js"></script>
   ```

### 6. 修改App.razor

   **替换以下代码**

   ```csharp
   @using Masa.Contrib.Service.Caller.Authentication.Standard;
   @inject Masa.Contrib.StackSdks.Caller.TokenProvider TokenProvider
   
   <CascadingAuthenticationState>
       <Microsoft.AspNetCore.Components.Routing.Router AdditionalAssemblies="additionalAssemblies" AppAssembly="@typeof(App).Assembly">
           <Found Context="routeData">
               <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
               <FocusOnNavigate RouteData="@routeData" Selector="h1" />
           </Found>
           <NotFound>
               <PageTitle>Not found</PageTitle>
               <LayoutView Layout="@typeof(MainLayout)">
                   <p role="alert">Sorry, there's nothing at this address.</p>
               </LayoutView>
           </NotFound>
       </Microsoft.AspNetCore.Components.Routing.Router>
   </CascadingAuthenticationState>
   
   @code {
   
       private List<Assembly> additionalAssemblies = new();
   
       public App()
       {
           var masaStackComponentsAssembly = typeof(Masa.Stack.Components.UserCenter).Assembly;
           additionalAssemblies.Add(masaStackComponentsAssembly);
       }
   
       [Parameter]
       public Masa.Contrib.StackSdks.Caller.TokenProvider InitialState { get; set; } = null!;
   
       protected override Task OnInitializedAsync()
       {
           TokenProvider.AccessToken = InitialState.AccessToken;
           TokenProvider.RefreshToken = InitialState.RefreshToken;
           TokenProvider.IdToken = InitialState.IdToken;
           return base.OnInitializedAsync();
       }
   }
   ```

### 7. 添加PermissionCode.cs常量类

**这里需要把元素的Code码添加为静态变量，供页面检查元素使用**

```csharp
public class PermissionCodes
{

    public readonly static string AppId = "masa-iot-admin";

    #region 产品中心

    /// <summary>
    /// 产品管理
    /// </summary>
    public static readonly string Product_List = "0029000200010001";

    /// <summary>
    /// 同步
    /// </summary>
    public static readonly string Synchronize = "00290002000100010001";

    ...
}
```

### 8. 实现IPermissionValidator自定义权限校验接口

   **创建PermissionValidator.cs继承IPermissionValidator接口**

   ```csharp
   public class PermissionValidator : IPermissionValidator
   {
       private readonly IUserContext _userContext;
       private readonly IDistributedCacheClient _distributedCacheClient;
       private readonly ICallerFactory _callerFactory;
   
       public PermissionValidator(IUserContext userContext, IDistributedCacheClient distributedCacheClient, ICallerFactory callerFactory)
       {
           _userContext = userContext;
           _distributedCacheClient = distributedCacheClient;
           _callerFactory = callerFactory;
       }
   
       /// <summary>
       /// 元素权限校验方法
       /// </summary>
       public bool Validate(string code, ClaimsPrincipal user)
       {
           try
           {
               var userId = _userContext.GetUserId<Guid>();
               List<string>? permissions = null;
   
               try
               {
                   permissions = _distributedCacheClient.Get<List<string>>(userId.ToString());
               }
               catch (Exception ex)
               {
               }
   
               if (permissions == null)
               {
                   this.GetPermissionsAsync();
   
                   permissions = _distributedCacheClient.Get<List<string>>(userId.ToString());
               }
   
               return permissions.Any(per => per == code);
           }
           catch (Exception)
           {
               return false;
           }
       }
   
       /// <summary>
       /// 获取元素权限存到Redis缓存五分钟
       /// </summary>
       public void GetPermissionsAsync()
       {
           var userId = _userContext.GetUserId<Guid>();
           var caller = _callerFactory.Create("masa.contrib.basicability.auth");
           var permissions = Task.Run(async () =>
           {
               var reponse = await caller.GetAsync($"api/permission/element-permissions?appId={PermissionCodes.AppId}&userId={userId}");
               return await reponse.Content.ReadFromJsonAsync<List<string>>();
           }).Result;
   
           _distributedCacheClient.Set<List<string>>(userId.ToString(), permissions, new CacheEntryOptions()
           {
               AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
           });
       }
   }
   ```

   **修改Program.cs注入单例**

   ``` csharp
   builder.Services.AddScoped<IPermissionValidator, PermissionValidator>();
   ```

### 9. 使用PermissionView组件实现元素权限控制

   ```
   <PermissionView Code="@PermissionCodes.Agent_List_Add">
       <MButton Color="primary" Class="ml-6 rounded-lg btn small-btn" OnClick="OnAdd">
            <MIcon Left>mdi-plus</MIcon><span>新增</span>
       </MButton>
   </PermissionView>
   ```

## 五、MASA Stack服务组件线上K8s环境变量配置

线上环境在目前阶段此项属于必须项

### 1. 添加资源配置映射

- 选择菜单选项依次选择打开**资源**-> **配置映射**->**添加配置映射**页面

    > 名称：masastack  
    > 命名空间：选择项目所在的（如：masa.iot）

- 点击确定保存
- 返回上一页，选择刚刚masa.iot命名空间下的`masastack` ，点击 `查看/编辑YAML`

``` yml
//添加data以下参数，并修改一下参数
data:
    ADMIN_PWD: admin123
CLUSTER: Default

    //根据具体情况修改
    CONNECTIONSTRING: '{"Server": "redis-host",
        "Port": 3433,"Database":"","UserId": "masastack","Password":"password"}'
    DCC_SECRET: secret
    DOMAIN_NAME: masastack.com
    ELASTIC: '{"Nodes":["http://masastack-es.masastack:9200"],"Index":"user_dev"}'
    ENVIRONMENT: Production
IS_DEMO: "false"

    //`服务地址域名前缀`： pm-server.masastack.com
MASA_SERVER: '{"pm":{"server":"pm-server"},"dcc":{"server":"dcc-server"},"auth":{"server":"auth-server"},"mc":{"server":"mc-server"},"scheduler":{"server":"scheduler-server","worker":"scheduler-worker"},"tsc":{"server":"tsc-server"},"alert":{"server":"alert-server"}}'

    //`服务地址域名前缀`： pm-ui.masastack.com
MASA_UI: '{"pm":{"ui":"pm-ui"},"dcc":{"ui":"dcc-ui"},"auth":{"ui":"auth-ui","sso":"auth-sso"},"mc":{"ui":"mc-ui"},"scheduler":{"ui":"scheduler-ui"},"tsc":{"ui":"tsc-ui"},"alert":{"ui":"alert-ui"}}'

    //`服务地址域名后缀`： pm-server.masastack.com
    NAMESPACE: masastack.com
    OTLP_URL: http://otel-collector.masastack
    
    //根据具体情况修改
    REDIS: '{"RedisHost": "redis-host", "RedisPort":
        6379, "RedisDb": 0,"RedisPassword": "your-password"}'
    TLS_NAME: masastack.com
    VERSION: 1.0-Preview1
```
  
### 2. 添加资源配置映射
    
- 回到服务列表找到项目选择**升级**  
- 打开`环境变量`卡片，点击**添加附加资源**

> 类型选择：Config Map  
> 源：选择masastack  
> 键：All  
> 前缀：空  