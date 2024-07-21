# 项目监测

## 团队

团队是以项目团队的维度来查看团队所拥有的项目的运行状态，使用该功能时，必须集成Masa PM

![团队图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/team/team-1.0.png)

### 搜索选择

- 最上方左边为项目类型，可以作为过滤条件
- 搜索框可以搜索项目名称或应用名称
- 最右边时间搜索指定时间范围内，有相关数据的应用和项目
- 右边卡片点击，可根据项目状态快速筛选项目，从上往下，依次为 全部，警告，异常和正常

### 功能

- 一个六边形为一个项目，每个项目包含多个应用，最多显示三个应用，多于3个时，最下方会显示 ...
- 项目状态用颜色来区分，绿色代表正常，黄色代表警告，红色代表异常
- 应用状态红色背景代表异常，黄色背景代表警告，无背景色代表正常
- 应用显示排序为，先根据应用状态异常，警告，正常，再根据应用名称
- 鼠标移至项目上方，会显示项目名称，项目Id和项目包含的应用的数量；移至应用上方，会显示应用名称和应用Id
- 右边卡片对应信息，可参见提示信息

### 其它功能入口

- 点击六边形内的应用，可跳转至应用详情

### 应用详情

该页面展示项目详情、所属团队详情，以及应用相关监控概览信息

![团队详情图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/team/team_detail-1.0.png)

#### 服务选择

- 服务下拉框可以切换应用，查看不同应用的监控概览
- 时间搜索切换时间范围内，查看相关时间的应用监控数据

#### 功能

- 左侧上方展示项目信息，分别为：项目名称，项目内应用数量，项目id，项目描述和项目创建人
- 左侧下方为团队详情信息，展示信息分别为：团队图标，团队名称，团队包含的项目数量，包含的应用总数量，团队管理员和团队描述
- 右侧上方红色通知栏出现时，代表应用出现了应用请求链路错误，点击右边的查看详情可跳转至错误链路列表
- 服务成功率：该段时间内服务请求状态码不为5XX的总数/总的请求数量
- 服务调用次数：服务负载，可根据折线图变化快速发现服务是否异常
- 服务平均响应时间：当前时间范围内，该服务所有接口请求的平均耗时，改值越大，说明接口可能比较慢
- 服务满意度：Apdex，根据折线变化，快速判断服务响应是否存在问题
- 服务响应时长：P99，P95，P90，P75，P50，快速判断是整个应用出现问题，还是少部分接口存在问题
- 接口调用次数：接口调用次数top 10,可以发现接口是否存在恶意访问
- 慢接口：top 10访问最慢的接口，排查接口是否存在问题，点击明细，可跳转至链路详情

#### 其它功能入口

- 右上角蓝色`查看详情`按钮点击跳转至应用仪表盘详情

![仪表盘详情图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/team/instrument-1.0.png)

- 红色通知栏右边的查看详情链接，可跳转至错误的链路列表

![日志错误列表图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/team/trace-1.0.png)

- 慢接口列表可跳转至对应的链路详情

![链路详情图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/team/trace_detail-1.0.png)

### 应用仪表盘

该仪表盘为内置的一个仪表盘，展示应用相关的监测信息

#### 概览

![仪表盘详情图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/team/instrument-1.0.png)

- **Service Avg Response Time**：服务平均响应时间，反馈服务整体的响应时间变化
- **Service Load Call**：服务负载，每分钟调用次数
- **Service Apdex**：服务满意度
- **Success Rate**：服务成功率
- **Service Response Time Percentile**：P99，P95，P90，P75，P50
- **Instance Load In Current Service**：实例负载，适合负载均衡查看各实例的负载状态
- **Service Instance Load**：服务实例负载折线图
- **Slow Endpoint In Current Service**：慢接口 top 10

#### 实例

- 服务实例的概览数据，点击服务实例，可以进入到实例仪表盘

![仪表盘实例图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/team/instrument_instance-1.0.png)

#### 接口

- 服务接口的概览数据，点击接口地址，可以进入到接口仪表盘

![仪表盘接口图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/team/instrument_endpoint-1.0.png)

