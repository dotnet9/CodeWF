# 领域驱动设计 - 领域模型

## 概述

在软件开发中，对特定业务领域进行抽象和建模的过程。它描述了一个系统中涉及的实体、属性、关系以及相互作用，以及它们之间的行为和规则。 通过构建领域模型，可以更好地理解业务需求，并将其转换为可执行的代码。

可通过可视化软件架构，反映软件架构师和开发人员如何思考和构建软件的抽象

![](https://cdn.masastack.com/framework/building-blocks/ddd/c4.png)

## 实体

实体是领域模型中的领域对象，它具有连续性，与值对象之间的主要区别:

- 比较: 实体是使用标识 `ID` 来比较指定; 值对象使用结构比较，结构属性的值如果相同则是同一个

- 可变性: 实体是可变的，值对象不可变

- 生命周期: 实体有生命周期，值对象没有生命周期

  > 实体可以随时间跟踪信息，而值对象更像是一个时间点的快照

实体需要继承 `Entity`，例如：

```csharp l:1
public class OperateLog : FullEntity<Guid, int>
{
    public Guid CatalogId { get; private set; }

    public CatalogItem Catalog { get; private set; } = null!;
    
    public string Content { get; set; }
}
```

> AuditAggregateRoot：审计实体，在实体的基础上 **增加** 了 **审计功能** （创建操作人、创建时间、修改操作人、修改时间）
>
> FullEntity：Full 实体，在审计实体基础上 **增加** 了 **软删除**

## 聚合根

是一组相关对象的集合，作为一个整体被外界访问，聚合根具有全局标识，在全局中它是唯一的

聚合根类需要继承 `AggregateRoot`，例如：

```csharp Domain/Catalog/Aggregates/CatalogItem.cs l:7,35
using Masa.BuildingBlocks.Data;
using Masa.BuildingBlocks.Ddd.Domain.Entities.Full;
using Masa.EShop.Service.Catalog.Domain.Events;

namespace Masa.EShop.Service.Catalog.Domain.Catalog.Aggregates;

public class CatalogItem : FullAggregateRoot<Guid, int>
{
    public string Name { get; private set; } = null!;

    public decimal Price { get; private set; }

    public string PictureFileName { get; private set; } = "";

    public int CatalogTypeId { get; private set; }

    public CatalogType CatalogType { get; private set; } = null!;

    public Guid CatalogBrandId { get; private set; }

    public CatalogBrand CatalogBrand { get; private set; } = null!;

    public int Stock { get; private set; }
    
    private List<OperateLog> _operateLogs = new();

    public IReadOnlyCollection<OperateLog> OperateLogs => _operateLogs;

    private CatalogItem()
    {
    }

    public CatalogItem(string name, decimal price, string pictureFileName) : this()
    {
        Id = IdGeneratorFactory.SequentialGuidGenerator.NewId();
        Name = name;
        Price = price;
        PictureFileName = pictureFileName;
        AddCatalogDomainIntegrationEvent();
    }

    private void AddCatalogDomainIntegrationEvent()
    {
        var catalogCreatedIntegrationDomainEvent = new CatalogCreatedIntegrationDomainEvent(this);
        this.AddDomainEvent(catalogCreatedIntegrationDomainEvent);
    }

    public void SetCatalogType(int catalogTypeId)
    {
        CatalogTypeId = catalogTypeId;
    }

    public void SetCatalogBrand(Guid catalogBrand)
    {
        CatalogBrandId = catalogBrand;
    }

    public void AddStock(int stock)
    {
        Stock += stock;
    }
}
```

> AuditAggregateRoot：审计聚合根，在聚合根的基础上 **增加** 了 **审计功能** （创建操作人、创建时间、修改操作人、修改时间）
>
> FullAggregateRoot：Full 聚合根，在审计聚合根基础上 **增加** 了 **软删除**

### 领域事件

聚合根支持添加 [**领域事件**](/framework/building-blocks/ddd/domain-event)，它们将在工作单元被保存时入队写入事件总线，并在工作单元提交时发送

```csharp l:4
private void AddCatalogDomainIntegrationEvent()
{
    var catalogCreatedIntegrationDomainEvent = new CatalogCreatedIntegrationDomainEvent(this);
    this.AddDomainEvent(catalogCreatedIntegrationDomainEvent);
}
```

## 枚举类

提供 [枚举类](https://learn.microsoft.com/zh-cn/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types) 基类，使用 [枚举类](https://learn.microsoft.com/zh-cn/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types) 来代替使用枚举

为什么使用枚举类？

* 易于理解和维护：枚举常量是有意义的名称，可以很容易地阐明代码含义，这使得代码的可读性更强。

* 类型安全：枚举类型在编译时会检查类型，这消除了许多运行时错误。

* 可扩展性：添加新的枚举常量非常简单，并且不需要修改现有代码，这使得代码更加灵活和可扩展。

* 易于调试：使用枚举类型可以提高代码的可调试性，因为它允许您在代码中打印枚举常量的名称，而不是数字或其他表示形式。

枚举类需要继承 `Enumeration`，例如：

```csharp l:1
public class CatalogType: Enumeration
{
    public static CatalogType WaterDispenser = new(1, "Water Dispenser");
    
    public CatalogType(int id, string name) : base(id, name)
    {
    }
}
```

根据枚举类的值得到枚举对象：

```csharp
var submitted = Enumeration.FromValue<OrderStatus>(1);
```

## 值对象

是指软件开发中，用来描述某个对象所具有的固有属性值的一种对象，它有两个重要的特征：

* 没有任何标识
* 是不可变的

> 任何属性的变化都将视为一个新的值对象

值对象需要继承 `ValueObject`，例如：

```csharp l:1
public class Address : ValueObject
{
    public String Street { get; private set; }
    public String City { get; private set; }
    public String State { get; private set; }
    public String Country { get; private set; }
    public String ZipCode { get; private set; }

    public Address() { }

    public Address(string street, string city, string state, string country, string zipCode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
    }

    protected override IEnumerable<object> GetEqualityValues()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return ZipCode;
    }
}
```

> 通过 `Equals` 方法用来检查两个值对象是否相等  **var isEqual = address1.Equals(address2)**

## 高级

### 复合主键 

如果需要复合主键，而不是 `ID` 主键，则可以通过重写 `GetKeys()` 方法

```csharp l:1,9-13
public class UserRole : Entity
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }
    
    public DateTime CreationTime { get; set; }

    public override IEnumerable<(string Name, object Value)> GetKeys()
    {
        yield return ("UserId", UserId);
        yield return ("RoleId", RoleId);
    }
}
```

> 聚合根也类似，不使用泛型时，可通过重写 `GetKeys()` 方法来完成复合主键