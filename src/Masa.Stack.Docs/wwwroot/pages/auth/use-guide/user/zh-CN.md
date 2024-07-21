# 用户介绍

Auth用户是MASA Stack的一种实体身份，代表需要访问MASA Stack应用的人员，用户包含多类用户，包括终端用户、第三方用户（QQ/微信/Github/域用户等）、员工（内部员工/外部员工）。
为了让用户能够访问MASA Stack应用，需要先创建Auth用户并授权。

> 用户的权限来自于角色、团队、自身的扩展权限。可执行权限：页面权限、元素权限、API权限

![用户管理结构图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-functional.svg)

## 用户

1. 使用Auth用户登录**Auth控制台**。
2. 在左侧导航栏，点击**用户**。

用户列表使用分页以表格形式呈现，可模糊搜索和高级搜索。

![用户管理页图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/users.png)

### 搜索用户

1. 模糊搜索

> 模糊搜索支持昵称、手机号、邮箱、账号

![用户搜索列表图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-search.png)

2. 高级搜索

> 创建时间范围、用户状态搜索

![用户高级搜索列表图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-advanced-search-icon.png)

### 新建用户

点击`新建`，进入新建用户页，新建用户分三步。

1. 选择性别、选择/上传头像，点击右下角的`下一步`，进入用户信息页。

   > 可选择系统提供的默认头像，上传自定义头像推荐尺寸 300*300（px），图片大小不超过 2M。

   ![新建用户步骤一图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-add-step1.png)

2. 在用户信息页，顶部中间区域填写昵称，**基本信息**区域填写真实姓名、手机号码、邮箱、身份证号、用户帐号、用户密码，点击**填写更多**，**详细信息**区域填写公司名称、地址、部门、职位，点击右下角的`下一步`，进入用户授权页。

   > 唯一不可重复说明用户和字段一对一绑定，如一个用户只能绑定一个手机号码，一个手机号码只能对应一个用户。
   * **昵称**：必填，可包含中文、英文字母、数字，最多50个字符。
   * **真实姓名**：可空，可包含中文、英文字母、数字，2到50个字符。
   * **手机号码**：必填，唯一不可重复。
   * **邮箱**：可空，唯一不可重复。
   * **身份证号**：可空，唯一不可重复。
   * **用户帐号**：可空，唯一不可重复，可包含中文、英文字母、数字及特殊字符_@.，8到50个字符，如账号未填写，创建用户后会以手机号做为账号。
   * **用户密码**：必填，6到50个字符。
   * **公司名称**：可空，可包含中文、英文字母、数字，2到50个字符。
   * **地址**：可空。
   * **部门**：可空，可包含中文、英文字母、数字，2到16个字符。
   * **职位**：可空，可包含中文、英文字母、数字，2到16个字符。

   ![新建用户步骤二图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-add-step2.png)
   ![新建用户步骤二详细信息图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-add-step2-2.png)

3. 在授权页，通过绑定角色对用户分配权限，选择角色后可在中间区域预览用户的权限。

    > 用户不可直接关联权限，若要针对某用户拒绝某权限可通过拆分角色或添加拒绝该权限的角色（拒绝 > 授权）

    ![新建用户步骤三图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-add-step3.png)

### 编辑用户

点击用户列表中指定行所在的操作列中的`编辑图标`可打开编辑Auth用户的表单窗口。

在表单中可更换头像、修改基本信息、详细信息。
在表单中可查看此用户的授权认证、历史记录的简要信息。

> **账号**为不可编辑项。  
> 表单校验规则仍遵循**新建Auth用户**表单的校验规则。  
> 可点击表单左下角**启用/禁用**用户。

![编辑用户图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-edit.png)

#### 重置密码

在表单中点击`重置密码`，可重置用户的密码。

> 重置密码需要二次确认。  
> 重置密码是随机生成的密码，管理员不可自定义密码，管理员在重置密码后可点击复制图标复制新密码给相关用户。

![重置密码图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-reset-password.png)

#### 删除用户

点击表单下方右侧的`删除`可删除用户。

![删除用户图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-delete.png)

### 用户授权

点击表格中指定行所在的操作列中的`授权图标`可打开Auth用户授权的表单窗口。

> 权限配置功能与新建用户时权限配置一至。

