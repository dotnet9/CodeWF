import Link from "next/link";
import Image from "next/image";
import { api, resolveAssetUrl } from "@/api";
import { PostCard } from "@/components/PostCard";
import { dictionary, formatDate, withLocale } from "@/i18n";
import type { BlogPostBrief, Locale, TaxonomyItem, ToolNode } from "@/types";
import type { LocalePageProps } from "./layout";

export default async function HomePage({ params }: LocalePageProps) {
  const { locale } = await params;
  const home = await api.home(locale);
  const t = dictionary(locale);
  const copy = homeCopy(locale);
  const featuredPosts = home.bannerPosts.slice(0, 3);
  const featuredKeys = new Set(featuredPosts.map((post) => post.slug ?? post.url ?? post.title).filter(Boolean));
  const recentPosts = home.recentPosts
    .filter((post) => !featuredKeys.has(post.slug ?? post.url ?? post.title))
    .slice(0, 6);
  const recommendedTools = collectToolLeaves(home.tools).slice(0, 12);
  const heroImage = resolveAssetUrl(home.site, "site/banners/banner.jpg");
  const spotlightPost = featuredPosts[0] ?? home.recentPosts[0];
  const spotlightCover = spotlightPost ? resolveAssetUrl(home.site, spotlightPost.cover) : undefined;
  const topAlbum = home.albums[0];
  const topCategory = home.categories[0];
  const trustItems = [
    { value: home.counts.posts ?? 0, label: copy.postsMetric, href: withLocale(locale, "/post") },
    { value: home.counts.docs ?? 0, label: copy.projectsMetric, href: withLocale(locale, "/project") },
    { value: home.counts.tools ?? 0, label: copy.toolsMetric, href: withLocale(locale, "/tool") }
  ];
  const startCards = buildStartCards({
    locale,
    copy,
    latestPost: home.recentPosts[0],
    album: topAlbum,
    category: topCategory,
    projectsLabel: t.projects
  });

  return (
    <main className="page-wrap page-stack">
      <section className="home-hero">
        {heroImage ? (
          <Image
            className="home-hero__image"
            src={heroImage}
            alt=""
            fill
            priority
            sizes="100vw"
          />
        ) : null}
        <div className="home-hero__content">
          <span className="eyebrow">{copy.eyebrow}</span>
          <h1>{copy.title}</h1>
          <p className="home-hero__lead">{copy.description}</p>
          <div className="home-hero__actions">
            <Link className="primary-action" href={withLocale(locale, "/post")}>
              {copy.browsePosts}
            </Link>
            <Link className="secondary-action" href={withLocale(locale, "/project")}>
              {copy.openProjects}
            </Link>
            <Link className="secondary-action" href={withLocale(locale, "/s?q=dotnet")}>
              {copy.searchSite}
            </Link>
          </div>
          <div className="home-hero__trustline" aria-label={copy.trustlineLabel}>
            {trustItems.map((item) => (
              <Link href={item.href} key={item.label}>
                <strong>{item.value}</strong>
                <span>{item.label}</span>
              </Link>
            ))}
          </div>
        </div>
        <div className="home-hero__rail" aria-label={copy.spotlightLabel}>
          {spotlightPost ? (
            <Link className="home-spotlight-card" href={withLocale(locale, spotlightPost.url ?? "/post")}>
              {spotlightCover ? (
                <span className="home-spotlight-card__media">
                  <Image src={spotlightCover} alt="" fill sizes="(max-width: 920px) 100vw, 360px" />
                </span>
              ) : null}
              <span className="card-kicker">{copy.spotlight}</span>
              <strong>{spotlightPost.title ?? spotlightPost.slug}</strong>
              {spotlightPost.description ? <p>{spotlightPost.description}</p> : null}
              <small>{formatDate(spotlightPost.lastmod ?? spotlightPost.date, locale)}</small>
            </Link>
          ) : null}
        </div>
      </section>

      <section className="home-start-panel">
        <div className="home-start-grid">
          {startCards.map((item) => (
            <Link className="home-start-card" href={item.href} key={item.href}>
              <span className="card-kicker">{item.kicker}</span>
              <strong>{item.title}</strong>
            </Link>
          ))}
        </div>
      </section>

      <section className="home-section">
        <div className="section-head">
          <div>
            <h2>{t.featuredPosts}</h2>
          </div>
          <Link className="text-link" href={withLocale(locale, "/post")}>
            {t.posts}
          </Link>
        </div>
        <div className="home-featured-layout">
          {featuredPosts.length > 0 ? (
            <>
              <PostCard post={featuredPosts[0]} locale={locale} site={home.site} />
              <div className="home-featured-list">
                {featuredPosts.slice(1).map((post) => (
                  <CompactPostLink post={post} locale={locale} key={`${post.date}-${post.slug}`} />
                ))}
              </div>
            </>
          ) : (
            <div className="empty-state">{t.empty}</div>
          )}
        </div>
      </section>

      <section className="home-section">
        <div className="section-head">
          <div>
            <h2>{t.recentPosts}</h2>
          </div>
          <Link className="text-link" href={withLocale(locale, "/post")}>
            {t.posts}
          </Link>
        </div>
        <div className="home-post-list">
          {recentPosts.length > 0 ? (
            recentPosts.map((post) => <CompactPostLink post={post} locale={locale} key={`${post.date}-${post.slug}`} />)
          ) : (
            <div className="empty-state">{t.empty}</div>
          )}
        </div>
      </section>

      <section className="home-section home-tools-panel">
        <div className="section-head">
          <div>
            <h2>{t.recommendedTools}</h2>
          </div>
          <Link className="text-link" href={withLocale(locale, "/tool")}>
            {t.toolCatalog}
          </Link>
        </div>
        <div className="home-tool-grid">
          {recommendedTools.length > 0 ? (
            recommendedTools.map((tool) => (
              <Link className="home-tool-card" href={withLocale(locale, `/tool/${encodeURIComponent(tool.slug ?? "")}`)} key={tool.slug}>
                <span className="card-kicker">{locale === "zh-CN" ? "工具推荐" : "Tool"}</span>
                <strong>{tool.name}</strong>
                {tool.memo ? <p>{tool.memo}</p> : null}
              </Link>
            ))
          ) : (
            <div className="empty-state empty-state--compact">{t.empty}</div>
          )}
        </div>
      </section>
    </main>
  );
}

