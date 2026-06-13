# CodeWF

CodeWF 是一个前后端分离的博客与在线工具网站。

## 项目结构

- `src/CodeWF.Api`：ASP.NET Core Web API，负责读取文件型内容仓库、渲染 Markdown、提供公开内容接口、后台管理接口、RSS、站点地图，以及受限的 Git 维护接口。
- `src/CodeWF.Web`：Next.js 前台站点，提供文章、项目、搜索、时间线、多语言内容和浏览器端工具页面。
- `src/CodeWF.Admin`：React + Ant Design 后台管理端，用于维护文章、站点内容、资源文件和代码仓库。
- `docs`：架构、迁移和运维文档。
- `tests/CodeWF.Api.Tests`：后端内容解析与安全相关测试。

默认资源仓库路径为：

```text
D:\wwwroot\img1.dotnet9.com
```

多语言资源通过同名后缀文件加载，例如 `about.en.md`、`tools.ja.json`、`navigation.zh-tw.json`。缺少对应语言文件时，会回退到默认中文内容。

## 运行要求

- .NET 10 SDK
- Node.js 22 或更高版本
- Git 命令行工具

## 本地开发

安装依赖：

```powershell
npm install
dotnet restore CodeWF.slnx
```

启动后端、前台和后台：

```powershell
npm run dev:api
npm run dev:frontend
npm run dev:admin
```

默认访问地址：

- 前台站点：`http://localhost:5000`
- 后台管理端：`http://localhost:5001`
- 后端接口：`http://localhost:5002`

## 配置说明

后端配置位于 `src/CodeWF.Api/appsettings.json`，也可以通过环境变量覆盖：

```powershell
$env:Site__LocalAssetsDir = "D:\github\apps\Assets.Dotnet9"
$env:Site__AssetBaseUrl = "https://img1.dotnet9.com"
$env:Cors__AllowedOrigins__0 = "https://example.com"
$env:Admin__Read__0__UserName = "reader"
$env:Admin__Read__0__Password = "replace-with-a-secret"
$env:Admin__Super__0__UserName = "admin"
$env:Admin__Super__0__Password = "replace-with-a-strong-secret"
```

后台管理端会使用登录会话访问受保护接口。生产环境必须替换默认开发账号，并明确配置允许跨域访问的来源。

## 构建与测试

```powershell
dotnet test CodeWF.slnx
npm run build:frontend
npm run build:admin
```

## 安全要求

不要在生产环境使用默认后台账号。安全问题报告方式和部署要求见 `SECURITY.md`。

## 参与贡献

欢迎提交问题和合并请求。进行较大改动前，请先阅读 `CONTRIBUTING.md`。

## 许可证

本项目使用 MIT 许可证，详见 `LICENSE`。
