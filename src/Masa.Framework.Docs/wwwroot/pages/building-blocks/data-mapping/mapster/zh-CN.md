# 数据映射 - Mapster

## 概述

基于 [Mapster](https://github.com/MapsterMapper/Mapster) 的一个对象到对象的映射器，在原来的基础上增加自动获取并使用最佳构造函数映射，支持嵌套映射，减轻映射的工作量。

## 入门

1. 安装 `Masa.Contrib.Data.Mapping.Mapster`

   ```shell 终端
   dotnet add package Masa.Contrib.Data.Mapping.Mapster
   ```

2. 修改 `Program` ，注册 `Mapster` 的映射器

   ```csharp Program.cs l:2
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddMapster();
   ```

3. 映射对象

   :::: code-group
   ::: code-group-item 使用

   ```csharp l:9
   public void Main()
   {
       var request = new
       {
           Name = "Teach you to learn Dapr ……",
           OrderItem = new OrderItem("Teach you to learn Dapr hand by hand", 49.9m)
       };
       IMapper mapper;// 通过DI获取
       var order = _mapper.Map<Order>(request);// 将request映射到新的对象
       Assert.IsNotNull(order);
       Assert.AreEqual(request.Name, order.Name);
       Assert.AreEqual(1, order.OrderItems.Count);
       Assert.AreEqual(49.9m, order.TotalPrice);
   }
   ```

   :::
   ::: code-group-item Order

   ```csharp
   public class Order
   {
       public string Name { get; set; }
       public decimal TotalPrice { get; set; }
       public List<OrderItem> OrderItems { get; set; }
   
       public Order(string name)
       {
           Name = name;
       }
       
       public Order(string name, OrderItem orderItem) : this(name)
       {
           OrderItems = new List<OrderItem> { orderItem };
           TotalPrice = OrderItems.Sum(item => item.Price * item.Number);
       }
   }
   ```

   :::
   ::: code-group-item OrderItem

   ```csharp
   public class OrderItem
   {
       public string Name { get; set; }
       public decimal Price { get; set; }
       public int Number { get; set; }
       
       public OrderItem(string name, decimal price) : this(name, price, 1)
       {
       }
   
       public OrderItem(string name, decimal price, int number)
       {
           Name = name;
           Price = price;
           Number = number;
       }
   }
   ```

   :::
   ::::

## 高级

### 映射规则

* 目标对象没有构造函数时：使用空构造函数，映射到字段和属性。

* 目标对象存在多个构造函数：获取最佳构造函数映射

    > 最佳构造函数: 目标对象构造函数参数数量从大到小降序查找，参数名称一致（不区分大小写）且参数类型与源对象属性一致

### 扩展Map

为方便使用对象映射的能力，而并非必须先通过 `DI` 获取到 `IMapper` 后才能使用，提供了**静态扩展方法**

1. 安装 `Masa.BuildingBlocks.Data.MappingExtensions`

   ```shell 终端
   dotnet add package Masa.BuildingBlocks.Data.MappingExtensions
   ```

2. 注册 `Mapster` 的映射器

   ```csharp Program.cs
   builder.Services.AddMapster();
   ```

3. 使用映射

   ```csharp l:8
   public void Main()
   {
       var request = new
       {
           Name = "Teach you to learn Dapr ……",
           OrderItem = new OrderItem("Teach you to learn Dapr hand by hand", 49.9m)
       };
       var order = request.Map<Order>();// 将request映射到新的对象
   }
   ```
   
   > 扩展 Map 与对象映射的提供者并没有强绑定关系，项目中注入哪一个提供者，那映射方法就会使用哪一个提供者的映射方法