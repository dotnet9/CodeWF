# 开发指南-第三方平台

第三方平台内置支持了微信、GitHub 登录配置，只需要将客户端Id和客户端密钥更改为自己的即可完成微信和GitHub登录的对接。用户也可自己配置其他实现了 OAuth 的第三方应用。

## 基础配置

### 名称

名称是唯一的，不可重复，对应 `AuthenticationBuilder.AddOAuth` 方法的 `Schem`。代码会根据名称来执行对应平台的认证过程。

### 显示名称

显示名称用于展示给用户，登录页面中的第三方登录模块中，用户可看到第三方应用对应的显示名称。

### 客户端ID

客户端ID是第三方登录需要的关键参数，用户在第三方平台开通OAuth后可获得此参数。用于标识客户端身份。

### 客户端密钥

客户端密钥是第三方登录需要的关键参数，用户在第三方平台开通OAuth后可获得此参数。用于安全加密。

### 回调地址

用户跳转到在第三方登录页面登录成功后跳转返回的地址，此地址唯一，不可重复。代码会根据此地址将对应的第三方平台返回的数据进行签名，生成 token。

### 身份认证地址

跳转到第三方平台登录页面的地址。由第三方平台提供。

### 获取Token地址

获取第三方平台用户Token的地址。由第三方平台提供。

### 获取用户信息地址

获取第三方平台用户信息的地址。由第三方平台提供。

## 高级配置

由于每个第三方的用户数据字段是不一样的，比如 A平台的 NickName 和 B平台的 DisplayName 都是用于展示用户的 Name。高级配置就是将各个平台的用户字段配置映射到 MASA Auth 用户的字段。
出于 Cookie 大小的限制，默认第三方用户的所有字段不会存入 Cookie，只有配置映射的字段才会存入 Cookie，所以需要根据需求配置自己需要的用户字段。

### 以GitHub举例

github返回的用户数据字段分别为 id、email、name、avatar_utl、login，根据数据分析后将这五个字段映射为 MASA Auth 的 sub、email、name、picture、account。

## 二次开发

由于每个第三方平台并不会完全实现OAuth的规范，所以需要对这些第三方平台进行二次开发，MASA Auth 已经封装好二次开发的基础设施代码，用户只需要对第三方平台 OAuth 认证过程中特殊化的步骤进行重新实现即可完成第三方平台的接入，并获得热更新功能。

### 以GitHub举例

#### 由于获取 github 的用户邮箱数据是单独的一个接口，所以我们要在获取用户信息的时候调用这个接口来单独获取邮箱。

1. 创建 `GitHubDemoAuthenticationOptions` 类，`GitHubDemoAuthenticationOptions` 继承 `Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions`

   ```csharp 
   using Microsoft.AspNetCore.Authentication.OAuth;

   namespace Masa.Auth.Security.OAuth.Providers.GitHub;

   public class GitHubDemoAuthenticationOptions : OAuthOptions
   {
       public GitHubDemoAuthenticationOptions()
       {
           Scope.Add("user:email");
       }

       public string UserEmailsEndpoint { get; set; } = "https://api.github.com/user/emails";
   }
   ```

