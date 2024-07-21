---
title: 常见问题
date: 2023/01/10
---

# 常见问题

## 无法找到服务

![daprId不存在图](https://cdn.masastack.com/stack/doc/tsc/faq/daprid-not-exists.png)

出现该问题，一般是 service 的 daprId 配置错误，如果在本地开发环境，可以用命令行工具`cmd`执行：

```
dapr list
```

出现以下结果：

![daprid不存在图](https://cdn.masastack.com/stack/doc/tsc/faq/dapr-list.png)

如果在 `k8s`，到对应的 tsc-server 查看绑定的 daprid ,在 tsc-ui 的环境变量中配置 `DAPR_APPID = <your tsc-server dapr appid>` 即可。

## 数据无法上报到 OTEL

服务都正常启动，也没有任务异常，但是对应的 OTEL 服务那边没有接受到任务数据

1. 检查 OTEL 服务是否正常运行；
2. 检查配置的 otlpUrl 是否跟对应的 OTEL 服务一致；
3. 检查服务是否可以正常访问 OTEL 端口，默认 **4717**；
4. 检查 OTEL 配置信息是否正确，可参考 [OTEL Configuration](https://opentelemetry.io/docs/collector/configuration)