#### 拓扑图

- 拓扑图在没有指定服务的时候，默认以 Masa Auth Service 为中心
- 可以指定中心服务直接依赖层级，例如中心服务为A，A->B，B->C，C->D，A->D，当层级为1，得到A->B，A->D；当层级为2时，得到A->B，B->C，A->D；当层级为3时A->B，B->C，C->D，A->D

![仪表盘拓扑图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/team/instrument_topology-1.0.png)

#### 错误

- 应用请求异常信息按照内容分组后出现的次数，可点击对应的行，跳转到日志列表。

![错误](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/team/instrument_error-1.0.png)

## 日志

日志为系统记录的详细日志信息

![日志信息图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/log/log-1.0.png)

### 搜索功能

- 输入框可根据关键词搜索，也可以输入Elasticsearch高级查询语法来执行高级查询，输入完成后，按回车键进行搜索
- 也可以搜索指定时间范围

### 功能

- 上方柱图表示日志记录数量的变化，根据变化也可以发现的明显的异常剧增或变少
- 列表为日志条目和详细信息，日志信息主要分为两类

1. 有TraceId关联
   通常为请求过程中产生，通过TraceId和链路信息关联，通过SpanId和链路中具体某个请求过程关联，更加便于和Trace相配合快速排查故障和问题

2. 无TraceId关联
   一般为非请求产生，例如服务启动，停止，定时任务等产生，只能根据日志相关更详细信息来排查故障

## 链路

链路是记录应用中每个请求的上下文相关的完整请求链路信息，在出现问题时，可以根据时间和接口来查找相关链路的请求信息，核对请求参数，处理过程和处理结果，还可以通过TraceId跟日志相结合，查看链路相关的日志详细信息

![链路图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/trace/trace-1.0.png)

### 搜索

- 左边可以搜索应用，实例和接口(搜索接口时，可以不选择实例)，可以作为过滤条件
- 搜索框可以搜索任意关键词或TraceId，或类似日志的高级查询一样，输入高级查询表达式进行查询
- 最右边时间搜索指定时间范围内，有相关数据的应用和项目

### 链路详情

链路是记录应用中每个请求的上下文相关的完整请求链路信息，在出现问题时，可以根据时间和接口来查找相关链路的请求信息，核对请求参数，处理过程和处理结果，还可以通过TraceId跟日志相结合，查看链路相关的日志详细信息

![链路详情图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/trace/trace_detail-1.0.png)

#### 功能

- 上方线条为各链路的请求时间占比和开始结束位置，第一条蓝色线为当前链路的根，第二开始为子链路信息
- 左侧树形表格为链路的层级关系，点击对应的信息，右侧卡片会切换展示对应链路信息
- 右侧卡片信息上方显示 server端链路的服务名称和调用接口地址
- 左侧树形数据选择的类型为 Server 时，右侧会有 上一条 和下一条 的快速切换Icon
- 属性为链路信息Attributes信息，例如Http，Database的相关信息
- 资源为集成可观测性时，程序启动时设定的参数，程序启动后不可更改
- 详情为除了属性和资源之外的链路信息
- 日志为当前子链路相关的日志列表

## 时间选择

### 时间选择器

![时间选择器图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/time/view-1.0.png)

1. 选择的时间范围，点击后弹窗内可以手动设置时间自定义范围

   设置时间范围时，结束时间必须大于开始时间，范围最多为30天

   ![时间选择器打开图](https://cdn.masastack.com/stack/doc/tsc/use-guide/monitoring/time/open-1.0.png)

 2. 时区信息，一般为默认为用户所在时区，如果需要设置，点击时间范围，如上图，选择对应的时区后，确定即可生效，当前系统的时区就会全部改为该时区

 3. 快速时间范围选择，距离当前时间的一段时间范围，在做一些概览查询时，会比较方便，范围最大支持最近30天，最小最近5分钟

 4. 手动刷新功能，点击手，时间范围会按照`3`设置的最近时间范围更新时间，同时触发时间更新查询动作

 5. 自动刷新功能，主要用在仪表盘监测时，按照固定时间间隔刷新报表