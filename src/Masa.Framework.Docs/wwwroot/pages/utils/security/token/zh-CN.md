# 安全 - JWT 加密与验证

## 概述

根据 `JWT` 方案生成对应的 `Token` 并提供验证合法性方法

## 功能

* [生成Token](#CreateToken)
* [验证Token合法性](#IsValid)

## 使用

1. 安装 `Masa.Utils.Security.Token` 包

   ```shell 终端
   dotnet add package Masa.Utils.Security.Token
   ```

2. 注册 `JWT` 扩展

   ```csharp
   services.AddJwt(options => {
       options.Issuer = "{Replace-Your-Issuer}";
       options.Audience = "{Replace-Your-Audience}";
       options.SecurityKey = "{Replace-Your-SecurityKey}";
   });
   ```

3. 创建 `Token`

   ```csharp
   var param = "{Replace-Your-Param}";
   var token = JwtUtils.CreateToken(param, TimeSpan.FromSeconds(60));
   ```

4. 验证 `token`

   ```csharp
   var param = "{Replace-Your-Param}";
   var token = "{Replace-Your-Token}";
   if(!JwtUtils.IsValid(token, param))
   {
       throw new UserFriendlyException("Verification Failed");
   }
   ```

## 源码解读

* CreateToken：创建 `Token`，在 `timeout` 时间后失效
* CreateToken：根据传入的 `Claim` 集合创建 `Token`，在 `timeout` 时间后失效
* IsValid：判断 `Token` 是否合法