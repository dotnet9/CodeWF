# 单点登录（SSO）-- future

> 本文默认已经在MASA Auth 后台创建好Client

单点登录在一个客户端的多个应用之间共享登录态。用户只需要登录一次就可以访问客户端下的其他系统。

## 对接SSO

安装包`Masa.Utils.Security.Authentication.OpenIdConnect`

添加OpenId Connect身份验证

```csharp
builder.Services.AddMasaOpenIdConnect(MasaOpenIdConnectOptions:masaOpenIdConnectOptions);
```

MasaOpenIdConnectOptions介绍：

* `Authority` `string` 类型，
* `ClientId` `string` 类型
* `ClientSecret` `string` 类型
* `Scopes` `List<string>` 类型

AddMasaOpenIdConnect方法专门为MASA Stack封装的方法，AddOpenIdConnect方法内针对MASA Stack业务固定编码，同时注入了`IJwtTokenValidator`和`IIdentityProvider`的实现，IJwtTokenValidator为Jwt Token 有效性验证类，IIdentityProvider为Token和User操作类。

> IdentityProvider 依赖`identitymodel`实现。

## 获取Token

```csharp
IIdentityProvider _identityProvider;
var tokenResponse = await _identityProvider.RequestTokenAsync(TokenProfile:tokenProfile);
```

TokenProfile类型介绍：

* `GrantType` string 类型
* `Parameters` List<KeyValuePair<string, string>>类型

## 获取用户信息

```csharp
var userResponse = await _identityProvider.GetUserInfoAsync(string:token);
```

根据`access token`获取用户基本信息

## Token验证

```csharp
IJwtTokenValidator _jwtTokenValidator;
var result = await _jwtTokenValidator.ValidateAccessTokenAsync(string:assessToken,string?:refreshToken);
```

## 用户注销

> Blazor Server 操作HttpContext存在限制，所以需要通过新建Controller或cshtml页面进行登陆和退出操作。

```csharp
SignOut("OpenIdConnect", "Cookies")
```
