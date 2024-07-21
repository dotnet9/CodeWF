# 使用指南

## 初始化

在使用 `PM` 时，系统会默认创建三个环境（开发环境Development、预发布/模拟环境Staging、生产环境Production），一个集群（Default），并初始化 `MASA.Stack` 的一些项目和应用。

![全景图](https://cdn.masastack.com/stack/doc/pm/use-guide/overview.png)

## 新建环境

点击左侧**环境列表**底部的`新建环境`按钮，弹出新建环境窗口的表单。

- **名称**：必填，唯一不可重复，可包含中文、英文字母、数字、_-符号，2到50个字符。
- **颜色**：必选，系统提供5种颜色，默认选择第一个。
- **关联集群**：可不选，可选多个关联集群。
- **描述**：可空，最多255个字符。

![新建环境图](https://cdn.masastack.com/stack/doc/pm/use-guide/env-add.png)

## 编辑环境

点击左侧**环境列表**中的**当前选中*环境右边的 <i class="mdi mdi-pencil"></i>，弹出编辑环境窗口的表单。

> 表单元素见**新建环境**的验证规则。  
> 可查看操作历史的简要信息和关联的集群数。  
> 可删除要编辑的环境。

![编辑环境图](https://cdn.masastack.com/stack/doc/pm/use-guide/env-edit.png)

### 删除环境

点击编辑环境的表单右下角的 <i class="mdi mdi-delete" style="color:#FF5252;"></i>，弹出删除环境的确认框，点击`确定`即可删除。

> 当前环境有关联的集群，不能删除。

![删除环境图](https://cdn.masastack.com/stack/doc/pm/use-guide/env-delete.png)

## 新建集群

点击右上角的`新建集群`按钮，弹出新建集群窗口的表单。

- **名称**：必填，唯一不可重复，可包含中文、英文字母、数字、_-符号，2到50个字符。
- **关联环境**：至少选择一个环境，可选多个关联环境。
- **描述**：可空，最多255个字符。

![新建集群图](https://cdn.masastack.com/stack/doc/pm/use-guide/cluster-add.png)

## 编辑集群

点击右侧顶部**集群标签列表**中的**当前选中*集群右边的 <i class="mdi mdi-pencil"></i>，弹出编辑集群窗口的表单。

> 表单元素见**新建环境**的验证规则。  
> 可查看操作历史的简要信息和关联的项目数。  
> 可删除要编辑的集群。

![编辑集群图](https://cdn.masastack.com/stack/doc/pm/use-guide/cluster-edit.png)

### 删除集群

点击编辑集群的表单右下角的 <i class="mdi mdi-delete" style="color:#FF5252;"></i>，弹出删除集群的确认框，点击`确定`即可删除。

> 当前集群有项目，不能删除。

![删除集群图](https://cdn.masastack.com/stack/doc/pm/use-guide/cluster-delete.png)

## 新建项目

点击右边底部的`新建项目`按钮，弹出新建项目窗口的表单。

- **名称**：必填，唯一不可重复，2到50个字符。
- **团队**：可选，所属项目团队（为项目分配团队，只有该团队的成员才能看到）。
- **ID**：标识，唯一不可重复，可包含英文字母、数字、_-符号，2到50个字符。
- **类型**：必选。
- **环境/集群**：至少选择一个环境/集群，可选多个环境/集群。
- **描述**：可空，最多255个字符。

![新建项目图](https://cdn.masastack.com/stack/doc/pm/use-guide/project-add.png)

## 编辑项目

点击右侧**集群标签列表**下方要编辑的项目右边的 <i class="mdi mdi-pencil"></i>，弹出编辑项目窗口的表单。

> **ID**不可修改，表单元素见**新建项目**的验证规则。  
> 可查看操作历史的简要信息。  
> 可删除要编辑的项目。

![编辑项目图](https://cdn.masastack.com/stack/doc/pm/use-guide/project-edit.png)

### 删除项目

点击编辑项目的表单右下角的 <i class="mdi mdi-delete" style="color:#FF5252;"></i>，弹出删除项目的确认框，点击`确定`即可删除。

> 当前项目有应用，不能删除。

![删除项目图](https://cdn.masastack.com/stack/doc/pm/use-guide/project-delete.png)

## 新建应用

点击项目，展开项目详情，点击`新建应用`按钮，弹出新建应用窗口的表单。

- **名称**：必填，唯一不可重复，2到50个字符。
- **类型**：必选，`UI`、`Service`、`Job`，默认选择`UI`。
- **ID**：标识，唯一不可重复，可包含英文字母、数字、_-符号，2到50个字符。
- **环境/集群**：至少选择一个环境/集群，可选多个环境/集群。
- **Url**：可空，一个环境/集群对应一个Url。
- **描述**：可空，最多255个字符。

![新建应用图](https://cdn.masastack.com/stack/doc/pm/use-guide/app-add.png)

## 编辑应用

点击要编辑的`应用`卡片，弹出编辑项目窗口的表单。

> **ID**不可修改，表单元素见**新建项目**的验证规则。  
> 可查看操作历史的简要信息。  

![编辑应用图](https://cdn.masastack.com/stack/doc/pm/use-guide/app-edit.png)

### 删除应用

点击编辑应用的表单右下角的 <i class="mdi mdi-delete" style="color:#FF5252;"></i>，弹出删除应用的确认框，点击`确定`即可删除。

![删除应用图](https://cdn.masastack.com/stack/doc/pm/use-guide/app-delete.png)
