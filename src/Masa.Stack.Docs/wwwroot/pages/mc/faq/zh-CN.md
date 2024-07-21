# 常见问题

## Q：MASA.MC 是什么？

A：[MASA.MC 产品介绍](stack/mc/introduce)

## Q：短信模板审核状态如何更新?

A：点击同步模板按钮，会同步阿里云短信模板状态

## Q：邮箱、站内信模板如何配置变量？

A：模板标题、内容、跳转 `URL` 都支持变量, 变量格式为： \{ \{ 变量名 \} \}。

## Q：发送消息中手动选择的收件人数据哪里添加？

A：`MASA.MC` 中用户对接的 `MASA.Auth`，如果需要给内部用户发送消息，需要接入 `MASA.Auth`。

## Q：如何定时发送消息及分批次发送？

A：请参考[使用指南-发送消息](stack/mc/use-guide/send-message)。同时发送规则依赖 `MASA.Scheduler`, 若要支持发送规则需要接入 `MASA.Scheduler`。

## Q：调用 SDK 发送站内信广播消息，并没有生成用户的站内信数据？

A：广播模式下通过 `SignalR` 发送检查通知，客户端接收后需要主动调用 `SDK` 的检查方法才会生成当前用户的站内信数据。请参考 [SDK 示例](stack/mc/sdk-instance)。

## Q：在非生产环境，想要测试短信发送又不想真的发送短信？

A：可以在 `MASA DCC`配置`MC`的Mock开关。

1. 在 `DCC`中找到 `MC`项目的Service应用

   ![dcc-mc-service](https://cdn.masastack.com/stack/doc/mc/dcc-mc-service.png)

2. 找到Mock配置，将Enable改为 `true`并发布

   ![dcc-mc-mock](https://cdn.masastack.com/stack/doc/mc/dcc-mc-mock.png)

3. 重启MC服务

我们会持续收集更多的 FAQ。
