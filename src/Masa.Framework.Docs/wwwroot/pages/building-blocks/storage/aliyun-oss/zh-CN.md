# 存储 - 阿里云 OSS

## 概述

基于 [阿里云 OSS](https://www.aliyun.com/product/oss) 提供的对象存储

## 使用

1. 安装 `Masa.Contrib.Storage.ObjectStorage.Aliyun`

   ```shell 终端
   dotnet add package Masa.Contrib.Storage.ObjectStorage.Aliyun
   ```

2. 注册阿里云Oss存储

   ```csharp Program.cs
   builder.Services.AddAliyunStorage();
   
   #region 或者通过代码指定传入阿里云存储配置信息使用，无需使用配置文件
   // builder.Services.AddAliyunStorage(new AliyunStorageOptions()
   // {
   //     AccessKeyId = "Replace-With-Your-AccessKeyId",
   //     AccessKeySecret = "Replace-With-Your-AccessKeySecret",
   //     Endpoint = "Replace-With-Your-Endpoint",
   //     RoleArn = "Replace-With-Your-RoleArn",
   //     RoleSessionName = "Replace-With-Your-RoleSessionName",
   //     Sts = new AliyunStsOptions()
   //     {
   //         RegionId = "Replace-With-Your-Sts-RegionId",
   //         DurationSeconds = 3600,
   //         EarlyExpires = 10
   //     }
   // }, "storage1-test");
   #endregion
   ```

3. 新增阿里云配置

   ```json appsettings.json
   {
     "Aliyun": {
       "AccessKeyId": "Replace-With-Your-AccessKeyId",
       "AccessKeySecret": "Replace-With-Your-AccessKeySecret",
       "Sts": {
         "RegionId": "Replace-With-Your-Sts-RegionId",
         "DurationSeconds": 3600,
         "EarlyExpires": 10
       },
       "Storage": {
         "Endpoint": "Replace-With-Your-Endpoint",
         "RoleArn": "Replace-With-Your-RoleArn",
         "RoleSessionName": "Replace-With-Your-RoleSessionName",
         "TemporaryCredentialsCacheKey": "Aliyun.Storage.TemporaryCredentials",
         "Policy": "",
         "BucketNames" : {
           "DefaultBucketName" : "storage1-test"//默认BucketName，非必填项，仅在使用IClientContainer时需要指定
         }
       }
     }
   }
   ```

4. 上传文件

   ```csharp
   app.MapPost("/upload", async (HttpRequest request, IClientContainer clientContainer) =>
   {
       var form = await request.ReadFormAsync();
       var formFile = form.Files["file"];
       if (formFile == null)
           throw new FileNotFoundException("Can't upload empty file");
   
       await clientContainer.PutObjectAsync(formFile.FileName, formFile.OpenReadStream());
   });
   ```
   
   > 上传对象（上传到默认 `Bucket` ）

## 其它示例

### 存储客户端 IClient

#### 上传对象

```csharp
app.MapPost("/upload", async (HttpRequest request, IClient client) =>
{
    var form = await request.ReadFormAsync();
    var formFile = form.Files["file"];
    if (formFile == null)
        throw new FileNotFoundException("Can't upload empty file");

    await client.PutObjectAsync("storage1-test", formFile.FileName, formFile.OpenReadStream());
});
``` 

> Form 表单提交，key 为 file，类型为文件上传

#### 删除对象

```csharp
public class DeleteRequest
{
    public string Key { get; set; }
}

app.MapDelete("/delete", async (IClient client, [FromBody] DeleteRequest request) =>
{
    await client.DeleteObjectAsync("storage1-test", request.Key);
});
```

#### 判断对象是否存在

```csharp
app.MapGet("/exist", async (IClient client, string key) =>
{
    await client.ObjectExistsAsync("storage1-test", key);
});
```

#### 返回对象数据的流

```csharp
app.MapGet("/download", async (IClient client, string key, string path) =>
{
    await client.GetObjectAsync("storage1-test", key, stream =>
    {
        //Download the file to the specified path
        using var requestStream = stream;
        byte[] buf = new byte[1024];
        var fs = File.Open(path, FileMode.OpenOrCreate);
        int len;
        while ((len = requestStream.Read(buf, 0, 1024)) != 0)
        {
            fs.Write(buf, 0, len);
        }
        fs.Close();
    });
});
```

#### 获取临时凭证(STS)

```csharp
app.MapGet("/GetSts", (IClient client) =>
{
    client.GetSecurityToken();
});
```

> [阿里云](https://www.aliyun.com/product/oss) 平台使用 `STS` 来获取临时凭证

### 存储客户端容器 IClientContainer

#### 上传文件到默认 Bucket

```csharp
app.MapPost("/upload", async (HttpRequest request, IClientContainer clientContainer) =>
{
    var form = await request.ReadFormAsync();
    var formFile = form.Files["file"];
    if (formFile == null)
        throw new FileNotFoundException("Can't upload empty file");

    await clientContainer.PutObjectAsync(formFile.FileName, formFile.OpenReadStream());
});
``` 

#### 上传文件到指定 Bucket

```csharp
[BucketName("picture")]
public class PictureContainer
{
}

builder.Services.Configure<StorageOptions>(option =>
{
    option.BucketNames = new BucketNames(new List<KeyValuePair<string, string>>()
    {
        new("DefaultBucketName", "storage1-test"),//Default BucketName
        new("picture", "storage1-picture")//Specify the BucketName with the alias picture as storage1-picture
    });
});

app.MapPost("/upload", async (HttpRequest request, IClientContainer<PictureContainer> clientContainer) =>
{
    var form = await request.ReadFormAsync();
    var formFile = form.Files["file"];
    if (formFile == null)
        throw new FileNotFoundException("Can't upload empty file");

    await clientContainer.PutObjectAsync(formFile.FileName, formFile.OpenReadStream());
});
```