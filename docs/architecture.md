# CodeWF Architecture

CodeWF is organized as an open-source monorepo:

```text
src/
  CodeWF.Api/     ASP.NET Core Web API
  CodeWF.Web/     Next.js public website
  CodeWF.Admin/   React + Ant Design admin console
tests/
  CodeWF.Api.Tests/
docs/
  architecture.md
```

The API owns content loading and mutation. Both React applications consume the API instead of reading the asset repository directly.

The content repository remains file based and defaults to `D:\wwwroot\img1.dotnet9.com`.
