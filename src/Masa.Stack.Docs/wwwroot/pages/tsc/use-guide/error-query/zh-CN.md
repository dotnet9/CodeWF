# 错误查询

## 常见错误说明

### 团队

![团队错误图](https://cdn.masastack.com/stack/doc/tsc/use-guide/error-query/team-1.0.png)


1. 红色项目有应用报错，绿色为正常
2. 红色应用有错误日志，无背景色为正常
3. 警告目前无实际用处
4. 数字大于1时，表示有错误的应用数量和应用的错误总记录数

应用详情页

有红色通知栏时，表示应用请求链路有错误产生

![团队详情错误图](https://cdn.masastack.com/stack/doc/tsc/use-guide/error-query/team_detail-1.0.png)

## 指标说明

Apdex 服务满意度：

```
(sum(rate(http_server_duration_bucket{le="250"}[5m])) by (service_name)+sum(rate(http_server_duration_bucket{le="1000"}[5m])) by (service_name))/2
/sum(rate(http_server_duration_bucket{le="1000"}[5m])) by (service_name)
```

[参见](https://www.bookstack.cn/read/prometheus-manual/best_practices-histogram_and_summaries.md#Apdex%20score%20%E5%BA%94%E7%94%A8%E6%80%A7%E8%83%BD%E6%8C%87%E6%95%B0)

服务成功率：

```
sum by (service_name) (increase(http_server_duration_count{http_status_code!~"5.."}[1m]))*100/sum by (service_name) (increase(http_server_duration_count[1m]))
```

服务请求平均耗时：

```
sum by (service_name) (increase(http_server_duration_sum[1m]))/sum by (service_name) (increase(http_server_duration_count[1m]))
```

服务请求耗时百分比(P99):

```
histogram_quantile(0.99, sum(rate(http_request_duration_seconds_bucket[5m])) by (le))
```

[参见](https://www.bookstack.cn/read/prometheus-manual/best_practices-histogram_and_summaries.md#Quatiles%E5%88%86%E4%BD%8D%E6%95%B0)

慢接口查询：

```
sort_desc(round( sum by(http_target) (increase(http_response_sum[1m]))/sum by(http_target) (increase(http_response_count[1m])),1))
```