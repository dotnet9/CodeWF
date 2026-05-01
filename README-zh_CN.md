# CodeWF

CodeWF 是 `dotnet9.com` / `codewf.com` 的网站源代码仓库。

English version: [README.md](./README.md)
更新日志：[CHANGELOG-zh_CN.md](./CHANGELOG-zh_CN.md)

当前网站基于 ASP.NET Core Razor Pages 构建，并把同级仓库 `Assets.Dotnet9` 作为文件型内容仓库使用。文章、文档、时间线、工具元数据、图片以及站点级 Markdown 页面，都会在运行时从该仓库读取。

## 仓库关系

- 网站源码（本地）：`D:\github\owner\CodeWF`
- 内容与资源（本地）：`D:\github\owner\Assets.Dotnet9`
- 网站源码（GitHub）：[https://github.com/dotnet9/CodeWF](https://github.com/dotnet9/CodeWF)
- 内容与资源（GitHub）：[https://github.com/dotnet9/Assets.Dotnet9](https://github.com/dotnet9/Assets.Dotnet9)

## 技术栈

- .NET 11
- ASP.NET Core Razor Pages
- Bootstrap
- Markdig
- 基于本地资源目录的文件型内容加载

## 目录结构

```text
src/WebApp/
  Components/        视图组件
  Controllers/       MVC 控制器
  Models/            内容模型与页面模型
  Pages/             Razor Pages 页面
  Services/          内容加载、搜索与站点服务
  wwwroot/           站点自身静态资源

tests/WebApp.Tests/
  AppServiceTests.cs 最小测试集
```

## 内容加载方式

`AppService` 会从 `Site:LocalAssetsDir` 指向的本地资源目录读取内容。

主要输入包括：

- `site/albums.json`
- `site/categories.json`
- `site/friend-links.json`
- `site/timelines.json`
- `site/doc/navigation.json`
- `site/tools/tools.json`
- `site/search-keywords.json`
- `site/blocked-search-keywords.json`
- `site/about.md`
- `site/pays/Donation.md`
- `2019/` 到当前年份的文章 Markdown 目录

当前前台常用路由包括：

- `/post` 全部文章
- `/project` 项目/文档中心
- `/tool` 工具目录
- `/s` 搜索页
- `/blog` 旧文章入口兼容路由
- `/search` 搜索兼容路由
- `/doc` 文档兼容路由
- `/sitemap` 和 `/sitemap.xml` 站点地图

## 本地开发

1. 将两个仓库并排克隆到本地。
2. 确认 `src/WebApp/appsettings.json` 中的 `Site:LocalAssetsDir` 指向本机的 `Assets.Dotnet9` 目录。
3. 运行：

```powershell
cd D:\github\owner\CodeWF\src\WebApp
dotnet run
```

## 开发体验

开发环境下，`AppService` 会通过 `FileSystemWatcher` 监听内容仓库中的 Markdown、JSON 和常见图片资源变化，并自动失效内存缓存。

这意味着：

- 改文章不用手动重启站点
- 改分类、专题、导航后刷新页面即可看到新结果
- 改配图资源后也能快速验证页面效果

说明：`site/search-keywords.json` 由搜索行为自动维护，不会触发整站缓存失效。

## 测试与 CI

仓库现在包含一套最小护栏：

- GitHub Actions 构建检查：`.github/workflows/build.yml`
- xUnit 最小测试集：`tests/WebApp.Tests`

当前测试覆盖了这些基础场景：

- Front Matter 解析
- Markdown 转 HTML
- 非法搜索关键词拦截

本地执行：

```powershell
dotnet test D:\github\owner\CodeWF\CodeWF.slnx
```

## 配置约定

不要把真实密钥提交到受版本控制的配置文件中。

推荐通过环境变量覆盖：

- `OpenAI__Key`
- `OpenAI__Endpoint`
- `OpenAI__ChatModel`
- `Site__LocalAssetsDir`
- `Site__Domain`

PowerShell 示例：

```powershell
$env:Site__LocalAssetsDir = "D:\github\owner\Assets.Dotnet9"
$env:OpenAI__Key = "your-real-key"
dotnet run --project D:\github\owner\CodeWF\src\WebApp
```

## 内容维护流程

1. 在 `Assets.Dotnet9` 中新增或更新 Markdown、图片和站点数据。
2. 保持文章 Front Matter 完整，至少包含 `title`、`slug`、`description`、`date`、`categories`、`cover`。
3. 当分类、专题、文档、工具或友情链接发生变化时，同步更新 `site/*.json`。
4. 本地运行站点，验证相关页面渲染是否正常。

## 更专业的仓库习惯

- 让内容源仓库和应用源码仓库职责清晰分离。
- 让受版本控制的配置文件不包含真实密钥。
- 尽量依赖显式元数据，而不是隐式约定。
- 在扩大协作前，先把内容结构文档化。
- 每次内容变更后都验证首页、列表页和详情页。
