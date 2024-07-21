# 扩展 - 枚举

## 概述

提供常用枚举扩展方法

## 功能

* [`Enum`类型扩展](#enum)
* [`Enum`帮助类](#utils)

## 使用

1. 安装 `Masa.Utils.Extensions.Enums` 

   ```shell 终端
   dotnet add package Masa.Utils.Extensions.Enums
   ```

2. 获取枚举类型的描述

   :::: code-group
   ::: code-group-item Human
   ```csharp Human.cs
   public enum Human
   {
       [Description("BOY")]
       Boy = 1,
       Girl
   }
   ```
   :::
   ::: code-group-item Main

   ```csharp l:3,7
   public void Main()
   {
       var value = Human.Boy.GetDescriptionValue();
       Assert.IsNotNull(value);
       Assert.AreEqual("BOY", value);
   
       value = Human.Girl.GetDescriptionValue();
       Assert.IsNotNull(value);
       Assert.AreEqual(nameof(Human.Girl), value);
   }
   ```
   :::
   ::::

## 其它示例

### Enum 类型扩展

* GetDescriptionValue()：得到枚举的描述，如果未增加描述特性则返回当前枚举Name

  ```csharp l:3,7
  public void Main()
  {
      var value = Human.Boy.GetDescriptionValue();
      Assert.IsNotNull(value);
      Assert.AreEqual("BOY", value);
  
      value = Human.Girl.GetDescriptionValue();
      Assert.IsNotNull(value);
      Assert.AreEqual(nameof(Human.Girl), value);
  }
  
  public enum Human
  {
      [Description("BOY")]
      Boy = 1,
      Girl
  }
  ```

* GetCustomAttribute\<TAttribute\>()：得到枚举的自定义特性信息

   ```csharp l:3,7
   public void Main()
   {
       var attribute = Human.Boy.GetCustomAttribute<DescriptionAttribute>();
       Assert.IsNotNull(attribute);
       Assert.AreEqual("BOY", attribute.Description);
   
       attribute = Human.Girl.GetCustomAttribute<DescriptionAttribute>();
       Assert.IsNotNull(attribute);//若枚举未增加特性会默认初始化
       Assert.AreEqual(string.Empty, attribute.Description);
   }
   ```

* GetCustomAttribute\<TAttribute\>(Func\<TAttribute?\> defaultFunc)：得到枚举的自定义特性，当特性不存在时，返回 `defaultFunc` 的值

   ```csharp l:7
   public void Main()
   {
       var attribute = Human.Boy.GetCustomAttribute(() => new DescriptionAttribute("男"));
       Assert.IsNotNull(attribute);
       Assert.AreEqual("BOY", attribute.Description);
   
       attribute = Human.Girl.GetCustomAttribute<DescriptionAttribute>(() => new DescriptionAttribute("女"));
       Assert.IsNotNull(attribute);
       Assert.AreEqual("女", attribute.Description);
   }
   ```

### Enum 帮助类

* GetDescriptionValue\<TEnum\>(object? enumValue)：传入枚举的值得到枚举描述，如果枚举值在枚举中不存在，则返回 `null` ，否则返回 `DescriptionAttribute` 的值，如果未增加 `DescriptionAttribute` 特性，则返回其 `name` 值

   ```csharp l:3
   public void Main()
   {
       var value = EnumUtil.GetDescriptionValue<Human>(1);
       Assert.AreEqual("BOY", value);
   
       value = EnumUtil.GetDescriptionValue<Human>(2);
       Assert.AreEqual(nameof(Human.Girl), value);
   
       value = EnumUtil.GetDescriptionValue<Human>(3);
       Assert.AreEqual(null, value);
   }
   ```

* GetDescription\<TEnum\>(object? enumValue)：传入枚举的值并得到描述特性，如果枚举值在枚举中不存在，则返回 `null`，否则返回 `DescriptionAttribute` ，如果未增加 `DescriptionAttribute` 特性，则返回初始化后的 `DescriptionAttribute`，其 `Description` 的值为枚举的 `name` 值

   ```csharp l:3
   public void Main()
   {
       var value = EnumUtil.GetDescriptionValue<Human>(1);
       Assert.AreEqual("BOY", value);
   
       value = EnumUtil.GetDescriptionValue<Human>(2);
       Assert.AreEqual(nameof(Human.Girl), value);
   
       value = EnumUtil.GetDescriptionValue<Human>(3);
       Assert.AreEqual(null, value);
   }
   ```

* DescriptionAttribute? GetDescription(Type enumType, object? enumValue)：传入枚举的值以及枚举类型得到描述特性，如果枚举值在枚举中不存在，则返回`null`，否则返回`DescriptionAttribute`，如果未增加`DescriptionAttribute`特性，则返回初始化后的`DescriptionAttribute`，其`Description`的值为枚举的`name`值 (如果传入`enumType`不是枚举类型，则抛出`NotSupportedException`)
* TAttribute? GetCustomAttribute\<TEnum, TAttribute\>(object? enumValue)：传入枚举的值得到自定义特性，如果枚举值为`null`或在枚举中不存在时返回`null`，否则返回对应的自定义特性的值，如果自定义特性不存在，则返回初始化后的自定义特性

   ```csharp l:3
   public void Main()
   {
       var attribute = EnumUtil.GetCustomAttribute<Human, DescriptionAttribute>(1);
       Assert.IsNotNull(attribute);
       Assert.AreEqual("BOY", attribute.Description);
       
       attribute = EnumUtil.GetCustomAttribute<Human, DescriptionAttribute>(2);
       Assert.IsNotNull(attribute);
       Assert.AreEqual(string.Empty, attribute.Description);
       
       attribute = EnumUtil.GetCustomAttribute<Human, DescriptionAttribute>(3);
       Assert.IsNull(attribute);
   
       attribute = EnumUtil.GetCustomAttribute<Human, DescriptionAttribute>(null);
       Assert.IsNull(attribute);
   }
   ```

* GetCustomAttribute\<TAttribute\>(Type enumType, object? enumValue)：传入枚举的值以及枚举类型得到自定义特性，如果枚举值为`null`或在枚举中不存在时返回`null`，否则返回对应的自定义特性的值，如果自定义特性不存在，则返回初始化后的自定义特性 (如果传入`enumType`不是枚举类型，则抛出`NotSupportedException`)
* GetCustomAttributeDictionary\<TEnum, TAttribute\>()：得到自定义特性字典

   ```csharp l:3
   public void Main()
   {
       var dictionary = EnumUtil.GetCustomAttributeDictionary<Human, DescriptionAttribute>();
       Assert.IsNotNull(dictionary);
       Assert.AreEqual(2, dictionary.Count);
       Assert.IsTrue(dictionary.ContainsKey(Human.Boy));
       Assert.AreEqual("BOY", dictionary[Human.Boy].Description);
       Assert.IsTrue(dictionary.ContainsKey(Human.Girl));
       Assert.AreEqual(string.Empty, dictionary[Human.Girl].Description);
   }
   ```

* GetItems\<TEnum\>()：得到枚举的所有值

   ```csharp
   public void Main()
   {
       var list = EnumUtil.GetItems<Human>().ToList();
       Assert.AreEqual(2, list.Count);
       Assert.IsTrue(list.Contains(Human.Boy));
       Assert.IsTrue(list.Contains(Human.Girl));
   }
   ```

* GetList\<TEnum\>(bool withAll = false, string allName = "所有", int allValue = default)：得到枚举列表，常被用于渲染下拉框 (withAll：是否增加`所有`选项，默认不增加)

   ```csharp
   public void TestGetList()
   {
       var list = EnumUtil.GetList<Human>().ToList();
       Assert.AreEqual(2, list.Count);
       Assert.AreEqual(1, list[0].Value);
       Assert.AreEqual(2, list[1].Value);
       Assert.AreEqual("BOY", list[0].Name);
       Assert.AreEqual(nameof(Human.Girl), list[1].Name);
   
       list = EnumUtil.GetList<Human>(true).ToList();
       Assert.AreEqual(3, list.Count);
       Assert.AreEqual(1, list[1].Value);
       Assert.AreEqual(2, list[2].Value);
       Assert.AreEqual("BOY", list[1].Name);
       Assert.AreEqual(nameof(Human.Girl), list[2].Name);
   }
   ```

* GetList(Type enumType, bool withAll = false, string allName = "所有", int allValue = default)：传入枚举类型，得到枚举列表，常被用于渲染下拉框 (withAll：是否增加`所有`选项，默认不增加. 如果传入`enumType`不是枚举类型，则抛出`NotSupportedException`)
* List\<EnumObject\<TEnum?\>\> GetEnumList\<TEnum\>(bool withAll = false, string allName = "所有", TEnum? allValue = default)：得到枚举列表，且`TValue`的值是枚举类型

   ```csharp
   public void Main()
   {
       var list = EnumUtil.GetEnumList<Human>().ToList();
       Assert.AreEqual(2, list.Count);
       Assert.AreEqual(Human.Boy, list[0].Value);
       Assert.AreEqual(Human.Girl, list[1].Value);
       Assert.AreEqual("BOY", list[0].Name);
       Assert.AreEqual(nameof(Human.Girl), list[1].Name);
   }
   ```