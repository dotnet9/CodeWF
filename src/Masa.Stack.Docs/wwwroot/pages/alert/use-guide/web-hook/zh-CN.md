# 使用指南 - 网络钩子（WebHook）

网络钩子配置和管理，暂时用于告警处理中通知第三方。
第三方完成后续操作后需要通知告警中心告警已处理，目前提供了处理完成接口，后续会提供 SDK。

## 列表

网络钩子列表以卡片形式展现，支持模糊搜索、分页等功能。

![网络钩子](https://cdn.masastack.com/stack/doc/alert/webHooks.png)

- 根据网络钩子的名称进行模糊搜索

## 创建/编辑

密钥用与网络钩子通信中校验请求是否有效所用。
![网络钩子-编辑](https://cdn.masastack.com/stack/doc/alert/webHook-edit.png)

## 测试

指定处理人，立即向配置的网络钩子地址发送推送。
![网络钩子-测试](https://cdn.masastack.com/stack/doc/alert/webHook-test.png)