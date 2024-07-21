# 数据序列化 - Json

## 概述

基于 `System.Text.Json` 实现的序列化与反序列化程序

## 使用

1. 安装 `Masa.Contrib.Data.Serialization.Json`

   ```shell 终端
   dotnet add package Masa.Contrib.Data.Serialization.Json
   ```

2. 注册 `json` 序列化/反序列化程序

   ```csharp Program.cs l:3
   var builder = WebApplication.CreateBuilder(args);
   
   builder.Services.AddSerialization(builder => builder.UseJson());
   ```

3. 序列化

   ```csharp l:2-6
   IJsonSerializer jsonSerializer;// 通过DI获取
   jsonSerializer.Serialize(new
   {
       id = 1,
       name = "序列化"
   });
   ```

4. 反序列化

   ```csharp l:12
   public class UserDto
   {
       [JsonPropertyName("id")]
       public int Id { get; set; }
   
       [JsonPropertyName("name")]
       public string Name { get; set; }
   }
   
   var json = "{\"id\":1,\"name\":\"反序列化\"}";
   IJsonDeserializer jsonDeserializer; // 通过DI获取
   var user = jsonDeserializer.Deserialize<UserDto>(json);
   ```

   
