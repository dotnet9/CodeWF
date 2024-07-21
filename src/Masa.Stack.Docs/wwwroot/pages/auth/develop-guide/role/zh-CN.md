# 开发指南-角色

角色实现了RBAC3权限模型，拥有角色继承、基数约束特性。

## 角色继承

角色继承功能使角色配置灵活、方便，更易于管理，一个角色可以继承任意个角色然后获得所有继承角色的权限总和，继承具有递归性，A继承B，B继承C，A拥有B和C的权限总和。

## 基数约束

基数约束用于限制角色被用户绑定次数，用户直接绑定角色或通过团队间接绑定角色都纳入基数限制。下面通过财务角色来举例。

1.在角色页面中创建财务角色，限制绑定次数配置为2

![](https://cdn.masastack.com/stack/doc/auth/role-add-caiwu.png)

2.在用户页面点击指定用户对应的授权图标进行授权，将财务角色绑定给用户，如此操作两个用户授权后，再操作第三个用户时会显示由于绑定次数限制，无法选择此角色

![](https://cdn.masastack.com/stack/doc/auth/user-authorize-role-limit.png)

3.在团队页面中点击新增团队

先将财务角色绑定给团队管理员，再操作添加团队管理员时会提示由于角色:[财务]约束限制,当前最多只能选择0人

![](https://cdn.masastack.com/stack/doc/auth/team-add-role-limit-01.png)

先添加团队管理员，再操作添将财务角色绑定给团队管理员时会显示显示由于绑定次数限制，无法选择此角色

![](https://cdn.masastack.com/stack/doc/auth/team-add-role-limit-02.png)

4.此时我们在角色页面中编辑财务角色，将限制绑定次数设置为1，会提示绑定限制次数不能小于2

![](https://cdn.masastack.com/stack/doc/auth/role-edit-limit.png)