# 数据序列化 - Yaml

## 概述

基于 [YamlDotNet](https://github.com/aaubry/YamlDotNet) 实现的序列化与反序列化程序

## 使用

1. 安装 `Masa.Contrib.Data.Serialization.Yaml`

   ```shell 终端
   dotnet add package Masa.Contrib.Data.Serialization.Yaml
   ```

2. 注册 `Yaml` 序列化/反序列化程序

   ```csharp Program.cs l:3
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddSerialization(builder => builder.UseYaml());
   ```

3. 序列化

   ```csharp l:23
   var person = new Person
   {
       Name = "Abe Lincoln",
       Age = 25,
       HeightInInches = 6f + 4f / 12f,
       Addresses = new Dictionary<string, Address>{
           { "home", new  Address() {
                   Street = "2720  Sundown Lane",
                   City = "Kentucketsville",
                   State = "Calousiyorkida",
                   Zip = "99978",
               }},
           { "work", new  Address() {
                   Street = "1600 Pennsylvania Avenue NW",
                   City = "Washington",
                   State = "District of Columbia",
                   Zip = "20500",
               }},
       }
   };
   
   IYamlSerializer yamlSerializer;// 通过DI获取
   var yaml = yamlSerializer.Serialize(person);
   ```

4. 反序列化

   ```csharp l:13
   var yml = @"
   name: George Washington
   age: 89
   height_in_inches: 5.75
   addresses:
     home:
       street: 400 Mockingbird Lane
       city: Louaryland
       state: Hawidaho
       zip: 99970
   ";
   IYamlDeserializer yamlDeserializer; // 通过DI获取
   var people = yamlDeserializer.Deserialize<Person>(yml);
   ```

> YamlDotNet序列化和反序列化默认使用的是驼峰命名,下横线命名转换需增加配置
> ```
> builder.UseYaml(configure =>
> {
>     configure.Deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
> });
> ```
   