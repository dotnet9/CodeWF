# 领域驱动设计 - 领域事件

## 概述

什么是[领域事件](https://learn.microsoft.com/zh-cn/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)

根据事件类型我们将领域事件分为本地事件（`DomainEvent`）、集成事件（`IntegrationDomainEvent`），而本地事件根据读写性质不同划分为 `DomainCommand` 、 `DomainQuery` ，例如:

```csharp
/// <summary>
/// 当订单被创建时使用的是订单状态为已提交的领域事件
/// </summary>
public record OrderStartedDomainEvent(Order Order,
    string UserId,
    string UserName,
    int CardTypeId,
    string CardNumber,
    string CardSecurityNumber,
    string CardHolderName,
    DateTime CardExpiration) : DomainEvent;
```

通过领域事件，将发布者与订阅者解耦，订阅者只需要关注自己关心的事件，不必担心因为新业务的变化导致领域层不断增加一些不属于业务逻辑的代码从而变得无法维护

> 领域事件总线可以在 **聚合根**、**领域服务**被发布