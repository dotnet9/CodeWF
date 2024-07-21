# 扩展 - FluentValidation 验证

## 概述

提供基于 [`FluentValidation`](https://www.nuget.org/packages/FluentValidation) 的参数验证扩展

扩展验证支持了 `zh-CN`（中文简体）、`en-US`（英语(美国)）、 `en-GB`（英语(英国)）的本地化验证支持

## 使用

1. 安装`Masa.Utils.Extensions.Validations.FluentValidation`

   ```shell 终端
   dotnet add package Masa.Utils.Extensions.Validations.FluentValidation
   ```

2. 如何使用

   ```csharp
   public class RegisterUser
   {
       public string Account { get; set; }
   
       public string Password { get; set; }
   }
   
   public class RegisterUserValidator : MasaAbstractValidator<RegisterUser>
   {
       public RegisterUserValidator()
       {
           RuleFor(user => user.Account).Letter();
           
           // WhenNotEmpty 的调用示例
           //_ = WhenNotEmpty(r => r.Phone, r => r.Phone, new PhoneValidator<RegisterUser>());
           //_ = WhenNotEmpty(r => r.Phone, new PhoneValidator<RegisterUser>());
           //_ = WhenNotEmpty(r => r.Phone, r => r.Phone, rule => rule.Phone());
           //_ = WhenNotEmpty(r => r.Phone, rule => rule.Phone());
       }
   }
   ```

## 源码解读

### 验证扩展支持

* Chinese: 中文验证
* Number: 数字验证
* Letter: 字母验证 (大小写均可)
* LowerLetter: 小写字母验证
* UpperLetter: 大写字母验证
* LetterNumber: 字母数字验证
* ChineseLetterUnderline: 中文字母下划线验证
* ChineseLetter： 中文字母验证
* ChineseLetterNumberUnderline: 中文字母数字下划线验证
* ChineseLetterNumber: 中文字母数字验证
* Phone: 手机号验证 (支持 `zh-CN`、 `en-US`、 `en-GB`)
* IdCard: 身份验证 (支持 `zh-CN`)
  * 目前身份验证仅支持中国15、18位身份证

> 不同国家的手机号码有着不同的规则，并不是简单通过输入内容是否数字以及数字的长度来验证手机号码

|  国家  | 文化 | 验证规则  |
|  ----  | ----  | ----  |
| 中国 | `zh-CN` | `^(\+?0?86\-?)?1[345789]\d{9}$` |
| 美国 | `en-US` | `^(\+?1)?[2-9]\d{2}[2-9](?!11)\d{6}$` |
| 英国 | `en-GB` | `^(\+?44\|0)7\d{9}$` |

* IdCard: 身份证验证 (支持 `zh-CN`)
* Url: `Url` 地址验证
* Port: 端口验证
* Required: 必填验证 (与`NotEmpty`效果一致)

### MasaAbstractValidator&lt;T&gt; 基类

提供了 `WhenNotEmpty` 方法，只有当给定的属性值不为空（ `NULL/Empty` /空集合/默认值）的时候才会进入验证

```csharp
// WhenNotEmpty 的调用示例
//_ = WhenNotEmpty(r => r.Phone, r => r.Phone, new PhoneValidator<RegisterUser>());
//_ = WhenNotEmpty(r => r.Phone, new PhoneValidator<RegisterUser>());
//_ = WhenNotEmpty(r => r.Phone, r => r.Phone, rule => rule.Phone());
//_ = WhenNotEmpty(r => r.Phone, rule => rule.Phone());
```

> 基类的所有方法都是`Virtual`的，用户可以根据实际需要进行重写。

### 修改默认语言

`GlobalValidationOptions.SetDefaultCulture("zh-CN");`

针对`Phone`、`IdCard`等支持多语言的验证器，其`culture`获取顺序为

指定`culture` ->（全局设置默认Culture）-> `CultureInfo.CurrentUICulture.Name`

用`IdCard`验证器举例：

`IdCard<T>(string? culture = null)` 这个静态扩展方法接收一个可空的 `culture` 参数

1. 如果参数 `culture` 传入 `zh-CN` （**大小写不敏感**），那么验证器会调用对应 `culture` 的验证器 `FluentValidation.ChinaIdCardProvider` 的实现来进行校验。
2. 如果未传入参数 `culture`，方法会读取 `GlobalValidationOptions.DefaultCulture` ，该属性可以通过 `GlobalValidationOptions.SetDefaultCulture(string culture)` 方法进行设置。
3. 如果项目未调用 `GlobalValidationOptions.SetDefaultCulture` 来设置，那么 `GlobalValidationOptions.DefaultCulture` 会获取 `System.Globalization.CultureInfo.CurrentUICulture.Name` 的值。
4. 如果获取到不被支持的 `culture` ，验证的时候会抛出异常 `NotSupportedException`。

> `IdCard` 目前提供支持 `zh-CN` 的验证器。
> `Phone` 目前提供支持 `zh-CN`、 `en-US`、 `en-GB` 的验证器。
> 这里设置的`culture`与[国际化](/framework/building-blocks/globalization/overview)没有关联关系。

### 本地化支持

更换默认验证器语言中的提示信息

```csharp
ValidatorOptions.Global.LanguageManager = new MasaLanguageManager();
```

> 当你使用了 `MASA Framework` 提供的扩展验证器，如果未指定默认语言信息，而且没有重新指定 `LanguageManager`，将会导致获取到**不正确的验证信息**。
