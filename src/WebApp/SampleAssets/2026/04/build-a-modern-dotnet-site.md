Razor Pages 很适合构建内容站、工具站和轻量级产品站。它的页面模型清晰，路由直观，也便于把静态资源仓库中的文章、文档和配置一次性加载到内存中。

## 资源目录

示例资源目录使用下面的结构：

```text
SampleAssets/
  2026/04/
    build-a-modern-dotnet-site.md
    build-a-modern-dotnet-site.yml
  site/
    albums.json
    categories.json
    doc/navigation.json
    tools/tools.json
```

文章正文保存在 `.md`，标题、slug、分类、专题、标签、封面和发布时间保存在同名 `.yml`。站点启动时读取这些静态内容，页面请求只需要从内存中取数据。

## 页面入口

首页会展示推荐文章、专题、分类、随机发现、项目条目和工具入口。真实站点可以把这些数据托管在独立资源仓库中，源码仓库只负责渲染和交互体验。
