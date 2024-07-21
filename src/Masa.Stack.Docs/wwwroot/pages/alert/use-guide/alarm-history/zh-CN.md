# 使用指南 - 告警历史

根据告警规则产生的告警数据

## 列表

告警历史列表表格形式展现，有三个筛选tab（正在告警、已处理、无需通知），支持高级筛选、模糊搜索、分页等功能。

![告警历史](https://cdn.masastack.com/stack/doc/alert/alarmHistorys.png)

- 高级筛选：含时间段（区分时间类型）、严重程度、状态
- 模糊搜索：根据规则名称进行模糊搜索

## 处理告警

1. 告警详情

   告警详情及告警的监控项历史记录图表。
   ![处理告警-详情](https://cdn.masastack.com/stack/doc/alert/handleAlarm-details.png)

   告警规则的触发规则，未触发的规则置灰显示。
   ![处理告警-详情1](https://cdn.masastack.com/stack/doc/alert/handleAlarm-details2.png)

2. 处理告警
   - 已处理，标记告警已处理并指定处理完成后的通知，处理人为当前操作人。
   - 第三方处理，选择网络钩子和处理人，使用选择的网络钩子通知第三方，让第三方业务方自己处理告警。
   ![处理告警](https://cdn.masastack.com/stack/doc/alert/handleAlarm.png)

## 查看告警

告警详情及处理详情查看
![告警-详情](https://cdn.masastack.com/stack/doc/alert/alarm-detail.png)
![告警-详情2](https://cdn.masastack.com/stack/doc/alert/alarm-detail2.png)
