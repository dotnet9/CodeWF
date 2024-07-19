using CodeWF.Models;
using Microsoft.AspNetCore.Components;

namespace CodeWF.Components.Pages;

public partial class Stack
{
    private static readonly List<string> sWhyContent1 =
    [
        ".NET应用交付“保姆级”护航",
        "性能指标强悍，可担当电商、物联网等大流量场景的坚实底座",
        "支持任意板块可替换",
        "富含软件工程实践、项目管理方法论"
    ];

    private static readonly List<string> sWhyContent2 =
    [
        "全职开源团队，快速响应",
        "Apache-2.0 协议，可放心商用",
        "微软代码规范，欢迎共同维护"
    ];

    private static readonly List<string> sWhyContent3 =
    [
        "多位.NET领域大咖推荐",
        "共同引领微软技术生态",
        "开放的社区",
        "定期社区例会，线上线下Meetup互动"
    ];

    private static readonly List<MenuableTitleItem> sMenuableTitleItems =
    [
        new MenuableTitleItem("Basic Ability", "现代应用治理解决方案", "#basic-ability-content"),
        new MenuableTitleItem("Operator", "高效运维解决方案", "#operator-content"),
        new MenuableTitleItem("Data Factory", "数据治理解决方案", "#data-factory-content"),
        new MenuableTitleItem("Why MASA Stack", "为什么选择MASA Stack?", "#why-masa-stack-content")
    ];

    private static readonly List<StackFeature> basicAbilityFeatures =
    [
        new("Auth", "权限中心", "内置RBAC3标准，集成诸多平台用户认证与访问管理，支持OIDC标准扩展接入", "https://cdn.masastack.com/images/Auth.svg", 172),
        new("Scheduler", "调度中心", "统一调度，支持多种模式触发智能任务调度，智能worker弹性伸缩", "https://cdn.masastack.com/images/Scheduler.svg", 172, true),
        new("WorkFlow", "工作流", "依据CNCF Serverless Workflow规范，集成诸多开源服务或中间件，支撑数据处理事件编排、API聚合等场景支持Dapr标准API",
            "https://cdn.masastack.com/images/WorkFlow.svg", 196),
        new("Micro FE", "微前端", "Blazor微前端解决方案，为Blazor提供工程化最佳实践", "https://cdn.masastack.com/images/MicroFE.svg", 196, true),
        new("Insights", "洞察", "基于ML.NET，内嵌诸多成熟模型，应用于AIOps、RPA、预测性维护等场景", "https://cdn.masastack.com/images/Insights.svg", 172),
        new("Security", "安全", "一站式安全解决方案，包含访问控制、数据安全、审计等适用于混合云、物联网等领域", "https://cdn.masastack.com/images/Security.svg", 172, true),
        new("YARP", "网关", "基于YARP定制，可支撑大流量高性能场景，内置API全生命周期管理，适用于K8S南北流量治理，新老架构升级等场景", "https://cdn.masastack.com/images/YARP.svg", 196),
        new("Function", "函数计数", "基于OpenFunction，Dapr，KEDA，打造.NET领域函数计算解决方案", "https://cdn.masastack.com/images/Functions.svg", 196, true),
        new("MC", "消息中心", "支持站内信、短信、邮件、模板消息，统一消息管理，统一消息调度", "https://cdn.masastack.com/images/MC.svg", 172),
        new("API Management", "API管理", "对流程编排、数据服务等场景暴露的API的管理，流量治理的相关配置能力", "https://cdn.masastack.com/images/ApiManagement.svg", 172, true),
    ];

    private static readonly List<StackFeature> operatorFeatures =
    [
        new("PM","项目管理", "提供项目管理一站式解决方案，是应用持续交付的基础支撑", "https://cdn.masastack.com/images/PM.svg", 196),
        new("DCC","配置中心", "支持多级配置缓存，支持服务配置热更新，及标准的配置变更管理流程，并支持Dapr标准API", "https://cdn.masastack.com/images/DCC.svg", 196, true),
        new("TSC","故障排查控制台", "基于openTelementry标准，兼容开源可视化监控组件，如ES，Prometheus，Grafana接入", "https://cdn.masastack.com/images/TSC.svg", 172),
        new("Alert","告警中心", "内置灵活的告警规则引擎，无缝集成MC与TSC，开放Webhook对接", "https://cdn.masastack.com/images/Alerts.svg", 172, true),
        new("DevOps","开发运维", "开发运维一体化，代码库、CI/CD和任务管理", "https://cdn.masastack.com/images/DevOps.svg", 172),
    ];

    private static readonly List<StackFeature> dataFactoryFeatures =
    [
        new("Metadata","元数据", "覆盖元数据采集、加工、管理全流程，以支撑标签系统、ETL、低代码建模等场景", "https://cdn.masastack.com/images/Metadata.svg", 196),
        new("ETL","数据管道", "基于MASA Workflow打造分布式数据处理引擎，支持各类数据库的输入输出，适用于数据迁移、数据入仓\\湖、流批一体等解决方案", "https://cdn.masastack.com/images/ETL.svg", 196, true),
        new("Big Data API","数据服务", "将数据分析能力、标签对象以API的形式对外提供服务支持GraphQL和OData查询规范", "https://cdn.masastack.com/images/BigDataApi.svg", 172),
        new("View","数据可视化", "基于开源echarts搭建的，低代码的可视化工具，适用于即席分析、大屏展示、BI等场景", "https://cdn.masastack.com/images/View.svg", 172, true),
    ];

    public record StackFeature(string Title, string SubTitle, string Content, string Image, int Height, bool RightImage = false);
}
