# SSO登录

单点登录（Single Sign On），简称为 SSO，是比较流行的企业业务整合的解决方案之一。SSO 的定义是在多个应用系统中，用户只需要登录一次就可以访问所有相互信任的应用系统。
单点登录（SSO）就是通过用户的一次性鉴别登录。即，当用户在身份认证服务器上登录一次以后，即可获得访问单点登录系统中其他关联系统和应用软件的权限，同时这种实现是不需要管理员对用户的登录状态或其他信息进行修改的，
这意味着在多个应用系统中，用户只需一次登录就可以访问所有相互信任的应用系统。这种方式减少了由登录产生的时间消耗，辅助了用户管理，是比较流行的。

> 特点：一次登录，全部访问。  
> 从整个系统层面来看SSO，它的核心就是3个元素：1. 用户，2. 系统，3. 验证中心。

![非SSO和SSO比较图](https://cdn.masastack.com/stack/doc/auth/sso/nosso-sso.svg)

使用Auth用户登录**Auth控制台**。

## 用户声明

用户申明用于描述用户的属性，比如年龄、性别、户籍所在地等。单个用户申明对应单个用户属性。

在左侧导航栏，点击**单点登录**->**用户声明**。

用户声明以表格形式展现，有分页、模糊搜索功能。

### 搜索用户声明

> 模糊搜索用户声明名称

![用户声明列表图](https://cdn.masastack.com/stack/doc/auth/sso/userclaim/userclaim-search.png)

### 新建用户声明

点击列表页右上角的`新建`可打开新建用户声明的表单窗口。

* **名称**：必填，唯一不可重复，2到50个字符。
* **描述**：必填，2到255个字符。

![新建用户声明图](https://cdn.masastack.com/stack/doc/auth/sso/userclaim/userclaim-add.png)

### 快速新建标准声明

点击列表页右上角的`快速新建标准声明`，系统会自动创建[标准的用户申明](https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims)。

> 快速创建标准声明执行一次即可。

![快速新建标准声明图](https://cdn.masastack.com/stack/doc/auth/sso/userclaim/userclaim-add-quick.png)

### 编辑用户声明

点击表格中指定行对应的操作列中的`编辑图标`可打开编辑用户声明的表单窗口。

* **名称**：不可编辑。
* **描述**：必填，2到255个字符。

![编辑用户声明图](https://cdn.masastack.com/stack/doc/auth/sso/userclaim/userclaim-edit.png)

### 删除用户声明

点击表格中指定行对应的操作列中的`删除图标`，点击`确定`即可删除。

![删除用户声明图](https://cdn.masastack.com/stack/doc/auth/sso/userclaim/userclaim-delete.png)

## 身份资源

身份资源是[用户声明](/stack/auth/instructions/ssologin#用户声明)的集合。比如创建"身份证"身份资源，身份证包含姓名、出生日期、户籍所在地、头像等用户声明。单个份资源对应多个用户声明。

在左侧导航栏，点击**单点登录**->**身份资源**。

身份资源以表格形式展现，有分页、模糊搜索功能。

### 搜索身份资源

> 模糊搜索身份资源名称，显示名称

![身份资源列表图](https://cdn.masastack.com/stack/doc/auth/sso/identityresource/identityresource-search.png)

### 新建身份资源

点击列表页右上角的`新建`可打开新建身份资源的表单窗口。

* **显示名称**：必填，2到50个字符。
* **名称**：必填，唯一不可重复，2到50个字符。
* **描述**：可空，最多255个字符。

> 是否必须：勾选代表[Client](/stack/auth/guides/sso/client)必须Scope设置包含改身份资源  
> 强调：todo  
> 在文档中展示：勾选代表会在发现文档中展示该身份资源

![新建身份资源图](https://cdn.masastack.com/stack/doc/auth/sso/identityresource/identityresource-add.png)

### 快速新建标准身份资源

点击列表页右上角的`快速新建标准身份资源`，系统会自动创建[标准的身份资源](https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims)。

> 快速创建标准身份资源执行一次即可。

![快速新建标准声明图](https://cdn.masastack.com/stack/doc/auth/sso/identityresource/identityresource-add-quick.png)

### 编辑身份资源

点击表格中指定行对应的操作列中的`编辑图标`可打开编辑身份资源的表单窗口。

* **显示名称**：必填，2到50个字符。
* **名称**：不可编辑。
* **描述**：必填，2到255个字符。

![编辑身份资源图](https://cdn.masastack.com/stack/doc/auth/sso/identityresource/identityresource-edit.png)

### 删除身份资源

点击表格中指定行对应的操作列中的`删除图标`，点击`确定`即可删除。

![删除身份资源图](https://cdn.masastack.com/stack/doc/auth/sso/identityresource/identityresource-delete.png)

## API范围

API范围用于描述API,比如发送邮件API。单个API范围对应单个API。

```csharp
var apiScope = new ApiScope
{
	Name = "SendEmail",
	DisplayName = "发送邮件"
};
```

在左侧导航栏，点击**单点登录**->**API范围**。

以表格形式展现，有分页、模糊搜索功能。

### 搜索API范围

> 模糊搜索Api范围名称，显示名称

![API范围列表图](https://cdn.masastack.com/stack/doc/auth/sso/apiscope/apiscope-search.png)

### 新建API范围

点击列表页右上角的`新建`可打开新建API范围的表单窗口。

* **显示名称**：必填，2到50个字符。
* **名称**：必填，唯一不可重复，2到50个字符。
* **描述**：可空，最多255个字符。

> 是否必须：勾选代表[Client](/stack/auth/guides/sso/client)必须Scope设置包含改身份资源  
> 强调：todo  
> 在文档中展示：勾选代表会在发现文档中展示该身份资源  
> 用户声明可多选

![新建API范围图](https://cdn.masastack.com/stack/doc/auth/sso/apiscope/apiscope-add.png)

### 编辑API范围

点击表格中指定行对应的操作列中的`编辑图标`可打开编辑API范围的表单窗口。

* **显示名称**：必填，2到50个字符。
* **名称**：不可编辑。
* **描述**：必填，2到255个字符。

> 是否必须：勾选代表[Client](/stack/auth/guides/sso/client)必须Scope设置包含改身份资源  
> 强调：todo  
> 在文档中展示：勾选代表会在发现文档中展示该身份资源  
> 用户声明可多选

![编辑API范围图](https://cdn.masastack.com/stack/doc/auth/sso/apiscope/apiscope-edit.png)

### 删除API范围

点击表格中指定行对应的操作列中的`删除图标`，点击`确定`即可删除。

![删除Api范围图](https://cdn.masastack.com/stack/doc/auth/sso/apiscope/apiscope-delete.png)

## API资源

API资源是API范围的集合，一个API资源对应多个API范围。

```csharp
var apiResource = new ApiResource
{
	Name = "EmailModule",
	DisplayName = "邮件模块",
	Scopes = new List<string>
	{
		"SendEmail",
		"RemoveEmail",
		"ReadEmail"
	}
};
```

在左侧导航栏，点击**单点登录**->**API资源**。

以表格形式展现，有分页、模糊搜索功能。

### 搜索API资源

> 模糊搜索API资源名称，显示名称

![API资源列表图](https://cdn.masastack.com/stack/doc/auth/sso/apiresource/apiresource-search.png)

### 新建API资源

点击列表页右上角的`新建`可打开新建API资源的表单窗口。

* **显示名称**：必填，2到50个字符。
* **名称**：必填，唯一不可重复，2到50个字符。
* **描述**：可空，最多255个字符。

> 在文档中展示：勾选代表会在发现文档中展示该API资源  
> 用户声明和Api范围可多选

![新建API资源图](https://cdn.masastack.com/stack/doc/auth/sso/apiresource/apiresource-add.png)

### 编辑API资源

点击表格中指定行对应的操作列中的`编辑图标`可打开编辑API资源的表单窗口。

* **显示名称**：必填，2到50个字符。
* **名称**：不可编辑。
* **描述**：必填，2到255个字符。

> 在文档中展示：勾选代表会在发现文档中展示该API资源  
> 用户声明和API范围可多选

![编辑API资源图](https://cdn.masastack.com/stack/doc/auth/sso/apiresource/apiresource-edit.png)

### 删除API资源

点击表格中指定行对应的操作列中的`删除图标`，点击`确定`即可删除。

![删除API资源图](https://cdn.masastack.com/stack/doc/auth/sso/apiresource/apiresource-delete.png)

## 客户端

客户端功能为[OAuth 2.0授权框架](https://www.rfc-editor.org/rfc/rfc6749#section-1.1)中定义的客户端角色提供支持。代表服务器创建受保护资源请求的应用程序资源所有者及其授权。

在左侧导航栏，点击**单点登录**->**客户端**。

以表格形式展现，有分页、模糊搜索功能。

### 搜索客户端

> 模糊搜索客户端名称，客户端ID

![客户端列表图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-search.png)

### 新建客户端

点击列表页右上角的`新建`可打开新建客户端的表单窗口，包含基础信息和资源信息。

> 五种客户端类型：Web、Spa、Native、Machine、Device，默认选择Web。

系统会根据客户端类型设置默认GrantTypes。分别为：

* **Web**： `authorization_code` + Pkce
* **Spa**： `authorization_code` + Pkce
* **Native**：`authorization_code` + Pkce
* **Machine**：`client_credentials`
* **Device**：`device_code`

> 暂不支持自定义，应针对`IExtensionGrantValidator`支持自定义Grant Type  
> 注：五种客户端类型信息一致，编辑时可编辑不同客户端的信息。

点击新建客户端表单顶部中间的**基础信息**/**资源信息**标签，可切换

* **客户端Logo**：提供默认Logo，可自定义上传。
* **客户端类型**：必选，默认Web。
* **客户端ID**：必填，客户端唯一标识，对接Oidc时指定的ClientId值。
* **客户端名称**：必填，最多50个字符，授权页等页面展示的客户端名称。
* **客户端Url**：可空，Url。
* **需要授权确认**：可选。
* **允许离线请求**：可选。
* **重定向地址**：可空，Url，可添加多个。
* **注销跳转地址**：可空，Url，可添加多个。
* **描述**：可空，最多200个字符。

![新建客户端基础信息图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-add.png)

* **资源信息**：身份资源和Api资源可多选。

![新建客户端资源信息图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-add-resource.png)

### 编辑客户端

点击表格中指定行对应的操作列中的`编辑图标`可打开编辑客户端的表单窗口。

> 共有信息：基础信息、授权确认页、认证、资源信息。
> 点击顶部中间的Tab标签切换不同信息编辑。

#### 共有信息

##### 基础信息

* **客户端ID**：不可修改，客户端唯一标识，对接Oidc时指定的ClientId值。
* **客户端名称**：必填，最多50个字符，授权页等页面展示的客户端名称。
* **描述**：可空，最多200个字符。
* **启用**：可选，不选中即禁用。
* **授权请求对象**：可选，是否授权请求对象。
* **允许CORS的源**：可空，跨域Url，可添加多个。
* **属性集**：可空，键值对，可添加多个。

![编辑客户端基础信息图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-edit-basic.png)

##### 授权确认页

* **客户端Logo**：可自定义上传修改。
* **客户端Url**：可空，Url。
* **是否需要授权确认**：可选。
* **是否允许记住授权**：可选。
* **授权生命周期**：可空，数字（秒）。

![编辑客户端授权确认页图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-edit-authorization.png)

##### 认证

* **重定向地址**：可空，Url，可添加多个。
* **注销跳转地址**：可空，Url，可添加多个。
* **其它字段**：根据提示页面信息操作。

![编辑客户端认证图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-edit-authentication.png)

##### 资源信息

* **资源信息**：身份资源和API资源可选。

![编辑客户端资源信息图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-edit-resource.png)

#### 客户端Web/Spa/Native

> 类型/值成对出现，点击右边的**+**时必填，可新建多个。  
> 其它根据页面提示信息操作。

![编辑客户端Token图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-edit-token.png)

#### 客户端Machine

* **值**：必填。
* **到期时间**：可选到期日期。
* **描述**：可空。

> 输入值、选择到期时间、输入描述，点击`新建`，新建一个密钥，可新建多个密钥。

![编辑客户端Machine图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-edit-machine.png)

![编辑客户端Machine有密钥图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-edit-machine2.png)

#### 客户端Device

![编辑客户端Device图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-edit-device.png)

#### 删除客户端

点击**编辑客户端表单页**左下角的`删除`，弹出确认删除框，点击`确定`即或删除。

![删除客户端图](https://cdn.masastack.com/stack/doc/auth/sso/client/client-delete.png)

## 自定义登录

在左侧导航栏，点击**单点登录**->**自定义登录**。

以表格形式展现，有分页、模糊搜索功能。

### 搜索自定义登录

> 模糊搜索名称、标题

![自定义登录搜索列表图](https://cdn.masastack.com/stack/doc/auth/sso/customlogin/customlogin-search.png)

### 新建自定义登录

点击列表页右上角的`新建`可打开新建自定义登录的表单窗口，包含基础信息和登录和注册。

基础信息

* **名称**：必填，可包含中文、英文字母、数字，2到50个字符。
* **标题**：必填，可包含中文、英文字母、数字，2到50个字符。
* **客户端**：必选，选择客户端。

![新建自定义登录基础信息图](https://cdn.masastack.com/stack/doc/auth/sso/customlogin/customlogin-add-basic.png)

登录

> 右侧：点击底部的`新建`，添加OAuth第三方登录，可选择Github、Wechat等，显示在左侧的底部，表格可对添加的OAuth第三方登录进行排序、删除。  
> 左侧：点击`登录`，只是验证表单。

![新建自定义登录登录图](https://cdn.masastack.com/stack/doc/auth/sso/customlogin/customlogin-add-login.png)

![新建自定义登录登录添加第三方登录图](https://cdn.masastack.com/stack/doc/auth/sso/customlogin/customlogin-add-login2.png)

注册

> 右侧：点击底部的`新建`，添加注册表单项，可选择表单项，显示在左侧，表格可对添加的表单项进行是否必填、排序、删除。  
> 左侧：点击`注册`，只是验证表单。

![新建自定义登录注册图](https://cdn.masastack.com/stack/doc/auth/sso/customlogin/customlogin-add-reg.png)

![新建自定义登录注册添加表单项图](https://cdn.masastack.com/stack/doc/auth/sso/customlogin/customlogin-add-reg2.png)

### 编辑自定义登录

点击表格中指定行对应的操作列中的`编辑图标`可打开编辑自定义登录的表单窗口。

基础信息

* **名称**：必填，可包含中文、英文字母、数字，2到50个字符。
* **标题**：必填，可包含中文、英文字母、数字，2到50个字符。
* **客户端**：不可修改。

![编辑自定义登录基础图](https://cdn.masastack.com/stack/doc/auth/sso/customlogin/customlogin-edit-basic.png)

登录

> 右侧：点击底部的`新建`，添加OAuth第三方登录，可选择Github、Wechat等，显示在左侧的底部，表格可对添加的OAuth第三方登录进行排序、删除。  
> 左侧：点击`登录`，只是验证表单。

![编辑自定义登录登录图](https://cdn.masastack.com/stack/doc/auth/sso/customlogin/customlogin-edit-login.png)

注册

> 右侧：点击底部的`新建`，添加注册表单项，可选择表单项，显示在左侧，表格可对添加的表单项进行是否必填、排序、删除。  
> 左侧：点击`注册`，只是验证表单。

![编辑自定义登录注册图](https://cdn.masastack.com/stack/doc/auth/sso/customlogin/customlogin-edit-reg.png)

### 删除自定义登录

点击表格中指定行对应的操作列中的`删除图标`，点击`确定`即可删除。

![删除自定义登录图](https://cdn.masastack.com/stack/doc/auth/sso/customlogin/customlogin-delete.png)
