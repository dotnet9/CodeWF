# design — CodeWF 前台 UI 原型

CodeWF 前台站点的 HTML 高保真原型。用浏览器直接打开 `mockups/*.html` 即可预览，无外部字体/脚本/图片依赖（图片以占位块表示）。

## 视觉语言：「Fluent 白 × .NET 紫」

配色参考 [dotnet.microsoft.com](https://dotnet.microsoft.com/zh-cn/) 的 Fluent/.NET 品牌色系，版式保留本站自己的开发者气质：

- **.NET 品牌紫主色**（`#512bd4`）贯穿导航、按钮、标签、时间轴；**Fluent 链接蓝**（`#0078d4`）、幽紫（`#8b5cf6`）、琥珀（`#b25f00`）为辅助色。
- **白底 + 浅灰分区**（`#fff` / `#faf9f8`），克制的小圆角（8–10px）与细描边，微软式的干净素净；页脚浅灰底。
- **深色控制台反差元素**：代码块、首页终端窗口、工具交互面板保持深底（偏紫黑 `#1e1b2e`）+ 亮色文字，形成明暗节奏，是全站记忆点。
- **等宽字体气质**：slug、元数据、面包屑、统计数字全部使用 mono 字体，标题以 `##`/`###` 前缀呼应 Markdown。
- **`Ctrl K` 命令面板搜索**、卡片 hover 上浮泛紫色描边、终端光标闪烁、时间轴发光节点。
- **动效增强**：渐变大标题流动（grad-flow）、主按钮紫蓝渐变呼吸、卡片顶部扫过渐变光线、关于页 Logo 悬浮、首页关键词跑马灯（hover 暂停）。
- **响应式**：全部页面兼容移动端——1024px 以下栅格塌缩为两列/单列、侧栏下移为纵向排布、导航横向滑动；640px 以下文章卡片改为上下布局、表单全宽。

## 界面清单

| 文件 | 对应路由（CodeWF.Web） | 说明 |
|------|------|------|
| `mockups/index.html` | `/[locale]` | 首页：渐变大标题 + 终端窗口 hero、Bento 区（最新文章/热榜/常用工具）、精选文章流 |
| `mockups/blog.html` | `/blog`、`/cat`、`/tag` | 文章列表：mono 分类芯片筛选、标签云、按年归档 |
| `mockups/post.html` | `/post/[slug]` | 文章详情：Markdown 正文排版（代码高亮/引用块）、TOC、相关阅读、上下篇 |
| `mockups/album.html` | `/album` | 专题系列卡片，右上角霓虹光斑区分色系 |
| `mockups/doc.html` | `/doc` | 文档中心：左侧目录树 + 正文 |
| `mockups/tools.html` | `/tool` | 工具总览：全局搜索框、10 分类芯片、分类分节网格 |
| `mockups/tool-detail.html` | `/tool/[slug]`、`/timestamp` 等 | 工具页：暗面板表单、霓虹结果卡、相关工具 |
| `mockups/timeline.html` | `/timeline` | 建站时间线：极光渐变轴 + 发光节点 |
| `mockups/search.html` | `/search`、`/s` | 搜索结果：命令面板输入框、命中高亮 |
| `mockups/about.html` | `/about`、`/donation`、`/privacy` | 关于本站：站长介绍、联系方式、捐赠二维码 |
| `mockups/friends.html` | 友链 | 友链卡片 + 申请友链 |

共享样式见 `mockups/shared.css`。多语言（中/EN/JA/繁）在导航右侧以切换器呈现，对应数据文件的 `*.en.json` / `*.ja.json` / `*.zh-tw.json` 后缀机制。

## 原型数据来源

原型中的文章标题、分类、专题、工具、时间线、友链均为真实数据，取自本地内容仓库 `D:\github\apps\Assets.Dotnet9`：

- 文章：`YYYY/MM/slug.yml`（标题/日期/分类/标签/封面）
- 分类：`site/categories.json`；专题：`site/albums.json`
- 工具：`site/tools/tools.json`（10 分类 91 个工具）
- 时间线：`site/timelines.json`；友链：`site/friend-links.json`
- 关于页：`site/about.md`

## 落地说明

- 前台为 Next.js（`src/CodeWF.Web`），设计变量可直接映射为 Tailwind/CSS 自定义属性（见 `shared.css` 的 `:root`）。
- 占位块 `.img-ph` 在实现中替换为 `https://img1.dotnet9.com` 下的真实封面图。
- `Ctrl K` 命令面板在实现中对应全局搜索（`/search`）。
