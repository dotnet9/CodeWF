# 组织架构

组织架构模块目前在整个系统中相对独立，只起到维护组织数据的作用，包括新增组织子级、编辑节点、复制节点、删除节点，组织节点成员管理等。

> 根节点不可复制删除

![组织架构图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-view.png)

## 组织架构操作菜单

鼠标移到组织节点，点击节点右侧的`操作图标`，弹出操作菜单，包括复制、编辑、新建子部门。

![组织架构图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-operate.png)

## 新建组织

点击操作菜单中的`新建子部门`按钮，输入组织名称和描述（可选）即可。

* **组织名称**：必填。
* **位置**：必选，可以更改组织的上级组织。
* **描述**：可空。

![新建组织节点图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-add.png)

## 复制组织

点击操作菜单中的`复制`按钮，修改组织名称和根据需要勾选迁移原部门人员。

![复制组织节点步骤一图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-copy.png)

点击`下一步`按钮，在新的窗口中调整节点位置和进一步筛选迁移哪些部门成员。

![复制组织节点步骤二组织成员操作图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-copy-staff.png)

## 编辑组织

点击对应组织菜单的`编辑`按钮，新弹出的窗口中，修改名称、部门位置、描述，点击提交即可。

![修改组织节点图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-edit.png)

## 删除组织

在要删除的组织菜单中选择`编辑`，点击弹窗底部`删除`按钮即可。

> 删除节点会连带将该节点的所有子节点一起删除,且当节点下有员工时不可删除

![删除组织节点图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-delete.png)

## 组织成员

组织节点成员以表格形式分页显示，

### 模糊搜索

在组织节点成员表格左上角搜索框输入 ，按下回车Enter键

![组织节点成员图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-staff.png)

### 新建成员

> 表单规则详见[用户->新建员工](/stack/auth/instructions/usermanage#新建员工)

点击组织节点成员表格右上角的`新建`按钮，弹出新建成员输入页

![新建组织节点成员性别头像图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-staff-add.png)

选择性别，选择/上传头像，点击`下一步`按钮，进入成员信息输入页

![新建组织节点成员信息图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-staff-add-info.png)

### 编辑成员

点击组织节点成员表格所在行右侧的`编辑图标`，弹出编辑成员页，可在编辑成员页启用、禁用、删除成员

![编辑组织节点成员信息图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-staff-edit.png)

#### 删除成员

点击编辑组织节点成员信息页的`删除`按钮

![删除组织节点成员图](https://cdn.masastack.com/stack/doc/auth/use-guide/organization/org-staff-delete.png)
