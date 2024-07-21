# 领域驱动设计 - 概述

什么是 [领域驱动设计](https://blogs.masastack.com/2022/02/11/masa/framework/design/2.MASA%20Framework%20-%20DDD%E8%AE%BE%E8%AE%A1%EF%BC%881%EF%BC%89/)

MASA Framework 框架提供了基础设施使得基于领域驱动设计的开发更容易实现，通过 `DDD` 使得大家更聚焦于核心业务，基于 `DDD` 理论，我们可以设计出高质量的软件模型，它围绕业务概念构建领域模型来控制业务的复杂度，**解决软件难以理解和演化的问题**。

 `DDD` 中常见的技术概念和模式：

* [领域模型](/framework/building-blocks/ddd/domain-model)
* [仓储](/framework/building-blocks/ddd/repository)
* [领域服务](/framework/building-blocks/ddd/domain-service)

## 优势

使用 `DDD` 有什么优势？

* 设计清晰，规范
* 利于传递与传承
* 帮助团队建立良好的沟通
* 协助系统架构严谨
* 提高团队设计能力

## 如何落地

<div style="display: flex; justify-content: center;">
  <div style="width:50%" >
    <img src="https://cdn.masastack.com/framework/building-blocks/ddd/strategic-design.png"/>
  </div>
  <div style="width:50%" >
    <img src="https://cdn.masastack.com/framework/building-blocks/ddd/tactical-design.png"/>
  </div>
</div>

* 事件风暴：一种灵活的 **研讨会** 形式，用于 **协作探索复杂的业务领域**。

  ![事件风暴](https://cdn.masastack.com/framework/building-blocks/ddd/event-storm.png)

## 总结

`DDD` 并不是银弹，它并非适用于所有场景，以下情况可能不适合使用 `DDD`：

* 简单项目：引入 `DDD` 会增加过多的复杂性
* 限制时间和预算的项目：使用 `DDD` 会花费大量的时间和精力来分析业务需求，模型设计
* 人员能力不足：开发人员具备较强的业务知识和设计能力
* 技术局限性：某些技术环境可能难以支持 `DDD` 所需的复杂性

## 相关链接

* [.NET 现代化应用的产品设计 - DDD 实践](https://www.bilibili.com/video/BV1qV4y1x7d6)