function CompactPostLink({ post, locale }: { post: BlogPostBrief; locale: Locale }) {
  return (
    <Link className="compact-post-link" href={withLocale(locale, post.url ?? "/post")}>
      <span>{formatDate(post.lastmod ?? post.date, locale)}</span>
      <strong>{post.title ?? post.slug}</strong>
      {post.description ? <p>{post.description}</p> : null}
    </Link>
  );
}

function collectToolLeaves(nodes: ToolNode[]) {
  const results: ToolNode[] = [];

  const visit = (items: ToolNode[]) => {
    for (const node of items) {
      if (node.children?.length) {
        visit(node.children);
      } else if (node.slug) {
        results.push(node);
      }
    }
  };

  visit(nodes);
  return results;
}

type HomeCopy = ReturnType<typeof homeCopy>;

function buildStartCards({
  locale,
  copy,
  latestPost,
  album,
  category,
  projectsLabel
}: {
  locale: Locale;
  copy: HomeCopy;
  latestPost?: BlogPostBrief;
  album?: TaxonomyItem;
  category?: TaxonomyItem;
  projectsLabel: string;
}) {
  return [
    latestPost
      ? {
          kicker: copy.latestKicker,
          title: latestPost.title ?? latestPost.slug ?? projectsLabel,
          href: withLocale(locale, latestPost.url ?? "/post")
        }
      : null,
    album
      ? {
          kicker: copy.albumKicker,
          title: album.name ?? album.slug ?? projectsLabel,
          href: withLocale(locale, `/album/${encodeURIComponent(album.slug ?? album.name ?? "")}`)
        }
      : null,
    category
      ? {
          kicker: copy.categoryKicker,
          title: category.name ?? category.slug ?? projectsLabel,
          href: withLocale(locale, `/cat/${encodeURIComponent(category.slug ?? category.name ?? "")}`)
        }
      : null,
    {
      kicker: copy.projectKicker,
      title: projectsLabel,
      href: withLocale(locale, "/project")
    }
  ].filter((item): item is Exclude<typeof item, null> => item !== null);
}

