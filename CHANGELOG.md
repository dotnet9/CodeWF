# Changelog (CodeWF)

Chinese version: [CHANGELOG-zh_CN.md](./CHANGELOG-zh_CN.md)

V1.1.0 (2026-04-29)

- 😄 [Added] Added `/bbs/album` and `/Bbs/Category` directory pages for browsing all albums and categories.
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
