# 故障分析

## 故障分析数据源

错误信息

![日志错误图](https://cdn.masastack.com/stack/doc/tsc/use-guide/trouble-shooting/log-1.0.png)

发现异常日志信息，具体的接口请求无法判断，根据关联的`TraceId`到链路查询后进入到链路详情

![链路详情错误图](https://cdn.masastack.com/stack/doc/tsc/use-guide/trouble-shooting/trace_detail-1.0.png)

发现调用`Auth`服务的一个接口报错了，在对应的链路详情也看到详细的请求参数和异常详细信息，可借用工具验证`Auth`服务的该接口是否正常。

## 故障分析操作流程

### 分析过程

分析过程分为两种，一种是主动，还没有发现到服务受影响之前，另一种是被动，在被具体相关的负责人告知某些业务已经收到了影响。

### 主动模式

- 接入了 Masa PM 的，在团队首页，可以看到服务的运行状况，查看对应项目的错误(Trace状态码为500，或自定义状态码)总数量，

![主动模式团队图](https://cdn.masastack.com/stack/doc/tsc/use-guide/trouble-shooting/team-1.0.png)

可以根据系统常规运行的情况，逐个进入到应用详情，查看有报错的应用的详情

![主动模式团队详情图](https://cdn.masastack.com/stack/doc/tsc/use-guide/trouble-shooting/team_detail-1.0.png)

 1. 服务的调用次数是否有突然剧烈增加或减少；
 2. 服务满意度是否存在突然剧烈增加或减少；
 3. 服务响应时长是否存在剧烈的增加或减少；
 4. 点击错误的查看详情，跳转到链路看错误的列表，再逐条分析受影响的业务和进行对应的处理。

- 进入到链路界面：

![主动模式链路图](https://cdn.masastack.com/stack/doc/tsc/use-guide/trouble-shooting/trace-1.0.png)

查看span数量是否正常，是否存在剧烈增加或减少的情况，以及对应的平均耗时时长的变化是否在可接受范围内，如果发现异常，缩短时间查找，再逐步排查到对应的服务；

### 被动模式

该模式一般为已经发现了对应的一些错误信息，可以根据这些错误信息来对应查找，该模式适合开发人员排查错误

- 已知业务具体模块，可以知道是具体时间和接口

到链路界面，选择相应的时间，选择服务和对应的接口地址，查询时间段内响应的接口请求信息，根据反馈结果查找具体对应的请求记录；

![被动模式链路图](https://cdn.masastack.com/stack/doc/tsc/use-guide/trouble-shooting/trace_search-1.0.png)

点击详情进入查看对应的请求上下文和请求参数信息

![被动模式链路详情图](https://cdn.masastack.com/stack/doc/tsc/use-guide/trouble-shooting/trace_detail-1.0.png)

如果还想查找对应的错误日志信息，复制`TraceId`，到日志界面，选择对应的时间，输入：

```
{ "term": { "Attribute.TraceId.Keyword": "TraceId" } }

```

按回车键查询，如果在请求过程中记录相关的日志信息，则会查询到对应的记录，再通过日志信息详细排查

- 已知异常关键信息，不知道具体接口，过程跟上述相反，先到日志输入关键词模糊搜索后，找到对应的错误日志，再根据找到的TraceId到链路查找相关的接口请求

## 故障分析示例

某次查看链路信息时发现：
