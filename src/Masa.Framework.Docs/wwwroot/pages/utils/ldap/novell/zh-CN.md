# LDAP - Novell

## 概述

基于 [Novell.Directory.Ldap.NETStandard](https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard) 的实现，适用于任何 LDAP 协议兼容的目录服务器 (包括 Microsoft Active Directory)

## 使用

1. 安装 `Masa.Utils.Ldap.Novell`

   ```shell 终端
   dotnet add package Masa.Utils.Ldap.Novell
   ```

2. 注册 `Ldap`

   ```csharp
   services.AddLadpContext(options => {
       options.ServerAddress = "{Replace-Your-ServerAddress}";
       options.ServerPort = "{Replace-Your-ServerPort}";
       options.ServerPortSsl = "{Replace-Your-ServerPortSsl}";
       options.BaseDn = "{Replace-Your-BaseDn}";
       options.UserSearchBaseDn = "{Replace-Your-UserSearchBaseDn}";
       options.GroupSearchBaseDn = "{Replace-Your-GroupSearchBaseDn}";
       options.RootUserDn = "{Replace-Your-RootUserDn}";
       options.RootUserPassword = "{Replace-Your-RootUserPassword}";
   });
   ```

3. 获取所有用户信息

   ```csharp
   ILdapProvider ldapProvider;//由DI获取
   var allUser = await ldapProvider.GetAllUserAsync();
   ```
