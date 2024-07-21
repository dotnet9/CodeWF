# 产品介绍

### MASA Scheduler 是什么？

`Scheduler` 是 `MASA Stack 1.0` 推出的一款辅助性软件产品，主要负责处理应用程序任务执行的调度，以及自动重试等相关操作。在 `MASA Stack` 产品中，与 `MASA MC`、`MASA TSC`、`MASA Alert` 3 款产品结合，发挥最大的调度价值。当然 `Scheduler` 并不只是给 `MASA Stack` 产品使用，它同样可以为业务创造价值。

![introduce](http://cdn.masastack.com/stack/doc/scheduler/introduce.png)

### 特性与优势

1. 可以对接 `MASA Stack` 的相关项目，也可以考虑外接其他项目；

2. 目前已支持 `Job`、`Http`、`Dapr Service Invocation` 三种类型的任务调度；

3. 支持配置各种运行策略与失败策略（并可设置手动、自动）。

对于执行策略有多维度可以进行配置，特别对于执行优先级、执行失败后的相关策略都可进行设置。

### 基础结构

![Infrastructure](http://cdn.masastack.com/stack/doc/scheduler/Infrastructurei.png)

### Job运行流程图

![job-run-flow](http://cdn.masastack.com/stack/doc/scheduler/job-run-flow.png)

### 互相发现模型

![discovery-model](http://cdn.masastack.com/stack/doc/scheduler/discovery-model.png)
