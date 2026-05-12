# CodeWF

CodeWF is now a separated frontend/backend blog and online tools platform.

## Architecture

- `src/CodeWF.Api` - ASP.NET Core Web API. It reads the file-based content repository, renders Markdown, exposes public content APIs, admin article APIs, RSS, sitemap, and Git operations for the asset repository.
- `src/CodeWF.Web` - Next.js public site. It renders SEO-friendly blog, project, search, timeline, i18n/l10n pages, and browser-side online tools.
- `src/CodeWF.Admin` - React + Ant Design admin console for posts and asset repository maintenance.
- `docs` - Architecture, migration notes, and operational documentation.
- `tests/CodeWF.Api.Tests` - API content parsing tests.

The default asset repository path is:

```text
D:\wwwroot\img1.dotnet9.com
```

Localized content is loaded from sibling files such as `about.en.md`, `tools.ja.json`, and `navigation.zh-tw.json`, with fallback to the default Chinese content.

## Requirements

- .NET 10 SDK
- Node.js 22+
- Git available on `PATH`

## Local Development

Install dependencies:

```powershell
npm install
dotnet restore CodeWF.slnx
```

Run the three apps:

```powershell
npm run dev:api
npm run dev:frontend
npm run dev:admin
```

Default URLs:

- API: `http://localhost:5100`
- Public site: `http://localhost:3000`
- Admin: `http://localhost:3001`

## Configuration

Important API settings live under `src/CodeWF.Api/appsettings.json` and can be overridden by environment variables:

```powershell
$env:Site__LocalAssetsDir = "D:\wwwroot\img1.dotnet9.com"
$env:Site__AssetBaseUrl = "https://img1.dotnet9.com"
$env:Admin__ApiKey = "change-me"
```

When `Admin__ApiKey` is empty, admin APIs are open for local development. In production, set it and enter the same value in the admin console.

## Build And Test

```powershell
dotnet test CodeWF.slnx
npm run build:frontend
npm run build:admin
```

No code is committed or pushed by this refactor branch.
