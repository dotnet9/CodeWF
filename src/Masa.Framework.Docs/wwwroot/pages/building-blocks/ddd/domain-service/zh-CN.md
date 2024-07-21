# 领域驱动设计 - 领域服务

## 概述

是领域模型的操作者，被用来处理业务逻辑，它是无状态的，状态由领域对象来保存，提供面向应用层的服务，完成封装领域知识，供应用层使用。

与应用服务不同的是，应用服务仅负责编排和转发，它将要实现的功能委托给一个或多个领域对象来实现，它本身只负责处理业务用例的执行顺序以及结果的拼装，在应用服务中不应该包含业务逻辑

## 使用

1. 安装 `Masa.Contrib.Ddd.Domain`

   ```shell 终端
   dotnet add package Masa.Contrib.Ddd.Domain
   ```

2. 注册 领域事件总线

   ```csharp l:1
   builder.Services.AddDomainEventBus(options =>
   {
       //todo: use other services
   });
   ```

3. 使用领域服务

   ```csharp l:1,22
   public class PaymentDomainService : DomainService
   {
       private readonly ILogger<PaymentDomainService> _logger;
   
       public PaymentDomainService(ILogger<PaymentDomainService> logger)
       {
           _logger = logger;
       }
   
       public async Task StatusChangedAsync(Aggregate.Payment payment)
       {
           var orderPaymentDomainEvent = payment.Succeeded ? 
               new OrderPaymentSucceededDomainEvent(payment.OrderId): 
               new OrderPaymentFailedDomainEvent(payment.OrderId);
   
           _logger.LogInformation(
               "----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", 
               orderPaymentDomainEvent.GetEventId(), 
               Program.AppName, 
               orderPaymentDomainEvent);
   
           await EventBus.PublishAsync(orderPaymentDomainEvent);
       }
   }
   ```

   > 继承 `DomainService` 的类会被自动注册，其生命周期为 `Scoped`，它可以在应用服务的构造函数中被注入使用

## 领域事件总线

领域事件总线不仅仅可以发布 [进程内事件](/framework/building-blocks/dispatcher/local-event)、也可发布 [集成事件](/framework/building-blocks/dispatcher/integration-event)，它提供了：

* EnqueueAsync：领域事件入队
* PublishQueueAsync：发布领域事件 （根据领域事件入队顺序依次发布）
* AnyQueueAsync：是否存在领域事件