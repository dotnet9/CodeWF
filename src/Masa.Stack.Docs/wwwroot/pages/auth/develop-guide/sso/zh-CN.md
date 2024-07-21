# 单点登录（SSO）

> 本文默认已经在MASA Auth 后台已成功创建SSO Client

单点登录在一个客户端的多个应用之间共享登录态。用户只需要登录一次就可以访问客户端下的其他系统。

## 对接SSO

### Blazor Server

#### MASA Stack Component

1. 安装包 `Masa.Stack.Components`

   添加MASA Stack Component 服务
   
   ```csharp
   builder.AddMasaStackComponentsForServer();
   ```
2. 添加 OpenId Connect 身份验证
   
   ```csharp
   builder.Services.AddMasaOpenIdConnect(MasaOpenIdConnectOptions:masaOpenIdConnectOptions);
   ```
   **AddMasaOpenIdConnect** 方法专门为 MASA Stack 的封装，**AddOpenIdConnect** 方法内编码针对 MASA Stack 业务。

   **MasaOpenIdConnectOptions** 介绍：
   
   * `Authority` `string` 类型，指定 SSO 站点地址，如：`https://auth-sso-dev.masastack.com`
   * `ClientId` `string` 类型，指定客户端 Id
   * `ClientSecret` `string` 类型，指定客户端 ClientSecret，没有为空即可
   * `Scopes` `List<string>` 类型，补充应用请求的 Scope 集合。默认为 `openid` 和 `profile`，具体见 [OpenIdConnectOptions](https://github.com/dotnet/aspnetcore/blob/3ea008c80d5cc63de7f90ddfd6823b7b006251ff/src/Security/Authentication/OpenIdConnect/src/OpenIdConnectOptions.cs#L42)

   > `AddMasaOpenIdConnect` 方法专门为 MASA Stack 的封装，`AddMasaOpenIdConnect` 方法内编码针对 MASA Stack 业务。且 `Masa.Stack.Components` 内依赖了 `MASA Dcc` ,由 `Masa.Contrib.StackSdks.Config` 读取相关配置，需要配置环境变量保证正常运行，具体可参考 MASA Stack 项目 `launchSettings.json`。目前不是最终版，后期优化调整可能较大，故此处先不做过多说明。

3. 在 `Startup.cs` 中添加相关中间件

   ```csharp l:3,4
   app.UseRouting();
   
   app.UseAuthentication();
   app.UseAuthorization();
   
   app.MapBlazorHub();
   ```

#### 原生Blazor

1. 安装包 `Microsoft.AspNetCore.Authentication.OpenIdConnect`

2. 添加 OpenId Connect 身份验证
   ```csharp
   builder.Services.AddAuthentication(options =>
   {
       options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
       options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
       options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
   })
   .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
   .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
   {
       options.Authority = "https://auth-sso-dev.masastack.com/";//sso address
       options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
       options.SignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
       options.RequireHttpsMetadata = false;
       options.ClientId = "test_client"; //your client id
       options.ResponseType = OpenIdConnectResponseType.Code;
       options.Scope.Add("");//add scope 
   
       options.NonceCookie.SameSite = SameSiteMode.Unspecified;
       options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
   
       //options.ClaimActions.MapUniqueJsonKey("account", "account"); //claim map
   
       //ensure normal i use self signed certificate
       options.BackchannelHttpHandler = new HttpClientHandler
       {
           ServerCertificateCustomValidationCallback = delegate
           {
               return true;
           }
       };
   });
   
   builder.Services.AddAuthorization(options =>
   {
       // By default, all incoming requests will be authorized according to the default policy
       options.FallbackPolicy = options.DefaultPolicy;
   });
   ```
   
   > `AddOpenIdConnect` 方法 `options` 设置了很多参数，相对于 `AddMasaOpenIdConnect` 实现也删除了很多代码，可以根据业务调整代码。`AddAuthorization` 方法中使用 `DefaultPolicy` ，则所有请求默认跳转至 SSO。

3. 在 `Startup.cs` 中添加相关中间件
   ```csharp l:3,4
   app.UseRouting();
   
   app.UseAuthentication();
   app.UseAuthorization();
   
   app.MapBlazorHub();
   ```

### ASP.NET Core Web

同理 *原生Blazor* 对接 SSO

## 用户注销

`OIDC` 定义了三个规范完成用户注销动作：

**Session Management**：可选。Session 管理，规范 OIDC 服务如何管理 Session 信息。  
**Front-Channel Logout**：可选。基于前端的注销机制。  
**Back-Channel Logout**：可选。基于后端的注销机制。

对于有服务端的客户端，`IdentityServer` 提供了对 [front-channel](https://openid.net/specs/openid-connect-frontchannel-1_0.html) 规范的支持；对于基于浏览器的 javascript 客户端（例如 SPA、React、Angular 等），`IdentitySever` 提供了对 [session management](https://openid.net/specs/openid-connect-session-1_0.html) 规范的支持。

应用内增加注销代码：

```csharp
SignOut("OpenIdConnect", "Cookies")
```

> Blazor Server 操作 `HttpContext` 存在限制，所以需要通过新建 `Controller` 或 `cshtml` 页面进行登陆和退出操作。

应用端注销流程：

1. 应用调用 `SignOut` 方法，删除本地 `Cookie`，同时删除 `OIDC` 登录认证 `Cookie`
2. 上个步骤执行完，会自动跳转到身份认证服务器 `/connect/endssion` 地址
3. `/connect/endsession` 带上发起登出请求客户端应用的信息，跳转到身份服务器的登出页面 `/Account/Logout`
4. `/Account/Logout` 页面获取到 logoutId 后，删除身份服务器的登录认证 Cookie，并跳转到 `/connect/endsession/callback` 页面
5. `/connect/endsession/callback` 页面中生成一个或多个 `iframe`，通知已经登录的应用退出登录。