当此用户为员工时，表单中多出选择用户所在的项目团队，用于切换不同的团队来查看此用户在当前团队时的权限。

![编辑用户授权图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/user-authorize.png)

## 第三方用户

第三方用户来自于第三方平台（QQ/微信/Github/域用户等）登录过来的用户，由于第三方用户在第一次登录/创建时会与对应的Auth用户做绑定,所以Auth用户始终包含第三方用户。

> 与Auth用户做绑定时，如若不存在对应的Auth用户，系统则会新建与之对应的Auth用户。

点击**第三方用户**标签，切换到第三方用户

### 搜索第三方用户

1. 模糊搜索

> 同用户的**模糊搜索**

2. 高级搜索

> 创建时间范围、用户状态搜索、用户来源搜索

![第三方用户搜索列表图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/third-party-user-search.png)

### 域账号配置

点击列表页右上方的`域账号配置`，打开域账号同步窗口，域账号同步功能可将域用户同步到Auth员工中，以手机号作为同步绑定标识。

![域账号配置图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/third-party-user-ldap.png)

## 员工

员工来自于管理员创建/批量导入。由于员工在第一次创建时会与对应的Auth用户做绑定,所以Auth用户始终包含员工。

> 与Auth用户做绑定时，如若不存在对应的Auth用户，系统则会新建与之对应的Auth用户。

### 搜索员工

点击**员工**标签，切换到员工

> 同用户的**模糊搜索**

![员工搜索列表图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/staff-search.png)

### 新建员工

点击列表页右上方的`新建`可打开新建员工的的表单窗口，新建员工分两步。

1. 选择性别、选择/上传头像，点击右下角的`下一步`，进入用户信息页。
   > 可选择系统提供的默认头像，上传自定义头像推荐尺寸 300*300（px），图片大小不超过 2M。

   ![新建员工步骤一图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/staff-add-step1.png)

2. 在用户信息页，顶部中间区域填写昵称，**基本信息**区域填写真实姓名、手机号码、邮箱、员工类型、项目团队、身份证号、地址、部门、工号、职位、用户密码，点击右下角的`下一步`，进入用户授权页。
   > 唯一不可重复说明用户和字段一对一绑定，如一个用户只能绑定一个手机号码，一个手机号码只能对应一个用户。
   * **昵称**：必填，可包含中文、英文字母、数字，最多50个字符。
   * **真实姓名**：可空，可包含中文、英文字母、数字，2到50个字符。
   * **手机号码**：必填，唯一不可重复。
   * **邮箱**：可空，唯一不可重复。
   * **员工类型**：必选，内部员工/外部员工。
   * **项目团队**：可选，选择员工所属的项目团队。
   * **身份证号**：可空，唯一不可重复。
   * **地址**：可空。
   * **部门**：可空，可包含中文、英文字母、数字，2到16个字符。
   * **工号**：必填，可包含英文字母、数字，4到12个字符。
   * **职位**：可空，可包含中文、英文字母、数字，2到16个字符。
   * **用户密码**：必填，6到50个字符。

   ![新建员工步骤二图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/staff-add-step2.png)

### 编辑员工

点击表格中指定行所在的操作列中的`编辑图标`可打开编辑员工的表单窗口。

在表单中可更换头像、修改基本信息。
在表单中可查看此用户历史记录的简要信息。

> 表单校验规则仍遵循**新建员工**表单的校验规则。  
> 可点击表单左下角**启用/禁用**用户。

![编辑员工图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/staff-edit.png)

#### 删除用户

点击表单下方右侧的`删除`可删除用户。

![删除用户图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/staff-delete.png)

### 员工授权

点击表格中指定用户所在行的操作列中的`授权图标`可打开员工授权的表单窗口，可`预览`配置授权的权限。

> 鼠标移到应用或权限上，点击右侧的选择框选择权限，可多选（全选/反选）所属的全部权限。

![员工授权图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/staff-authorize.png)

### 员工同步

点击列表页右上方的`同步`按钮可打开同步员工的的窗口，员工同步支持utf8格式的csv文件，可将csv文件里的员工数据同步至员工列表。

> 下载`员工同步模板`，根据模板填写员工相关信息，上传文件同步。

![员工同步图](https://cdn.masastack.com/stack/doc/auth/use-guide/user/staff-sync.png)
