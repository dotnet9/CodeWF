# 身份认证

## 概述

用于 OIDC 基础数据的存储和读取

## 使用

1. 安装 `Masa.Contrib.Authentication.OpenIdConnect.EFCore`

   ```shell 终端
   dotnet add package Masa.Contrib.Authentication.OpenIdConnect.EFCore
   ```

2. 注册OidcDbContext，修改

   ```csharp Program.cs
   builder.Services.AddOidcDbContext<BusinessDbContext>();
   ```

   > `BusinessDbContext` 是使用者自己业务使用的 `EFCore` 的 [DbContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext)

   如果想在项目启动时自动创建标准规范的 OIDC 基础数据,使用以下代码

   ```csharp 
   await builder.Services.AddOidcDbContext<BusinessDbContext>(async option =>
   {
       await option.SeedStandardResourcesAsync();
   });
   ```
   
   > [标准规范的 OIDC 基础数据](https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims)包含用户申明和身份资源

3. 修改 `BusinessDbContext`，重写基类 `DbContext` 的 `OnModelCreating` 方法

   ```csharp 
   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       modelBuilder.ApplyConfigurationsFromAssembly(OpenIdConnectEFCore.Assembly);
   }
   ```

4. 读取和存储数据

   ```csharp 
   IClientRepository clientRepository;//Get from DI
   await _clientRepository.AddAsync(client);//Add client data
   await _clientRepository.GetDetailAsync(clientId);//Get client data
   IUserClaimRepository userClaim;//Get from DI
   await userClaim.AddStandardUserClaimsAsync();//Batch create standard specification user declaration data
   IIdentityResourceRepository identityResourcerepository;//Get from DI
   await identityResourcerepository.AddStandardIdentityResourcesAsync();//Create standard and standardized identity resource data in batches
   ```

   > Masa.Contrib.Authentication.OpenIdConnect.EFCore 提供了`IClientRepository`、`IIdentityResourceRepository`、`IApiResourceRepository`、`IApiScopeRepository`、`IUserClaimRepository`来维护OIDC的基础数据

## 高阶用法

### 数据库操作时同步更新多级缓存

1. 安装 `Masa.Contrib.Authentication.OpenIdConnect.Cache`

   ```shell 终端 
   dotnet add package Masa.Contrib.Authentication.OpenIdConnect.Cache
   ```

2. 注册 `OidcCache`

   ```csharp Program.cs
   builder.Services.AddOidcDbContext<BusinessDbContext>();
   builder.Services.AddOidcCache();
   ```

   > `AddOidcCache()` 默认使用本地 `RedisConfig` 节点的配置，详情参考 [多级缓存](/framework/building-blocks/caching)。使用者可使用重载方法传入自定义的 `redisOption`。

### 使用多级缓存读取 OIDC 基础数据

1. 安装 `Masa.Contrib.Authentication.OpenIdConnect.Cache.Storage`

   ```shell 终端 
   dotnet add package Masa.Contrib.Authentication.OpenIdConnect.Cache.Storage
   ```

2. 注册 `OidcCacheStorage`

   ```csharp Program.cs
   builder.Services.AddOidcCacheStorage();
   ```

   > `AddOidcCacheStorage()` 默认使用本地 `RedisConfig` 节点的配置，详情参考 [多级缓存](/framework/building-blocks/caching)。使用者可使用重载方法传入自定义的 `redisOption`。

3. 读取数据

   ```csharp 
   IMasaOidcClientStore _masaOidcClientStore;//Get from DI
   await _masaOidcClientStore.FindClientByIdAsync(clientId);//Get client data
   await _masaOidcResourceStore.GetAllResourcesAsync();//Get all resource data
   ```
   
   > Masa.Contrib.Authentication.OpenIdConnect.Cache.Storage 提供了 `IMasaOidcClientStore`、`IResourceStore`来获取 `OIDC` 的基础数据