2. 创建 `GitHubDemoAuthenticationHandler` 类，`GitHubDemoAuthenticationHandler` 继承 `Microsoft.AspNetCore.Authentication.OAuth.OAuthHandler<GitHubDemoAuthenticationOptions>`，由于我们要在获取用户信息的时候加上调用获取邮箱的代码，所以需要重写 `CreateTicketAsync` 方法。

   ```csharp 
   using Microsoft.AspNetCore.Authentication;
   using Microsoft.AspNetCore.Authentication.OAuth;
   using Microsoft.Extensions.Logging;
   using Microsoft.Extensions.Options;
   using System.Diagnostics.CodeAnalysis;
   using System.Net.Http.Headers;
   using System.Security.Claims;
   using System.Text.Encodings.Web;
   using System.Text.Json;

   namespace Masa.Auth.Security.OAuth.Providers.GitHub;

   public class GitHubDemoAuthenticationHandler : OAuthHandler<GitHubDemoAuthenticationOptions>
   {
       public GitHubDemoAuthenticationHandler(
           [NotNull] IOptionsMonitor<GitHubDemoAuthenticationOptions> options,
           [NotNull] ILoggerFactory logger,
           [NotNull] UrlEncoder encoder,
           [NotNull] ISystemClock clock)
           : base(options, logger, encoder, clock)
       {
       }

       protected override async Task<AuthenticationTicket> CreateTicketAsync(
           [NotNull] ClaimsIdentity identity,
           [NotNull] AuthenticationProperties properties,
           [NotNull] OAuthTokenResponse tokens)
       {
           using var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
           request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
           request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

           using var response = await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);
           if (!response.IsSuccessStatusCode)
           {
               throw new HttpRequestException("An error occurred while retrieving the user profile.");
           }

           using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted));

           var principal = new ClaimsPrincipal(identity);
           var context = new OAuthCreatingTicketContext(principal, properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);
           context.RunClaimActions();

           if (!string.IsNullOrEmpty(Options.UserEmailsEndpoint) &&
               !identity.HasClaim(claim => claim.Type == ClaimTypes.Email) &&
               Options.Scope.Contains("user:email"))
           {
               string? address = await GetEmailAsync(tokens);

               if (!string.IsNullOrEmpty(address))
               {
                   identity.AddClaim(new Claim(ClaimTypes.Email, address, ClaimValueTypes.String, Options.ClaimsIssuer));
               }
           }

           await Events.CreatingTicket(context);
           return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
       }

       protected virtual async Task<string?> GetEmailAsync([NotNull] OAuthTokenResponse tokens)
       {
           using var request = new HttpRequestMessage(HttpMethod.Get, Options.UserEmailsEndpoint);
           request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
           request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

           using var response = await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);
           if (!response.IsSuccessStatusCode)
           {
               throw new HttpRequestException("An error occurred while retrieving the email address associated to the user profile.");
           }

           using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted));

           return (from address in payload.RootElement.EnumerateArray()
                where address.GetProperty("primary").GetBoolean()
                select address.GetString("email")).FirstOrDefault();
       }
   ```

#### 根据上面步骤完成了GitHub针对OAuth实现的的重构，接下来需要将GitHub对接到MASA Auth中

1. 在 MASA Auth 后台中添加 GitHub 第三方应用,这里的客户端Id和客户端密钥用的是测试的，只能https://localhost可以用。

   ![GitHub第三方应用示例图](https://cdn.masastack.com/stack/doc/auth/develop-guide/github-demo.png)

2. 在 `Masa.Auth.Security.OAuth.Providers` 项目中添加 `GitHubDemoBuilder` 类

   ```csharp 
   namespace Masa.Auth.Security.OAuth.Providers.GitHub;

   public class GitHubDemoBuilder : IAuthenticationInstanceBuilder, IAuthenticationInject
   {
       public string Scheme { get; } = "GitHubDemo";

       public void InjectForHotUpdate(IServiceCollection serviceCollection)
       {
           serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<GitHubDemoAuthenticationOptions>, OAuthPostConfigureOptions<GitHubDemoAuthenticationOptions, GitHubDemoAuthenticationHandler>>());
       }

       public IAuthenticationHandler CreateInstance(IServiceProvider provider, AuthenticationDefaults authenticationDefaults)
       {
           var (options, loggerFactory, urlEncoder, systemClock) = CreateAuthenticationHandlerInstanceUtilities.BuilderParamter<GitHubDemoAuthenticationOptions>(provider, authenticationDefaults.Scheme);
           authenticationDefaults.BindOAuthOptions(options.CurrentValue);

           return new GitHubDemoAuthenticationHandler(options, loggerFactory, urlEncoder, systemClock);
       }

       public void Inject(AuthenticationBuilder builder, AuthenticationDefaults authenticationDefault)
       {
           throw new NotImplementedException();
       }
   }
   ```

   > 需要注意 Scheme 需要与第三方平台中添加的名称一致
