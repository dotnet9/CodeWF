# LDAP - 概述

用于提供支持 LDAP 协议兼容的目录服务器的扩展

目前的提供者有：

* [Novell](/framework/utils/ldap/novell): 基于 [Novell.Directory.Ldap.NETStandard](https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard)的实现，适用于任何LDAP协议兼容的目录服务器 (包括 Microsoft Active Directory)

## 功能介绍

### ILdapFactory

工厂类，只提供一个 `CreateProvider` 方法返回 `ILdapProvider` 类型。

### ILdapProvider

`Ldap` 功能提供者，可以直接通过 `DI` 获得也可以通过 `ILdapFactory` 创建返回，提供以下功能：

* GetGroupAsync(string groupName):根据分组名获取分组
* GetUsersInGroupAsync(string groupName):根据分组名获取分组下用户
* GetUsersByEmailAddressAsync(string emailAddress):根据邮件获取用户
* GetUserByUserNameAsync(string userName):根据用户名获取用户
* GetAllUserAsync():获取所有用户
* GetPagingUserAsync(int pageSize):分页获取用户
* AddUserAsync(LdapUser user, string password):添加域用户并指定密码
* DeleteUserAsync(string distinguishedName):根据专有名删除用户
* AuthenticateAsync(string distinguishedName, string password):根据专有名验证密码是否正确