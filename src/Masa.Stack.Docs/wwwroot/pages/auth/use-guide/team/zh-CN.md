# 团队介绍

团队管理包含添加团队、编辑团队、删除团队等操作。

1. 使用Auth用户登录**Auth控制台**。
2. 在左侧导航栏，点击**项目团队**。

## 团队搜索

团队列表已卡片的形式展现，目前没有做分页处理。卡片包含的团队信息分别为：团队头像、成员人数、团队名称、团队管理员以及编辑时间。
右上角搜索框可根据团队名称模糊搜索团队，回车触发搜索动作。

![项目团队图](https://cdn.masastack.com/stack/doc/auth/use-guide/team/teams.png)

## 新建团队

点击卡片列表右上方的`新建`按钮，弹出新建团队窗口。

新建团队为Step方式分别为团队基础信息、团队管理员、团队成员。

> 只有员工才能加入团队

### 1. 基础信息

​   输入团队名称，激活`下一步`按钮，默认会根据团队名称首个字符以及选择的颜色生成团队头像。

   * **团队名称**：必填

   * **头像文本**：可空，头像显示的文字，默认获取团队名称的第一个字符

   * **头像颜色**：必选，a头像背景色

   * **类型**：必选，团队类型

   * **描述**：可空，团队说明

   ![新建团队步骤一基础信息图](https://cdn.masastack.com/stack/doc/auth/use-guide/team/team-add-basic.png)

### 2. 团队管理员

   自顶向下依次为选择团队管理员、设置团队管理员角色、预览团队管理员权限

   > 角色列表中列出所有的可用角色，由于角色可以限制绑定人数，管理员人数大于角色可绑定人数时会禁用该角色。

   底部为应用权限树，应用数据从PM中获取。

   ![新建团队步骤二设置管理员图](https://cdn.masastack.com/stack/doc/auth/use-guide/team/team-add-admin.png)

### 3. 团队成员

   同`团队管理员`

   > 选择团队成员时，已自动过滤设置为该团队管理员的员工

   ![新建团队步骤三设置团队成员图](https://cdn.masastack.com/stack/doc/auth/use-guide/team/team-add-member.png)

## 编辑团队

点击要编辑的团队卡片右上角的`编辑图标`，弹出编辑团队窗口，编辑团队信息、删除团队

![编辑团队入口图](https://cdn.masastack.com/stack/doc/auth/use-guide/team/team-edit-icon.png)

### 基础信息

![编辑团队基础信息图](https://cdn.masastack.com/stack/doc/auth/use-guide/team/team-edit-basic.png)

### 团队管理员

点击设置管理员

![编辑团队设置管理员图](https://cdn.masastack.com/stack/doc/auth/use-guide/team/team-edit-admin.png)

### 团队成员

点击设置团队成员

![编辑团队设置团队成员图](https://cdn.masastack.com/stack/doc/auth/use-guide/team/team-edit-member.png)

### 删除成员

点击编辑团队窗口左下角的`删除`按钮，弹出删除团队确认信息框，点击`确定`删除团队。

![删除团队图](https://cdn.masastack.com/stack/doc/auth/use-guide/team/team-delete.png)
