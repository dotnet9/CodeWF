# 安全 - 密码

## 概述

提供常见的密码加密和解密的能力，安装`Masa.Utils.Security.Cryptography`即可

## 功能

* [Aes 加解密](#Aes帮助类)
* [Base64 编码解码](#Base64编码解码)
* [Des 加解密](#Des加解密)
* [Md5 加密](#Md5加密)
* [Sha 系列加密](#Sha系列加密)
* [全局配置](#全局配置)

## 源码解读

> `encoding`为编码格式，默认`UTF-8`

### Aes帮助类

* 默认秘钥：默认：`masastack.com`
* 默认偏移量：16位 默认：`AreyoumySnowman?`
* 默认秘钥长度：**GlobalConfigurationUtils.DefaultAesEncryptKeyLength** (默认32位，仅支持16、24、32)
* 未指定秘钥时将使用默认秘钥
* 未指定偏移量时将使用默认偏移量

> 默认秘钥与偏移量未设置内容长度，如果默认秘钥、偏移量长度不足的将会自动补齐，长度超出则会被截断

#### 加密

* Encrypt：对内容进行 `AES` 加密并返回加密后的字符串
* EncryptToBytes：对内容进行 `AES` 加密并返回加密字节数组
* EncryptFile：对文件流进行 `AES` 加密并将加密后的文件输出到指定目录

#### 解密

* Decrypt：对加密内容进行 `AES` 解密并返回解密后的内容
* DecryptToBytes：对加密字节数组进行 `AES` 解密并返回解密后的字节数组
* DecryptFile：将加密文件流进行 `AES` 解密并将解密后的文件输出到指定目录

### Base64编码解码

* Encrypt：将字符串按照指定编码格式编码并返回编码后的结果（默认编码格式：UTF8）

   ```csharp
   var str = "Hello MASA Stack";
   var encryptResult = Base64Utils.Encrypt(str);
   ```

### Des加解密

* 默认秘钥：`c7fac67c` （8位）
* 默认偏移量：`c7fac67c` （8位）
* 未指定秘钥时将使用默认秘钥
* 未指定偏移量时将使用默认偏移量

> 默认秘钥与偏移量未设置内容长度，如果默认秘钥、偏移量长度不足的将会自动补齐，长度超出则会被截断

* Encrypt：对内容进行 `DES` 加密并返回加密后的字符串
* EncryptFile：对文件流进行 `DES` 加密并将加密后的文件输出到指定目录

* Decrypt：对加密内容进行 `DES` 解密并返回解密后的内容
* DecryptFile：将加密文件流进行 `DES` 解密并将解密后的文件输出到指定目录


### Md5加密

* Encrypt：对内容进行 `md5` 加密并返回加密后的字符串
* EncryptRepeat：对内容进行指定次数的 `md5` 加密并返回加密后的字符串（默认仅加密一次）

   ```csharp
   public void Main()
   {
       var str = "Hello MASA Stack";
       var encryptResult = MD5Utils.Encrypt(str);
       Assert.AreEqual("e7b1bf81bacd21f9396bdbab6d881fe2", encryptResult);
   }
   ```
* EncryptFile：将指定文件进行 `md5` 加密并返回加密后的结果
* EncryptStream：将指定文件流加密并返回加密后的结果

### Sha系列加密

其中支持：`SHA1Utils`、`SHA256Utils`、`SHA384Utils`、`SHA512Utils`

* Encrypt：`Sha` 系列加密

   ```csharp
   public void Main()
   {
       var str = "Hello MASA Stack";
       var encryptResult = SHA256Utils.Encrypt(str);
       Assert.AreEqual("577da9f7698725d8ac8fc73e70b182b5ae47edaf5c2be73524861b3bf0f148dc", encryptResult);
   }
   ```

### 全局配置

* DefaultAesEncryptKey：全局Aes秘钥. 默认值为：`masastack.com                   `
* DefaultAesEncryptIv：全局Aes偏移量. 默认值为：`AreyoumySnowman?`
* DefaultAesEncryptKeyLength：全局Aes秘钥长度. 默认值为：32 (仅支持16、24、32)
* DefaultDesEncryptKey：全局Des秘钥. 默认值为：3b668589
* DefaultDesEncryptIv：全局Aes秘钥. 默认值为：3b668589

   例如:
   
   ```csharp
   GlobalConfigurationUtils.DefaultDesEncryptKey = "12345678";
   ```