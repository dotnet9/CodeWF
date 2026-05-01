# CodeWF

CodeWF is the website source repository for `dotnet9.com` / `codewf.com`.

Chinese version: [README-zh_CN.md](./README-zh_CN.md)
Changelog: [CHANGELOG.md](./CHANGELOG.md)

The site is built with ASP.NET Core Razor Pages and uses the sibling repository `Assets.Dotnet9` as a file-based content store. Articles, docs, timelines, tool metadata, images, and site-level markdown pages are all loaded from that repository at runtime.

## Repositories

- Website source (local): `D:\github\owner\CodeWF`
- Content and assets (local): `D:\github\owner\Assets.Dotnet9`
- Website source (GitHub): [https://github.com/dotnet9/CodeWF](https://github.com/dotnet9/CodeWF)
- Content and assets (GitHub): [https://github.com/dotnet9/Assets.Dotnet9](https://github.com/dotnet9/Assets.Dotnet9)

## Stack

- .NET 11
- ASP.NET Core Razor Pages
- Bootstrap
- Markdig
- File-based content loading from local assets

## Directory Layout

```text
src/WebApp/
  Components/        View components
  Controllers/       MVC controllers
  Models/            Content and page models
  Pages/             Razor Pages
  Services/          Content loading, search, and site services
  wwwroot/           Static assets for the app itself

tests/WebApp.Tests/
  AppServiceTests.cs Minimal regression tests
```

## How Content Is Loaded

`AppService` reads content from the local assets directory configured by `Site:LocalAssetsDir`.

Main inputs:

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
- `2019/` to current year article markdown trees

Common public routes:

- `/post` all posts
- `/project` project/doc center
- `/tool` tool directory
- `/s` search
- `/blog` legacy blog route
- `/search` legacy search route
- `/doc` legacy doc route
- `/sitemap` and `/sitemap.xml` sitemap

## Local Development

1. Clone both repositories side by side.
2. Make sure `src/WebApp/appsettings.json` points `Site:LocalAssetsDir` to your local `Assets.Dotnet9` path.
3. Run:

```powershell
cd D:\github\owner\CodeWF\src\WebApp
dotnet run
```

## Developer Workflow

In development, `AppService` uses `FileSystemWatcher` to monitor markdown, JSON, and common image assets under the content repository and automatically invalidates in-memory caches.

That means you can:

- edit articles without restarting the app
- tweak categories, docs, or navigation and refresh immediately
- update content images and verify page output quickly

Note: `site/search-keywords.json` is updated by live searches and intentionally does not invalidate all site caches.

## Tests and CI

The repository now includes a minimal safety net:

- GitHub Actions build check: `.github/workflows/build.yml`
- xUnit regression tests: `tests/WebApp.Tests`

Current tests cover:

- front matter parsing
- markdown-to-HTML conversion
- blocked search keyword handling

Run locally:

```powershell
dotnet test D:\github\owner\CodeWF\CodeWF.slnx
```

## Configuration

Do not commit real secrets into tracked config files.

Recommended overrides:

- `OpenAI__Key`
- `OpenAI__Endpoint`
- `OpenAI__ChatModel`
- `Site__LocalAssetsDir`
- `Site__Domain`

Example PowerShell session:

```powershell
$env:Site__LocalAssetsDir = "D:\github\owner\Assets.Dotnet9"
$env:OpenAI__Key = "your-real-key"
dotnet run --project D:\github\owner\CodeWF\src\WebApp
```

## Content Workflow

1. Add or update markdown/images in `Assets.Dotnet9`.
2. Keep article front matter complete: `title`, `slug`, `description`, `date`, `categories`, and `cover`.
3. Update `site/*.json` when categories, albums, docs, tools, or friend links change.
4. Run the site locally and verify the related page renders correctly.

## Professional Repo Checklist

- Keep content source and app source responsibilities separate.
- Keep tracked config files secret-free.
- Prefer explicit metadata over implicit conventions.
- Document content structure before scaling contributors.
- Verify homepage, listing pages, and detail pages after content changes.
