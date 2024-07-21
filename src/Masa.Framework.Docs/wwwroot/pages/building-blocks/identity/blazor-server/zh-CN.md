# 用户身份 - Blazor Server

## 概述

用于 `Blazor Server` 项目, 提供的用户身份信息来源于 `HttpContext.User` 、 `AuthenticationStateProvider` 

## 使用

1. 安装 `Masa.Contrib.Authentication.Identity.BlazorServer`

   ```shell 终端
   dotnet add package Masa.Contrib.Authentication.Identity.BlazorServer
   ```

2. 注册 `MasaIdentity`

   ```csharp Program.cs
   builder.Services.AddMasaIdentity();
   ```

3. 获取用户信息

   ```csharp
   IUserContext userContext;//Get from DI
   var userId = userContext.GetUserId<Guid>();
   ```

## 高阶用法

### 更改映射关系

```csharp
services.AddMasaIdentity(option =>
{
    option.TenantId = "TenantId";// The default tenant id source is: TenantId
    option.Mapping("TrueName", "RealName"); //Add identity information 'TrueName', and set the original information to: 'RealName'
});
```