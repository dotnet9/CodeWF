# 存储 -概述

在工作中，我们经常需要将文件内容（文件或二进制流）存储在应用程序中，例如你可能要保存商品的封面图片。MASA Framework 为此提供了对象存储的功能，并对功能抽象，抽象给我们带来的好处:

* 存储的无关性（不关心存储平台是阿里云 OSS 还是腾讯云的 COS ）
* 更换存储平台成本更低（仅需要更改下存储的提供者，业务侵染低）
* 支持自定义存储提供者

## 最佳实践

* [阿里云](/framework/building-blocks/storage/aliyun-oss): 数据存储在 [阿里云 OSS](https://www.aliyun.com/product/oss)

后续将逐步提供更多的云存储平台支持，如果您有喜欢的其它云存储平台，欢迎提 [建议](/framework/contribution/overview#section-95ee9898)，或者自己实现它并为 MASA Framework 框架做出 [贡献](/framework/contribution/overview)

## 能力介绍

| 提供者                                                | Sts  |  Token  | 获取文件流 | 获取指定区域文件流 | 上传文件 | 检测文件是否存在 | 删除文件 | 批量删除文件 |
|:---------------------------------------------------| :----: |:----: |:----: |:----: |:----: |:----: |:----: |:----: |
| [阿里云 OSS](/framework/contribs/support-storage/oss) | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

## 源码解读

### 存储客户端 IClient

是用来存储和读取对象的主要接口，可以在项目的任意地方通过 `DI` 获取到 `IClient` 来上传、下载或删除指定 `BucketName` 下的对象，也可用于判断对象是否存在，获取临时凭证等。

* PutObjectAsync：上传对象
* DeleteObjectAsync：删除对象
* ObjectExistsAsync：对象是否存在
* GetObjectAsync：返回对象数据的流
* GetSecurityToken：获取临时凭证(STS)
* GetToken：获取临时凭证（字符串类型的临时凭证）

### 存储空间提供者 IBucketNameProvider

是用来获取 `BucketName` 的接口，通过 `IBucketNameProvider` 可以获取指定存储空间的 `BucketName`，为 `IClientContainer` 提供 `BucketName` 能力，在业务项目中不会使用到

### 存储客户端容器 IClientContainer

对象存储容器，用来存储和读取对象的主要接口，一个应用程序下可能会存在管理多个 `BucketName`，通过使用 `IClientContainer` ，像管理 `DbContext` 一样管理不同 `Bucket` 的对象，不需要在项目中频繁指定 `BucketName` ，在同一个应用程序中，有且只有一个默认 `ClientContainer`，可以通过 `DI` 获取 `IClientContainer` 来使用

* PutObjectAsync：上传对象
* DeleteObjectAsync：删除对象
* ObjectExistsAsync：对象是否存在
* GetObjectAsync：返回对象数据的流
* GetSecurityToken：获取临时凭证(STS)
* GetToken：获取临时凭证（字符串类型的临时凭证）

> 与 `IClient` 最大的区别在于不需要指定 `Bucket`

### 存储客户端工厂

`IClientFactory` 对象存储提供者工厂，通过指定 `BucketName` ，创建指定的 `IClientContainer`