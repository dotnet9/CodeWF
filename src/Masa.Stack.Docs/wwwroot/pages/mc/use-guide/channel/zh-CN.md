# 使用指南 - 渠道管理

渠道管理用于配置不同的消息推送渠道，包括短信、邮箱、站内信、`APP` 等类型。其中短信渠道使用阿里云，`APP` 推送支持个推和极光。

## 渠道列表

渠道列表以卡片形式展示，支持模糊搜索和分页功能。

搜索功能支持渠道名称和渠道 ID 的模糊搜索。

![channels](https://cdn.masastack.com/stack/doc/mc/channels.png)

## 新建渠道

点击卡片列表右上角的`新建`按钮，弹出新建渠道的表单窗口。表单包括两个步骤。

1. 选择渠道类型。

   ![channel-add-type](https://cdn.masastack.com/stack/doc/mc/channel-add-type.png)

2. 配置渠道信息。

   - 渠道名称：渠道的显示名称。
   
   - 渠道 `ID`：用于调用 `SDK` 发送消息时指定渠道的唯一标识。

不同的渠道类型有不同的配置参数。

   * 短信渠道配置
   
      目前短信发送使用阿里云，需要填写阿里云的 `AccessKeyId` 和 `AccessKeySecret` 参数。创建短信渠道后，短信模板会自动同步到本地短信模板池中，创建短信模板时可以直接选择。
   
      ![channel-add-sms](https://cdn.masastack.com/stack/doc/mc/channel-add-sms.png)
   
   * 邮箱渠道配置
   
      ![channel-add-email](https://cdn.masastack.com/stack/doc/mc/channel-add-email.png)
   
   * 站内信渠道配置
   
      ![channel-add-websiteMessage](https://cdn.masastack.com/stack/doc/mc/channel-add-websiteMessage.png)
   
   * `APP` 渠道配置
   
      支持个推和极光。

      ![channel-add-app](https://cdn.masastack.com/stack/doc/mc/channel-add-app-provider.png)

      个推
   
      ![channel-add-app](https://cdn.masastack.com/stack/doc/mc/channel-add-app-getui.png)

      极光
   
      ![channel-add-app](https://cdn.masastack.com/stack/doc/mc/channel-add-app-jpush.png)
   
## 编辑和删除渠道

点击要编辑的渠道卡片右上角的编辑图标，弹出编辑渠道对话框，可以编辑渠道信息或删除渠道。

![channel-edit](https://cdn.masastack.com/stack/doc/mc/channel-edit.png)
