# Changelog (CodeWF)

Chinese version: [CHANGELOG-zh_CN.md](./CHANGELOG-zh_CN.md)

V1.1.4 (2026-05-01)

- 😄 [Added] Added development-time asset hot reload for markdown, JSON, and common image assets in the external content repository via `FileSystemWatcher`.
- 😄 [Added] Added a minimal xUnit regression test project and wired it into the solution for front matter parsing, markdown rendering, and blocked-search behavior checks.
- 😄 [Added] Added a GitHub Actions build workflow for the `develop`, `main`, and `master` branches.
- 🔤 [Changed] Expanded both README files with route, content-repository, hot-reload, test, and CI guidance.
- 🔤 [Changed] Added focused Chinese source comments across key site layers, including startup, caching, search, SEO metadata, markdown rendering, and article navigation logic.
- 🔤 [Changed] Updated `.gitignore` so test build outputs stay out of the repository.

V1.1.3 (2026-04-30)

- 😄 [Added] Added the `/tag` tag directory and `/tag/{tag}` tag aggregation pages, with safe encoding and decoding for Chinese tags, `C#`, and other special tag names.
- 😄 [Added] Added search suggestion dropdowns backed by recent high-frequency search terms and matching site content.
- 😄 [Added] Added persisted search keyword popularity in the resource repository at `site/search-keywords.json`, so hot-search suggestions survive service restarts.
- 🔨 [Changed] Added a static search index plus a bounded 20-key in-memory search-result cache for faster repeated searches.
- 🔨 [Changed] Added resource-backed server-side filtering for inappropriate search terms, with Chinese and English keyword configuration plus a friendly search-page fallback.
- 🔨 [Changed] Modernized internal blog page namespaces and URL helper names to align with the current site language.
- 🔨 [Changed] Improved SEO metadata with canonical URLs, Open Graph/Twitter tags, RSS discovery, article structured data, robots.txt, and broader sitemap coverage.
- 🔨 [Changed] Completed missing article tags and cover metadata in the resource repository so tag pages and article previews have cleaner source data.
- 🔨 [Changed] Turned article detail categories, albums, and tags into internal links for easier same-category, same-album, and same-tag exploration.
- 🔨 [Changed] Turned homepage hero statistics into internal links to `/post`, `/project`, and `/tool`.
- 🔨 [Changed] Increased desktop header navigation spacing for a more comfortable top-menu rhythm.
- 🔨 [Changed] Changed article list, homepage, tag, category, album, and navigation data flows to use `BlogPostBrief` instead of carrying full post content.
- 🔨 [Changed] Refined the overall visual clarity by reducing card title size and weight, softening overly dark text, and tuning cards, shadows, and grid backgrounds.
- 🔨 [Changed] Improved Markdown code block readability with Prism highlighting on article pages, language headers, copy buttons, a dark grid background, and smaller readable code typography.
- 🐛 [Fixed] Fixed mobile horizontal overflow in article list pagination.

V1.1.2 (2026-04-29)

- 😄 [Added] Added "Start Here" and "Unexpected Finds" homepage modules to help first-time visitors enter the content flow and continue reading more naturally.
- 😄 [Added] Added the `/project` project hub and project detail pages for open-source projects, NuGet packages, and their usage guides.
- 🔨 [Changed] Unified the site URL scheme around shorter routes: blog index is now `/post`, search is `/s`, categories use `/cat/{slug}`, albums use `/album/{slug}`, and article details use `/{yyyy}/{MM}/{slug}`.
- 🔨 [Changed] Removed legacy `/...` route compatibility and the old `/doc` entry, and standardized the public-facing section name to "Projects".
- 🔨 [Changed] Rebuilt the article header into a media-rich hero card that combines the cover image, minute-level publish/update timestamps, and compact category/album/tag summaries.
- 🔨 [Changed] Removed the article "reading guide" card and restored a simpler sticky table-of-contents sidebar for reading.
- 🔨 [Changed] Reorganized the header navigation by folding Albums and Categories into the Blog dropdown, with direct links to all articles, albums, categories, and the latest update.
- 🐛 [Fixed] Fixed garbled Chinese text and broken interaction copy on the NuoChe move-car QR generator page.

V1.1.1 (2026-04-29)

- 🔨 [Changed] Refined the global typography stack with dedicated display, text, and code fonts for a more professional technical-blog reading experience.
- 🔨 [Changed] Reworked the site visual language with a lighter technology-inspired grid background, sharper card styling, and more restrained shadows.
- 🔨 [Changed] Compacted the header navigation and moved source-code and issue links to the footer to keep primary menu items horizontal on desktop.
- 🔨 [Changed] Deferred non-critical Font Awesome and analytics loading, and only load jQuery on pages that need validation or legacy scripts to improve initial page performance.
- 🔨 [Changed] Matched the homepage album/category curation panel and latest-articles panel heights for a more balanced first-screen layout.
- 🔨 [Changed] Rebuilt the album and category header dropdowns as two-column browse menus with memo snippets, article counts, and more polished hover targets.
- 🔨 [Changed] Updated album and category detail headers and meta descriptions to read from each item's `Memo` field with safe directory fallbacks.
- 🔨 [Changed] Added the grid treatment to the footer for stronger visual continuity across the whole site.
- 🔨 [Changed] Updated the homepage hero to present two featured articles with single-line title truncation, two-line summary truncation, and hover title details.

V1.1.0 (2026-04-29)

- 😄 [Added] Added `/album` and `/Category` directory pages for browsing all albums and categories.
- 🔨 [Changed] Simplified the homepage into a cleaner visitor-facing layout with less duplicated copy, stats, and navigation.
- 🔨 [Changed] Refined the homepage album and category sections to use curated items with unified "View more" entry points.
- 🔨 [Changed] Updated homepage article cards to clamp titles and summaries while keeping full text available on hover.
- 🔨 [Changed] Upgraded Markdown presentation with more polished table, link, and text-selection styles.
- 🔨 [Changed] Streamlined the footer and friend-link areas, including removal of the non-applicable ICP filing text.
- 🐛 [Fixed] Fallback article authors now use the configured site `Owner` when a post author is empty.

V1.0.0 (2026-04-28)

- 😄 [Added] Added bilingual repository documentation for `CodeWF`.
- 😄 [Added] Added bilingual changelog files for the source repository.
- 🔨 [Changed] Refined the root README with local development, configuration, and content workflow guidance.
- 🔨 [Changed] Added official GitHub repository links for both `CodeWF` and `Assets.Dotnet9`.
- 🔨 [Changed] Moved version management to `Directory.Build.props` to align with standard .NET open source project practices.
- 🐛 [Fixed] Adjusted development startup behavior to avoid local HTTPS redirection warnings.
- 🐛 [Fixed] Clarified environment-variable based configuration guidance for sensitive settings in documentation.
