# 使用指南

## 配置对象管理

该文档以应用配置为例，业务配置和公共配置没有差异，只是入口不同。
配置中心的应用关联项目管理请参考[项目管理](stack/pm/get-started)。

### 创建配置对象

1. 点击全景页面项目下的应用进入配置详情页

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/config.png)

2. 点击新增

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/config_insert.png)

3. 输入配置对象信息

   - 名称
   - 格式（JSON、XML、YAML、Properties、RAW。该文档以 JSON 格式为大家演示）
   - 是否加密（如果选择加密则该配置的内容会被加密存储，且只有管理员能看的到内容，获取配置时也需要解密）
   - 选择集群（该配置对象会被添加到哪些环境集群中）

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/config_input.png)

### 修改配置内容

1. 点击配置对象的编辑图标

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/config_edit.png)

2. 把编辑好的内容填写进去并进行保存

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/config_edit_input.png)

### 发布配置

只有发布的配置才会被客户端获取到

1. 点击发布图标

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/release_config.png)

2. 填写发布信息并保存

   1. 发布名称

   2. 描述（非必填）

     ![](https://cdn.masastack.com/stack/doc/dcc/rc1/release_config_input.png)

### 回滚配置

#### 通过发布历史回滚

1. 点击发布历史

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/config_rollback.png)

2. 点击版本号回滚至指定版本

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/config_rollback_edit.png)

#### 直接点击回滚
   
1. 点击回滚

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/config_rollback_2.png)

2. 点击版本号回滚至指定版本（可以对比回滚前后的内容）

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/config_rollback_edit_2.png)

### 创建标签配置
    
所有的客户端都可以获取到

1. 点击发布图标

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/label.png)

2. 输入配置对象信息

   ![](https://cdn.masastack.com/stack/doc/dcc/rc1/label_insert.png)


### 配置项目实战
    
请参考[SDK示例](stack/dcc/sdk-instance)。