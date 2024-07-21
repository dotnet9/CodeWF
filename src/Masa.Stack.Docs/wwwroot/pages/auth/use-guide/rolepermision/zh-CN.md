# 角色权限

MASA Auth中的角色设计是一个扁平的权限带名词，角色本身没有任何权限。只做去重的处理，可以挂靠各种权限生成带权限的角色。
角色之间采用RBAC3的模型设计，可有角色权限的继承关系和使用次数。
权限做为项目中的功能使用合集，在调整与设置的时候又细分了前端、API等。前端权限除了可以生成菜单外，也可以挂靠API权限，还可以挂靠在角色、团队以及个人用户名下。

使用Auth用户登录**Auth控制台**。

## 角色

在左侧导航栏，点击**角色权限**->**角色**。

角色模块主要包含角色的增删改以及维护角色拥有的权限。

> MASA Auth目前基于角色的访问控制（RBAC），后续支持基于属性的访问控制（ABAC）

### 搜索角色

角色列表以表格形式展现，有分页、角色名称模糊搜索功能。

> 根据角色名称模糊搜索

![角色搜索列表图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/role-search.png)

### 新建角色

点击角色列表右上角的`新建`按钮，弹出新建角色的表单页。

1. 在新建角色的表单页中输入角色信息，点击`下一步`，进入绑定角色授权页。

   * **角色名称**：必填，可包含中文、英文字母、数字，2到50个字符。
   * **Code**: 必填，角色唯一标识，2到150个字符。
   * **限制绑定次数**：大于等于0的数字，该角色可以被几个用户使用
   * **描述**：最多50个字符。

   ![新建角色步骤一角色信息图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/role-add.png)

2. 角色授权

   可选择继承哪些角色的权限，底部权限树可以进一步补充角色权限和启用/禁用角色，可`预览`该角色授权的权限。

   ![新建角色步骤二授权图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/role-add-2.png)

### 编辑角色

点击表格中指定行所在的操作列中的`编辑图标`，打开编辑角色的表单窗口。

在表单中查看上级角色、角色拥有者及简单的历史操作记录，这些信息不可编辑。
可启用、禁用角色。

![编辑角色基础信息图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/role-edit.png)

点击`权限`标签编辑角色的权限。

可编辑继承哪些角色的权限，底部权限树可以进一步补充角色权限和启用/禁用角色，可`预览`该角色授权的权限。

![编辑角色授权图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/role-edit-2.png)

### 删除角色

点击表格中指定行所在的操作列中的`删除图标`，弹出删除角色的确认框，点击`确定`按钮删除角色。

![删除角色图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/role-delete.png)

## 权限

在左侧导航栏，点击**角色权限**->**权限**。

MASA Auth 权限分为两大类**前端权限**和**API权限**，其中前端权限分为菜单权限、元素权限和数据权限(暂不支持)。

* **菜单权限**：即系统菜单，配置用户拥有哪些菜单
* **元素权限**：页面内权限细分控制，如哪些人有删除权限，元素权限需要配合Masa.Blazor 提供的`IPermissionValidator`使用
* **数据权限**：用户数据控制、如业务员只能看到自己的业务数据(暂不支持)
* **API权限**：API接口访问权限配置

![权限管理功能结构图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/permision.svg)

### 新建权限

选择项目，点击**前端权限**或**API**标签，鼠标移到对应的节点上，点击`新建图标`

> 根节点为选择项目的应用

![新建权限入口图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/permision-add.png)

#### 新建前端权限

前端权限维护菜单权限和元素权限。

> 应用的第一子级只能为菜单权限，且菜单权限和元素权限不能有相同的父级

输入菜单名称及其它内容项点击提交即可。

* **Key**：必填，权限名称，可包含中文、英文字母、数字，最多20个字符。
* **Code**：必填，权限唯一标识（保证唯一性，默认拼接系统应用AppId）。
* **类型**：必选，菜单权限、元素权限和数据权限。
* **排序**：数字，菜单排序及权限树内排序字段
* **Url**：可空，权限地址，菜单及元素页面路由
* **描述**：可空，最多255个字符
* **Icon**：可空，图标代码，目前仅菜单权限有效，做菜单展示的图标。内容格式为Masa Blazor [Icon](https://docs.masastack.com/blazor/components/icons)支持的图标
* **匹配规则**：可空，正则字符串，仅菜单权限有效，作用于左侧菜单中ListItem的MatchPattern属性，[参考](https://docs.masastack.com/blazor/features/auto-match-nav)

![新建前端权限图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/permision-add-frontend.png)

#### 新建API权限

* **Key**：必填，权限名称，可包含中文、英文字母、数字，最多20个字符。
* **Code**：必填，权限唯一标识（保证唯一性，默认拼接系统应用AppId）。
* **类型**：必选，菜单权限、元素权限和数据权限。
* **排序**：数字，菜单排序及权限树内排序字段
* **Url**：可空，权限地址
* **描述**：可空，最多255个字符

![新建API权限图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/permision-add-api.png)

> 点击新建权限表单的`显示名称`，输入中英文显示名称

![新建权限显示名称图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/permision-add-displayname.png)

### 编辑权限

点击要编辑的权限节点，编辑权限信息，删除权限。

#### 编辑前端权限

权限类型、挂靠角色、权限使用者只可查看，编辑、启用、禁用、删除前端权限。

> 挂靠API权限：可选择关联的API权限。

![编辑前端权限图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/permision-edit-frontend.png)

#### 编辑API权限

在表单中编辑、删除API权限。

![编辑API权限图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/permision-edit-api.png)

> 点击编辑权限页的`显示名称`，输入中英文显示名称

![编辑权限显示名称图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/permision-edit-displayname.png)

#### 删除权限

点击编辑页面的`删除`按钮，弹出确认删除权限窗口，点击`确定`删除权限。

![删除权限图](https://cdn.masastack.com/stack/doc/auth/use-guide/permision/permision-delete.png)
