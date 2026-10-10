# CodeWF

CodeWF 是一个前后端分离的 Markdown 博客站点。

## 项目结构

- `src/CodeWF.Api`：ASP.NET Core Web API，负责读取文件型内容仓库、渲染 Markdown，提供公开内容接口、RSS 与站点地图。
- `src/CodeWF.Web`：Next.js 前台站点，提供文章、专题、搜索、时间线与多语言内容。
- `docs`：架构与迁移文档。
- `tests/CodeWF.Api.Tests`：后端内容解析与渲染相关测试。

在线工具已独立为工具箱站 `https://tools.codewf.com`（仓库 `dotnet9/Tools`），站点不再内置工具页面与后台管理端。

默认资源仓库路径为：

```text
D:\wwwroot\img1.dotnet9.com
```

多语言资源通过同名后缀文件加载，例如 `about.en.md`、`categories.ja.json`、`navigation.zh-tw.json`。缺少对应语言文件时，会回退到默认中文内容。

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

启动后端与前台：

```powershell
npm run dev:api
npm run dev:frontend
```

默认访问地址：

- 前台站点：`http://localhost:5000`
- 后端接口：`http://localhost:5002`

## 配置说明

后端配置位于 `src/CodeWF.Api/appsettings.json`，也可以通过环境变量覆盖：

```powershell
$env:Site__LocalAssetsDir = "D:\github\apps\Assets.Dotnet9"
$env:Site__AssetBaseUrl = "https://img1.dotnet9.com"
$env:Cors__AllowedOrigins__0 = "https://example.com"
```

站点不提供后台管理端：文章与 JSON 资源直接以文件维护，改完提交到内容仓库即可。生产环境需明确配置允许跨域访问的来源。

## 构建与测试

```powershell
dotnet test CodeWF.slnx
npm run build:frontend
```

## 安全要求

仓库已移除后台管理端与相关接口，不再提供登录入口。安全问题报告方式和部署要求见 `SECURITY.md`。

## 参与贡献

欢迎提交问题和合并请求。进行较大改动前，请先阅读 `CONTRIBUTING.md`。

## 许可证

本项目使用 MIT 许可证，详见 `LICENSE`。
