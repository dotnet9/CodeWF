# 使用指南 - 告警规则

告警规则支持日志监控和指标监控，告警规则会按检查频率定期查询监控项，通过触发规则触发告警，并按策略发送告警或恢复通知。

## 列表

告警规则列表以卡片形式展现，支持高级筛选、模糊搜索、分页等功能。  
![告警规则](https://cdn.masastack.com/stack/doc/alert/alarmRules.png)

卡片操作条
![告警规则-操作](https://cdn.masastack.com/stack/doc/alert/alarmRules-action.png)

### 日志监控

- 高级筛选：含时间段（区分时间类型）、项目、应用
- 模糊搜索：根据规则名称进行模糊搜索

### 指标监控

- 高级筛选：含时间段（区分时间类型）、指标
- 模糊搜索：根据规则名称进行模糊搜索

## 告警规则创建/编辑

1. 关联资源

   ![告警规则-关联资源](https://cdn.masastack.com/stack/doc/alert/alarmRule-add-res.png)

2. 监控设置
   - 检查频率支持固定间隔和 cron 表达式。
   - 偏移量，填写了偏移周期就会按偏移周期查询数据；不填则查询当前周期数据。
   - 变量，即监控项名，可在触发规则表达式中使用。
     - 日志监控
         - 日志筛选，日志数据存储在 ES，填写 ES 查询语法即可。
            ![告警规则-日志监控](https://cdn.masastack.com/stack/doc/alert/alarmRule-add-logMonitor.png)
         - 生成的查询表达式预览
            ![告警规则-日志监控2](https://cdn.masastack.com/stack/doc/alert/alarmRule-add-logMonitor2.png)

3. 指标监控

   - 指标监控项变量支持配置和表达式填写
      ![告警规则-指标监控](https://cdn.masastack.com/stack/doc/alert/alarmRule-add-MetricMonitor.png)

4. 图表预览
   - 根据检查频率和监控项配置模拟监控项查询记录的图表
   - 日志采样预览，获取上一周期符合规则的最近一条日志数据
      ![告警规则-图表预览](https://cdn.masastack.com/stack/doc/alert/alarmRule-add-chart.png)

5. 告警设置
   - 连续触发阈值：配置连续触发阈值。当累计的触发次数达到该值时，产生一条告警。不满足触发条件时不计入统计。
   - 沉默周期：支持按周期和按时间，告警发生后未恢复正常，间隔多久重复发送一次告警通知
   - 触发规则表达式：请参考 MASA Stack 文档站点规则引擎介绍：<https://docs.masastack.com/framework/building-blocks/rules-engine/overview>
      ![告警规则-告警设置](https://cdn.masastack.com/stack/doc/alert/alarmRule-add-ruleSetting.png)
      ![告警规则-告警设置2](https://cdn.masastack.com/stack/doc/alert/alarmRule-example-setting2.png)
