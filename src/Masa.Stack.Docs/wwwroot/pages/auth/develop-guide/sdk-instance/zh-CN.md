## 简介

通过注入 `IAuthClient` 接口，调用对应 `Service` 获取 `Auth SDK` 提供的能力。

`SDK` 依赖 `IMultiEnvironmentUserContext` 获取当前用户，所有用到当前用户 `ID` 的方法均不用传递改值，通过 `IMultiEnvironmentUserContext` 解析获取用户信息。

添加 `Auth SDK` 前应确保添加了 `Masa.Contrib.Authentication.Identity.XXXX`，否则会抛出异常。

## 服务介绍

`Auth SDK` 包含一下几个大类的服务

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

## 使用介绍

### 安装 Auth 包

```csharp 终端
donet add package Masa.Contrib.StackSdks.Auth
```

### 注册 Auth 服务

```csharp Program.cs 
builder.Services.AddAuthClient("http://authservice.com");
```

> `http://authservice.com` 为 `Auth` 后台服务地址

### 依赖注入 IAuthClient

```csharp 
var app = builder.Build();

app.MapGet("/GetTeams", ([FromServices] IAuthClient authClient) =>
{
    return authClient.TeamService.GetAllAsync();
});

app.Run();
```
