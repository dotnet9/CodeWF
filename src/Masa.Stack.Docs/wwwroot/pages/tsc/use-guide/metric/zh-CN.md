# 指标

## 概述

当前系统的仪表盘和部分图配置的数据源来自[Prometheus](https://prometheus.io/docs)的指标数据，当前系统的集成的指标数据参见：

|  名称  |  类型  |  说明  |
|  ------  |  --------  |  ---------  |
|  http_client_duration |  histogram  |  http client 耗时直方图  |
|  http_server_duration  |  histogram  |  http server 耗时直方图  |


 -  `Histogram` 概念不清楚的，参考[Prometheus 直方图](https://cloud.tencent.com/developer/article/1495303)，我们系统中这两个指标均为累计直方图；
 -  `Prometheus` 查询表达式也需要有一定的了解，参考[官方文档](https://prometheus.io/docs/prometheus/latest/querying/basics/)。
  

## 查询示例

Apdex 服务满意度：

```
(sum(rate(http_server_duration_bucket{le="250"}[5m])) by (service_name)+sum(rate(http_server_duration_bucket{le="1000"}[5m])) by (service_name))/2
/sum(rate(http_server_duration_bucket{le="1000"}[5m])) by (service_name)
```

参考[Apdex score 应用性能指数](https://www.bookstack.cn/read/prometheus-manual/best_practices-histogram_and_summaries.md#Apdex%20score%20%E5%BA%94%E7%94%A8%E6%80%A7%E8%83%BD%E6%8C%87%E6%95%B0)


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

参考[Quatiles分位数](https://www.bookstack.cn/read/prometheus-manual/best_practices-histogram_and_summaries.md#Quatiles%E5%88%86%E4%BD%8D%E6%95%B0)


慢接口查询：

```
sort_desc(round( sum by(http_target) (increase(http_response_sum[1m]))/sum by(http_target) (increase(http_response_count[1m])),1))
```