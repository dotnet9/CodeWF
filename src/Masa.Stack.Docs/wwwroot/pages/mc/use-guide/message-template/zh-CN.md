# 使用指南 - 消息模板

消息模板可用于预设消息内容和变量，规范消息格式，并实现营销内容的分配。目前，消息模板支持短信、邮箱、站内信和App等渠道。

## 列表

消息模板列表以表格形式展示，支持高级筛选、模糊搜索和分页等功能。

### 模糊搜索

你可以使用模板名称或模板 `ID` 进行模糊搜索。

![smsTemplates](https://cdn.masastack.com/stack/doc/mc/smsTemplates.png)

### 高级搜索

![smsTemplates-advanced-search](https://cdn.masastack.com/stack/doc/mc/smsTemplates-advanced-search.png)

## 新建消息模板

点击列表页的`新建`按钮可打开新建消息模板的表单窗口。

> 模板标题、内容、跳转 `URL` 都支持变量, 变量格式为： \{ \{ 变量名 \} \}。短信模板同步阿里云模板，无需自己定义变量

### 短信模板

![smsTemplate-add](https://cdn.masastack.com/stack/doc/mc/smsTemplate-add.png)

![smsTemplate-add2](https://cdn.masastack.com/stack/doc/mc/smsTemplate-add2.png)

| 属性 | 描述 |
| ---  | --- |
| **模板 `ID`** | 唯一标识，用于调用 `SDK` 发送消息时指定消息模板。 |
| **签名** | 阿里云短信签名。 |
| **同步模板** | 点击后同步所选渠道的阿里云短信模板，更新短信模板选择池和模板状态。 |
| **变量** |使用的模板变量名称。 |
| **映射变量** |阿里云短信模板使用的变量名称。 |
| **每日发送次数限制** |此模板每日给同一用户最多发送次数限制，填 `0` 即不限制。 |

### 邮箱模板

![emailTemplate-add](https://cdn.masastack.com/stack/doc/mc/emaiTemplate-add.png)

### 站内信模板

![websiteMessageTemplate-add](https://cdn.masastack.com/stack/doc/mc/websiteMessageTemplate-add.png)

### App模板

![appTemplate-add](https://cdn.masastack.com/stack/doc/mc/appTemplate-add.png)

![appTemplate-add2](https://cdn.masastack.com/stack/doc/mc/appTemplate-add2.png)

| 属性 | 描述 |
| ---  | --- |
| **应用内消息** | 勾选后会生成站内信 |
| **跳转地址** | 站内信的跳转url地址,同时会在透传消息Json中增加一条key为url的数据 |
| **附加字段** | 透传消息内容配置 |
| **可选配置-Android** | **点击通知打开指定页面**：使用 intent 里的 url 指定点击通知栏后跳转的目标页面 |
| **可选配置-iOS** | **是否生产环境**：配置推送环境 |

## 编辑消息模板

点击表格中指定行操作列中的`编辑图标`可打开编辑消息模板的表单窗口。

![smsTemplate-edit](https://cdn.masastack.com/stack/doc/mc/smsTemplate-edit.png)