function homeCopy(locale: Locale) {
  if (locale === "en") {
    return {
      eyebrow: "Tech blog / engineering notes",
      title: "Record .NET and modern development with engineering judgment",
      description: "Framework deep dives, source-level practice, toolchains, and lessons from real projects, organized as reusable technical decisions.",
      browsePosts: "Browse posts",
      openProjects: "Explore projects",
      searchSite: "Search site",
      trustlineLabel: "Site content metrics",
      postsMetric: "posts",
      projectsMetric: "project entries",
      toolsMetric: "tool entries",
      spotlightLabel: "Featured update",
      spotlight: "Latest update",
      startEyebrow: "Start here",
      startTitle: "A smoother route for first-time visitors",
      startDescription: "Pick the path that matches your intent: read the latest update, follow a topic, filter by category, or jump into projects.",
      latestKicker: "Latest article",
      latestFallback: "Latest post",
      latestDescription: "Start with the newest content.",
      albumKicker: "Theme",
      albumTitle: (name?: string) => `Read ${name ?? "a topic"} as a series`,
      albumDescription: (count: number) => `${count} posts for continuous reading`,
      categoryKicker: "Category",
      categoryTitle: (name?: string) => `Browse ${name ?? "a category"}`,
      categoryDescription: (count: number) => `${count} posts around one technical direction`,
      projectKicker: "Project",
      projectTitle: "Explore open-source projects",
      projectDescription: "Find repositories, packages, and usage notes in one place.",
      projectMeta: "Reusable entries",
      postCount: (count: number) => `${count} posts`
    };
  }

  if (locale === "ja") {
    return {
      eyebrow: "技術ブログ / エンジニアリングノート",
      title: ".NET と現代的な開発をエンジニア視点で記録",
      description: "フレームワーク、ソース実践、ツールチェーン、実案件の経験を、再利用しやすい技術判断として整理します。",
      browsePosts: "記事を見る",
      openProjects: "プロジェクトへ",
      searchSite: "サイト検索",
      trustlineLabel: "サイトの内容規模",
      postsMetric: "記事",
      projectsMetric: "プロジェクト",
      toolsMetric: "ツール",
      spotlightLabel: "注目更新",
      spotlight: "最新更新",
      startEyebrow: "ここから開始",
      startTitle: "初めての訪問者向けルート",
      startDescription: "最新記事、連載、カテゴリ、プロジェクトから目的に合う入口を選べます。",
      latestKicker: "最新文章",
      latestFallback: "最新記事",
      latestDescription: "新しい内容から読み始めます。",
      albumKicker: "主题",
      albumTitle: (name?: string) => `${name ?? "特集"}を連続で読む`,
      albumDescription: (count: number) => `${count} 本の記事を連続して読めます`,
      categoryKicker: "分类",
      categoryTitle: (name?: string) => `${name ?? "カテゴリ"}を見る`,
      categoryDescription: (count: number) => `${count} 本の記事を技術方向で整理`,
      projectKicker: "项目",
      projectTitle: "オープンソースを見る",
      projectDescription: "リポジトリ、パッケージ、利用メモをまとめています。",
      projectMeta: "再利用しやすい入口",
      postCount: (count: number) => `${count} 本`
    };
  }

  if (locale === "zh-TW") {
    return {
      eyebrow: "技術部落格 / 工程筆記",
      title: "用工程視角記錄 .NET 與現代開發",
      description: "深入框架、源碼實踐、工具鏈與真實專案經驗，沉澱可複用的技術判斷。",
      browsePosts: "瀏覽文章",
      openProjects: "進入專案",
      searchSite: "全站搜尋",
      trustlineLabel: "站點內容規模",
      postsMetric: "篇文章",
      projectsMetric: "個專案條目",
      toolsMetric: "個工具入口",
      spotlightLabel: "重點更新",
      spotlight: "最新更新",
      startEyebrow: "從這裡開始",
      startTitle: "新訪客起步路線",
      startDescription: "按你的目的選入口：先看最新更新、跟著專題連讀、按分類篩選，或直接進入專案索引。",
      latestKicker: "最新文章",
      latestFallback: "最新文章",
      latestDescription: "從最近更新的內容開始。",
      albumKicker: "主題",
      albumTitle: (name?: string) => `跟著專題讀 ${name ?? "內容"}`,
      albumDescription: (count: number) => `${count} 篇文章，適合連續閱讀`,
      categoryKicker: "分類",
      categoryTitle: (name?: string) => `先逛 ${name ?? "分類"}`,
      categoryDescription: (count: number) => `${count} 篇文章，快速熟悉內容結構`,
      projectKicker: "專案",
      projectTitle: "看看開源專案",
      projectDescription: "整理開源專案、NuGet 套件和對應使用說明。",
      projectMeta: "便於快速複用",
      postCount: (count: number) => `${count} 篇文章`
    };
  }

  return {
    eyebrow: "技术博客 / 工程笔记",
    title: "用工程视角记录 .NET 与现代开发",
    description: "深入框架、源码实践、工具链与真实项目经验，沉淀可复用的技术判断。",
    browsePosts: "浏览文章",
    openProjects: "进入项目",
    searchSite: "全站搜索",
    trustlineLabel: "站点内容规模",
    postsMetric: "篇文章",
    projectsMetric: "个项目条目",
    toolsMetric: "个工具入口",
    spotlightLabel: "重点更新",
    spotlight: "最新更新",
    startEyebrow: "从这里开始",
    startTitle: "新用户起步路线",
    startDescription: "按你的目的选入口：先看最新更新、跟着专题连读、按分类筛选，或直接进入项目索引。",
    latestKicker: "最新文章",
    latestFallback: "最新文章",
    latestDescription: "从最近更新的内容开始。",
    albumKicker: "主题",
    albumTitle: (name?: string) => `跟着专题读 ${name ?? "内容"}`,
    albumDescription: (count: number) => `${count} 篇文章，适合连续阅读`,
    categoryKicker: "分类",
    categoryTitle: (name?: string) => `先逛 ${name ?? "分类"}`,
    categoryDescription: (count: number) => `${count} 篇文章，快速熟悉内容结构`,
    projectKicker: "项目",
    projectTitle: "看看开源项目",
    projectDescription: "整理开源项目、NuGet 包和对应使用说明。",
    projectMeta: "便于快速复用",
    postCount: (count: number) => `${count} 篇文章`
  };
}
