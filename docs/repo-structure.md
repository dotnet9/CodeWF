# 目录结构

当前仓库建议按开源项目规范维护：

```text
CodeWF/
  src/
    CodeWF.Api/
    CodeWF.Web/
    CodeWF.Admin/
  docs/
    architecture.md
    solution-overview.md
    detailed-design.md
    repo-structure.md
    assets/
      architecture.svg
      admin-shell.svg
  tests/
```

## 目录职责

- `src/CodeWF.Api`
  - 统一内容读写。
  - 提供站点、文章、资源、Git 和后台管理接口。
- `src/CodeWF.Web`
  - 公开站点。
  - 负责文章阅读、工具浏览、搜索、页面展示。
- `src/CodeWF.Admin`
  - 后台工作台。
  - 负责维护内容和站点配置。
- `docs`
  - 放设计说明、迁移说明、截图或 SVG 图。
  - 新增设计内容优先放这里，不要散落到源码目录。

## 维护建议

1. 新功能先确定它属于前台、后台还是 API。
2. 页面样式和逻辑不要混写成一个难以维护的大文件。
3. 设计图优先用 SVG，便于后续手工修改。
4. 与内容模型相关的变更，先改 API，再改前端，再补文档。

