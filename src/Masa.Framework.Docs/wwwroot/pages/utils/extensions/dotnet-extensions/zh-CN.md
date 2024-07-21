# 扩展 - 常用扩展

## 概述

提供常用扩展方法，它不依赖任何包

## 功能

* [`String`类型扩展](#string-7c7b578b62695c55)
* [`Type`类型扩展](#type-7c7b578b62695c55)
* [`Object`类型扩展](#object-7c7b578b62695c55)
* [`MethodInfo`类型扩展](#methodinfo-7c7b578b62695c55)
* [`JsonSerializerOptions`类型扩展](#jsonserializeroptions-7c7b578b62695c55)
* [`Byte数组扩展`](#byte65707ec462695c55)
* [`Stream扩展`](#stream62695c55)
* [常用工具类](#section-5e3875285de551777c7b)
    * [特性帮助类](#section-5e3875285de551777c7b)
    * [环境帮助类](#section-5e3875285de551777c7b)
    * [网络帮助类](#section-5e3875285de551777c7b)

## 源码解读

### String 类型扩展

* IsNullOrWhiteSpace()：指定的字串是否为 `null`、空还是仅由空白字符组成

* IsNullOrEmpty()：指定的字串是否为 `null`、空字符串 ("")

* TrimStart(string trimParameter)：从当前字符串删除以 `{trimParameter}` 开头的字符串

* TrimStart(string trimParameter, StringComparison stringComparison)：从当前字符串删除以 `{trimParameter}` 开头的字符串（确定在比较时根据 `{stringComparison}` 规则进行匹配）

* TrimEnd(string trimParameter)：从当前字符串删除以 `{trimParameter}` 结尾的字符串

* TrimStart(string trimParameter, StringComparison stringComparison)：从当前字符串删除以 `{trimParameter}` 结尾的字符串（确定在比较时根据 `{stringComparison}` 规则进行匹配）

* ConvertToBytes()：将一组字符编码为一个字节序列 [查看详细](https://learn.microsoft.com/zh-cn/dotnet/api/system.text.encoding.getbytes)

* FromBase64String()：将指定的字符串（它将二进制数据编码为 `Base64` 数字）转换为等效的 8 位无符号整数数组 [查看详细](https://learn.microsoft.com/zh-cn/dotnet/api/system.convert.frombase64string)

* GetSpecifiedLengthString(int length, Action action, FillType fillType = FillType.NoFile, char fillCharacter = ' ')：获取指定长度的字符串

  > 如果 `fillType` 为 `NoFile` 且输入的字符串长度小于指定长度时，执行 `action` ，如果超出指定长度则被会截断，如果不满足指定长度并且 `fillType` 为 `Left` 或者 `Right`，将以 `fillCharacter` 补齐长度

### Type 类型扩展

* GetGenericTypeName()：得到泛型类型名
* IsNullableType()：是否可空类型
* IsImplementerOfGeneric(Type genericType)：判断是否派生自泛型类，`genericType` 必须是一个泛型类，否则为 `false` 
   
   > 例如：得到`typeof(String).IsImplementerOfGeneric(typeof(IEquatable<>))` 的结果为 `true`

### Object 类型扩展

* GetGenericTypeName()：得到泛型类型名

### MethodInfo 类型扩展

* IsAsyncMethod()：得到当前是否是异步方法

### JsonSerializerOptions 类型扩展

* EnableDynamicTypes()：启用动态类型

### Byte数组扩展

* ToBase64String()：将字节数组转换为字符串，使用 base 64 数字编码，所以它生成的全部是 ASCII 字符. [查看详细](https://learn.microsoft.com/zh-cn/dotnet/api/system.convert.tobase64string)
* ConvertToString()：将字节数组转换为字符串 (就是转换成我们平常所认识的字符串，但某些整数序列无法对应现实中的文字，因此会出现方块或者问号) [查看详细](https://learn.microsoft.com/zh-cn/dotnet/api/system.text.encoding.getstring)

### Stream扩展

* ConvertToBytes()：将流转换为字节数组
* ConvertToBytesAsync()：将流转换为字节数组 (异步)

### 常用工具类

#### 特性帮助类

* GetDescriptionValueByField：得到指定字段的描述
* GetCustomAttribute：得到自定义特性
* GetCustomAttributeValue：得到指定字段指定特性的值
* GetCustomAttributes：得到指定类、指定字段下特性的值集合

示例 demo：

```csharp
public void Main()
{
    var value = AttributeUtils.GetDescriptionByField<TestErrorCode>(nameof(ErrorCode.FRAMEWORK_PREFIX));
    Assert.AreEqual("Test Framework Prefix", value);
}

public class TestErrorCode
{
    [System.ComponentModel.Description("Test Framework Prefix")]
    public const string FRAMEWORK_PREFIX = "MF";
}
```

#### 环境帮助类

* TrySetEnvironmentVariable(string environment, string? value)：尝试设置环境变量的值，如果未设置或者值为`空还是仅由空白字符组成`则更新环境变量，并返回 true ，否则返回 false

#### 网络帮助类

* GetPhysicalAddress()：得到 `MAC` 地址，如果获取失败则返回空字符串