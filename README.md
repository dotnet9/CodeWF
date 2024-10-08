# CodeWF

[简体中文](README.zh-CN.md) | English

## 介绍

`CodeWF`是基于`Blazor`开发的一个内容管理系统，网站前台使用`Blazor`静态组件，后台使用`Known`开发框架。

![输入图片说明](https://foruda.gitee.com/images/1727322736359863475/b49a0e20_14334.png "屏幕截图")
![输入图片说明](https://foruda.gitee.com/images/1727322783597844539/eac6cf86_14334.png "屏幕截图")
![输入图片说明](https://foruda.gitee.com/images/1727322688669266612/0b76c1e1_14334.png "屏幕截图")

## 软件架构

- 前台采用`Blazor`静态组件，通过`Http`协议与后端交互
- 后台使用`Known`框架进行开发，采用`Blazor`的交互式`SSR`模式

```
├─AntBlazor      -> AntDesign静态组件库。
├─CodeWF       -> 系统网站前台项目，可以集成到各自的网站中。
├─CodeWF.Admin -> 系统管理后台项目，可以集成到各自的后台系统中。
├─WebAdmin       -> 管理后台示例项目。
├─WebSite        -> 网站前台示例项目。
├─CodeWF.sln   -> 解决方案文件。
```

## 使用说明

- 数据库默认根目录`SQLite`库`CMSLite.db`
- 后台用户名：`Admin`，密码：`1`
- 前台用户名：`known`，密码：`1`

## 赞助

> 如果你觉得这个项目对你有帮助，你可以请作者喝杯咖啡表示鼓励 ☕️

## 感谢

- [KnownCMS](https://gitee.com/known/known-cms)