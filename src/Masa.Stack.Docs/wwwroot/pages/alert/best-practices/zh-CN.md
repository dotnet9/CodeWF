# 最佳实践

## 监控服务日志告警

1. 将需要监控的服务接入可观测性，可参考[开发指南-可观测性接入](stack/alert/develop-guide)。
2. 在消息中心（MC）中创建好渠道和消息模板，消息中心（MC）负责管理告警的通知渠道和通知内容，可参考[消息中心](stack/mc/introduce)。
3. 在告警中心（Alert）中创建告警规则，配置好监控项、告警规则和通知策略，可参考[使用指南-告警规则](stack/alert/use-guide/alarm-rule)。
   ![告警规则-示例-监控项](https://cdn.masastack.com/stack/doc/alert/alarmRule-example-monitoring.png)
   ![告警规则-示例-设置1](https://cdn.masastack.com/stack/doc/alert/alarmRule-example-setting1.png)
   ![告警规则-示例-设置2](https://cdn.masastack.com/stack/doc/alert/alarmRule-example-setting2.png)
4. 规则创建后，告警中心（Alert）会在 任务调度中心（Scheduler）中注册 Job。告警规则会定期检查评估监控项，根据告警规则分析结果，触发告警或恢复通知，然后通过消息中心（MC）进行告警消息的发送。
