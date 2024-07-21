# OAuth 2.0

> OAuth 2.0 是行业标准的授权协议。OAuth 2.0 侧重于客户端开发人员的简单性，同时为 Web 应用程序、桌面应用程序、移动电话和客厅设备提供特定的授权流程。该规范及其扩展正在IETF OAuth 工作组内开发。

```
+--------+                               +---------------+
|        |--(A)- Authorization Request ->|   Resource    |
|        |                               |     Owner     |
|        |<-(B)-- Authorization Grant ---|               |
|        |                               +---------------+
|        |
|        |                               +---------------+
|        |--(C)-- Authorization Grant -->| Authorization |
| Client |                               |     Server    |
|        |<-(D)----- Access Token -------|               |
|        |                               +---------------+
|        |
|        |                               +---------------+
|        |--(E)----- Access Token ------>|    Resource   |
|        |                               |     Server    |
|        |<-(F)--- Protected Resource ---|               |
+--------+                               +---------------+
```

配置OAuth第三方登录渠道，内置支持了常用的OAuth第三方渠道配置模板，包含GitHub、Wechat，也可自定义配置支持OAuth 2.0协议的第三方。

1. 使用Auth用户登录**Auth控制台**。
2. 在左侧导航栏，点击**第三方平台**。

## 搜索OAuth

OAuth平台列表以表格形式展现，有分页、模糊搜索功能。

> 模糊搜索支持名称、显示名称

![第三方平台搜索列表图](https://cdn.masastack.com/stack/doc/auth/use-guide/third-party/third-party-search.png)

## 新建OAuth

点击列表页右上角的`新建`可打开新增OAuth平台的表单窗口，表单分为基本信息和高级配置。

> 高级配置用于将第三方平台返回的用户json数据配置映射为Auth的用户数据

* **Logo**：必须上传，鼠标移上去可选择系统集成的GitHub、Wechat。
* **名称**：必填，唯一不可重复，可包含汉字、英文字母、数字，2到50个字符。
* **显示名称**：必填，可包含汉字、英文字母、数字，2到50个字符。
* **客户端ID**：必填，可包含汉字、英文字母、数字，2到50个字符。
* **客户端密钥**：必填，2到255个字符。
* **回调地址**：必填。
* **身份认证地址**：必填，Url格式。
* **获取Token地址**：必填，Url格式。
* **获取用户信息地址**：必填，Url格式。
* **自定义JSON键/值**：成对出现，键可选。

![新建第三方平台图](https://cdn.masastack.com/stack/doc/auth/use-guide/third-party/third-party-add.png)

> 高级配置，可添加自定义JSON键/值

![新建第三方平台主高级图](https://cdn.masastack.com/stack/doc/auth/use-guide/third-party/third-party-add-advanced.png)

> GitHub、Wechat选择，鼠标移到顶部的**上传Logo**上，点击GitHub或Wechat

![新建第三方平台主Logo选择图](https://cdn.masastack.com/stack/doc/auth/use-guide/third-party/third-party-add-icon.png)

![新建第三方平台主Logo选择Wechat图](https://cdn.masastack.com/stack/doc/auth/use-guide/third-party/third-party-add-wechat.png)

## 编辑OAuth

点击表格中指定行对应的操作列中的`编辑图标`可打开编辑OAuth平台的表单窗口，表单分为基本信息和高级配置。

> 名称字段不可编辑，其它字段说明同**新建OAuth**

![编辑第三方平台图](https://cdn.masastack.com/stack/doc/auth/use-guide/third-party/third-party-edit.png)

## 删除OAuth

点击表格中指定行对应的操作列中的`删除图标`可删除所在行数据，点击`确定`即可。

![删除第三方平台图](https://cdn.masastack.com/stack/doc/auth/use-guide/third-party/third-party-remove.png)
