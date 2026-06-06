# CodeWF

CodeWF 已重构为前后端分离的博客 + 在线工具网站。

## 架构

- `src/CodeWF.Api`：ASP.NET Core Web API，负责读取文件型资源仓库、渲染 Markdown、提供前台内容接口、后台文章接口、RSS、sitemap 和资源仓库 Git 操作。
- `src/CodeWF.Web`：Next.js 前台站点，负责 SEO 友好的文章、项目、搜索、时间线、i18n/l10n 页面，以及浏览器端在线工具。
- `src/CodeWF.Admin`：React + Ant Design 后台管理，负责文章维护和资源仓库维护。
- `docs`：架构说明、迁移清单和运维文档。
- `tests/CodeWF.Api.Tests`：后端内容解析测试。

默认资源仓库目录：

```text
D:\wwwroot\img1.dotnet9.com
```

本地化资源会读取同级文件，例如 `about.en.md`、`tools.ja.json`、`navigation.zh-tw.json`，不存在时回退到默认中文内容。

## 环境要求

- .NET 10 SDK
- Node.js 22+
- Git 命令可用

## 本地开发

安装依赖：

```powershell
npm install
dotnet restore CodeWF.slnx
```

启动三个应用：

```powershell
npm run dev:api
npm run dev:frontend
npm run dev:admin
```

默认地址：

- API：`http://localhost:5100`
- 前台：`http://localhost:3000`
- 后台：`http://localhost:3001`

## 配置

关键后端配置在 `src/CodeWF.Api/appsettings.json`，也可以用环境变量覆盖：

```powershell
$env:Site__LocalAssetsDir = "D:\wwwroot\img1.dotnet9.com"
$env:Site__AssetBaseUrl = "https://img1.dotnet9.com"
$env:Admin__ApiKey = "change-me"
```

`Admin__ApiKey` 为空时后台接口默认开放，方便本地开发。生产环境应设置该值，并在后台页面填写相同密钥。

## 构建与测试

```powershell
dotnet test CodeWF.slnx
npm run build:frontend
npm run build:admin
```

本分支不会自动提交或推送代码。
