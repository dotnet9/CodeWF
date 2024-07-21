# 扩展 - 表达式

## 概述

提供 `Expression` 扩展方法

## 使用

1. 安装 `Masa.Utils.Extensions.Expressions`

   ```shell 终端
   dotnet add package Masa.Utils.Extensions.Expressions
   ```

2. 使用 **并且** 拼接表达式

   ```csharp l:8
   public void Main()
   {
       var list = new List<int>()
       {
           1, 2, 3, 4, 5, 6, 7
       };
       Expression<Func<int, bool>> condition = i => i > 0;
       condition = condition.And(i => i < 5);
       var result = _list.Where(condition.Compile()).ToList();
       
       Assert.AreEqual(4, result.Count);
       Assert.AreEqual(1, result[0]);
       Assert.AreEqual(2, result[1]);
       Assert.AreEqual(3, result[2]);
       Assert.AreEqual(4, result[3]);
   }
   ```

## 其它示例

* And\<T\>(Expression\<Func\<T, bool\>\> second): 合并两个 `Expression` 表达式，条件是 **并且**

   ```csharp l:8
   public void Main()
   {
       var list = new List<int>()
       {
           1, 2, 3, 4, 5, 6, 7
       };
       Expression<Func<int, bool>> condition = i => i > 0;
       condition = condition.And(i => i < 5);
       var result = _list.Where(condition.Compile()).ToList();
       
       Assert.AreEqual(4, result.Count);
       Assert.AreEqual(1, result[0]);
       Assert.AreEqual(2, result[1]);
       Assert.AreEqual(3, result[2]);
       Assert.AreEqual(4, result[3]);
   }
   ```

* And\<T\>(bool isCompose, Expression\<Func\<T, bool\>\>? second): 当 `isCompose` 为 `true` 时，合并两个 `Expression` 表达式，条件是 **并且**

   :::: code-group
   ::: code-group-item Main 单元测试

   ```csharp l:22,23
   public void Main()
   {
       DateTime? startTime = null;
       DateTime? endTime = null;
       var list = GetList(startTime, endTime);
       Assert.AreEqual(3, list.Count);
       startTime = DateTime.Parse("1990-01-01");
       endTime = DateTime.Parse("2000-01-01");
       list = GetList(startTime, endTime);
       Assert.AreEqual(1, list.Count);
   }
     
   private List<Human> GetList(DateTime? startTime, DateTime? endTime)
   {
       var list = new List<Human>()
       {
           new("Tom", DateTime.Parse("2000-12-12")),
           new("Adelaide", DateTime.Parse("1999-12-12")),
           new("Adolf", DateTime.Parse("2005-12-12"))
       };
       Expression<Func<Human, bool>> condition = h => h.BirthdayTime != null;
       condition = condition.And(startTime != null, h => h.BirthdayTime >= startTime);
       condition = condition.And(endTime != null, h => h.BirthdayTime <= endTime);
       return list.Where(condition.Compile()).ToList();
   }
   ```
   :::
   ::: code-group-item Human

   ```csharp
   public class Human
   {
       public Human(string name, DateTime? birthdayTime)
       {
           Name = name;
           BirthdayTime = birthdayTime;
       }
       public string Name { get; set; }
       public DateTime? BirthdayTime { get; set; }
   }
   ```
   :::
   ::::

* Or\<T\>(Expression\<Func\<T, bool\>\> second): 合并两个 `Expression` 表达式，条件是 **或者**

   ```csharp l:4
   public void Main()
   {
       Expression<Func<int, bool>> condition = i => i >5;
       condition = condition.Or(i => i < 2);
       
       var result = _list.Where(condition.Compile()).ToList();
       Assert.AreEqual(3, result.Count);
       Assert.AreEqual(1, result[0]);
       Assert.AreEqual(6, result[1]);
       Assert.AreEqual(7, result[2]);
   }
   ```

* Or\<T\>(bool isCompose, Expression\<Func<T, bool\>\>? second): 当 `isCompose` 为 `true` 时，合并两个 `Expression` 表达式，条件是 **或者**

   :::: code-group
   ::: code-group-item Main 单元测试

   ```csharp l:22,23
   public void Main()
   {
       string? name = null;
       bool? gender = null;
       var list = GetList(name, gender);
       Assert.AreEqual(0, list.Count);
       name = "Tom";
       gender = false;
       list = GetList(name, gender);
       Assert.AreEqual(2, list.Count);
   }
   
   private List<Human> GetList(string? name, bool? gender)
   {
       var list = new List<Human>()
       {
           new("Tom", true, DateTime.Parse("2000-12-12")),
           new("Adelaide", false, DateTime.Parse("1999-12-12")),
           new("Adolf", true, DateTime.Parse("2005-12-12"))
       };
       Expression<Func<Human, bool>> condition = h => false;
       condition = condition.Or(name != null, h => h.Name.Contains(name!));
       condition = condition.Or(gender != null, h => h.Gender == gender);
       return list.Where(condition.Compile()).ToList();
   }
   ```
   :::
   ::: code-group-item Human

   ```csharp Domain/Entities/CatalogType.cs
   public class Human
   {
       public Human(string name, bool gender, DateTime? birthdayTime)
       {
           Name = name;
           Gender = gender;
           BirthdayTime = birthdayTime;
       }
       public string Name { get; set; }
       public bool Gender { get; set; }
       public DateTime? BirthdayTime { get; set; }
   }
   ```
   :::
   ::::

* Compose\<T\>(Expression\<T\> second, Func\<Expression, Expression, Expression\> merge): 组合两个表达式得到新的组合后的结果

   ```csharp l:5
   public void Main()
   {
       Expression<Func<int, bool>> condition = i => i > 5;
       Expression<Func<int, bool>> condition2 = i => i < 7;
       var expression = condition.Compose(condition2, Expression.AndAlso);
       var list = _list.Where(expression.Compile()).ToList();
       Assert.AreEqual(1, list.Count);
       Assert.AreEqual(6, list[0]);
   }
   ```