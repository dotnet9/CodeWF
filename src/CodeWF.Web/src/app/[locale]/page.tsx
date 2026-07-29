import Link from "next/link";
import Image from "next/image";
import { api, resolveAssetUrl } from "@/api";
import { PostCard } from "@/components/PostCard";
import { formatDate, withLocale } from "@/i18n";
import type { BlogPostBrief, DocNode, Locale, TaxonomyItem, ToolNode } from "@/types";
import type { LocalePageProps } from "./layout";

const preferredAlbumSlugs = [
  "wpf-ui-design",
  "avalonia-ui-open-source-project",
  "avalonia-ui-control-library",
  "csharp-open-source-projects",
  "from-nurse-to-csharp-developer",
  "learn-blazor-series"
];

const preferredToolSlugs = [
  "json-prettify",
  "timestamp",
  "base64-string-converter",
  "qrcode-generator",
  "url-encoder",
  "jwt-parser",
  "hash-text",
  "slugify-string"
];

export default async function HomePage({ params }: LocalePageProps) {
  const { locale } = await params;
  const [home, docs] = await Promise.all([api.home(locale), api.docs(locale)]);
  const copy = homeCopy();
  const featuredPosts = home.bannerPosts.slice(0, 3);
  const featuredKeys = new Set(featuredPosts.map((post) => post.slug ?? post.url ?? post.title).filter(Boolean));
  const recentPosts = home.recentPosts
    .filter((post) => !featuredKeys.has(post.slug ?? post.url ?? post.title))
    .slice(0, 6);
  const toolLeaves = collectToolLeaves(home.tools);
  const recommendedTools = selectPreferredTools(toolLeaves).slice(0, 8);
  const routes = selectPreferredAlbums(home.albums).slice(0, 6);
  const projects = collectDocLeaves(docs).slice(0, 4);
  const heroImage = resolveAssetUrl(home.site, "site/banners/banner.jpg");
  const spotlightPost = featuredPosts[0] ?? home.recentPosts[0];
  const spotlightCover = spotlightPost ? resolveAssetUrl(home.site, spotlightPost.cover) : undefined;
  const metrics = [
    { value: home.counts.posts ?? 0, label: "篇文章", href: withLocale(locale, "/post") },
    { value: routes.length, label: "条推荐路线", href: withLocale(locale, "/album") },
    { value: home.counts.tools ?? 0, label: "个在线工具", href: withLocale(locale, "/tool") },
    { value: home.counts.docs ?? 0, label: "个开源项目", href: withLocale(locale, "/project") }
  ];
  const entryCards = [
    {
      kicker: "读文章",
      title: "查 .NET 和桌面开发文章",
      description: "常见问题、开源库推荐、项目实践和学习笔记都放在这里。",
      href: withLocale(locale, "/post")
    },
    {
      kicker: "看专题",
      title: "按专题一篇篇看",
      description: "WPF、Avalonia、Blazor、C# 开源项目等内容按主题整理。",
      href: withLocale(locale, "/album")
    },
    {
      kicker: "用工具",
      title: "直接使用在线工具",
      description: "JSON 格式化、时间戳、Base64、二维码、JWT 等常用工具打开就能用。",
      href: withLocale(locale, "/tool")
    },
    {
      kicker: "看项目",
      title: "查看开源项目",
      description: "了解 CodeWF 相关项目、组件库和使用说明。",
      href: withLocale(locale, "/project")
    }
  ];

  return (
    <main className="page-wrap page-stack">
      <section className="home-hero">
        {heroImage ? <Image className="home-hero__image" src={heroImage} alt="" fill priority sizes="100vw" /> : null}
        <div className="home-hero__content">
          <span className="eyebrow">{copy.eyebrow}</span>
          <h1>{copy.title}</h1>
          <p className="home-hero__lead">{copy.description}</p>
          <div className="home-hero__actions">
            <Link className="primary-action" href={withLocale(locale, "/album")}>
              浏览专题
            </Link>
            <Link className="secondary-action" href={withLocale(locale, "/tool")}>
              打开工具
            </Link>
            <Link className="secondary-action" href={withLocale(locale, "/project")}>
              查看项目
            </Link>
          </div>
          <div className="home-hero__trustline" aria-label="站点内容规模">
            {metrics.map((item) => (
              <Link href={item.href} key={item.label}>
                <strong>{item.value}</strong>
                <span>{item.label}</span>
              </Link>
            ))}
          </div>
        </div>
        <div className="home-hero__rail" aria-label="最新推荐">
          {spotlightPost ? (
            <Link className="home-spotlight-card" href={withLocale(locale, spotlightPost.url ?? "/post")}>
              {spotlightCover ? (
                <span className="home-spotlight-card__media">
                  <Image src={spotlightCover} alt="" fill sizes="(max-width: 920px) 100vw, 360px" />
                </span>
              ) : null}
              <span className="card-kicker">最新更新</span>
              <strong>{spotlightPost.title ?? spotlightPost.slug}</strong>
              {spotlightPost.description ? <p>{spotlightPost.description}</p> : null}
              <small>{formatDate(spotlightPost.lastmod ?? spotlightPost.date, locale)}</small>
            </Link>
          ) : null}
        </div>
      </section>

      <section className="home-start-panel" aria-label="站点内容">
        <div className="home-start-grid">
          {entryCards.map((item) => (
            <Link className="home-start-card home-start-card--rich" href={item.href} key={item.href}>
              <span className="card-kicker">{item.kicker}</span>
              <strong>{item.title}</strong>
              <p>{item.description}</p>
            </Link>
          ))}
        </div>
      </section>

      <section className="home-section">
        <div className="section-head">
          <div>
            <h2>推荐阅读路线</h2>
            <p>不知道先看哪篇时，可以从这些专题开始。每个专题都把相关文章放在一起。</p>
          </div>
          <Link className="text-link" href={withLocale(locale, "/album")}>
            全部专题
          </Link>
        </div>
        <div className="home-route-grid">
          {routes.length > 0 ? (
            routes.map((album) => (
              <Link className="home-route-card" href={withLocale(locale, `/album/${encodeURIComponent(album.slug ?? album.name ?? "")}`)} key={album.slug ?? album.name}>
                <span className="card-kicker">专题路线</span>
                <strong>{album.name}</strong>
                {album.memo ? <p>{album.memo}</p> : null}
                <small>{album.postCount} 篇文章</small>
              </Link>
            ))
          ) : (
            <div className="empty-state empty-state--compact">专题内容正在整理中。</div>
          )}
        </div>
      </section>

      <section className="home-section home-tools-panel">
        <div className="section-head">
          <div>
            <h2>常用在线工具</h2>
            <p>临时格式化 JSON、转时间戳、生成二维码，不用单独安装软件。</p>
          </div>
          <Link className="text-link" href={withLocale(locale, "/tool")}>
            全部工具
          </Link>
        </div>
        <div className="home-tool-grid">
          {recommendedTools.length > 0 ? (
            recommendedTools.map((tool) => (
              <Link className="home-tool-card" href={withLocale(locale, `/tool/${encodeURIComponent(tool.slug ?? "")}`)} key={tool.slug}>
                <span className="card-kicker">常用工具</span>
                <strong>{tool.name}</strong>
                {tool.memo ? <p>{tool.memo}</p> : null}
              </Link>
            ))
          ) : (
            <div className="empty-state empty-state--compact">工具内容正在整理中。</div>
          )}
        </div>
      </section>

      <section className="home-section">
        <div className="section-head">
          <div>
            <h2>开源项目</h2>
            <p>这里放一些可以直接查看源码或参考用法的项目和组件。</p>
          </div>
          <Link className="text-link" href={withLocale(locale, "/project")}>
            全部项目
          </Link>
        </div>
        <div className="home-project-grid">
          {projects.length > 0 ? (
            projects.map((project) => (
              <Link className="home-project-card" href={withLocale(locale, `/project/${encodeURIComponent(project.slug ?? "")}`)} key={project.slug}>
                <span className="card-kicker">开源项目</span>
                <strong>{project.name}</strong>
                {project.memo ? <p>{project.memo}</p> : null}
              </Link>
            ))
          ) : (
            <div className="empty-state empty-state--compact">项目内容正在整理中。</div>
          )}
        </div>
      </section>

      <section className="home-section">
        <div className="section-head">
          <div>
            <h2>最新文章</h2>
          </div>
          <Link className="text-link" href={withLocale(locale, "/post")}>
            全部文章
          </Link>
        </div>
        <div className="home-featured-layout">
          {featuredPosts.length > 0 ? (
            <>
              <PostCard post={featuredPosts[0]} locale={locale} site={home.site} />
              <div className="home-featured-list">
                {[...featuredPosts.slice(1), ...recentPosts].slice(0, 5).map((post) => (
                  <CompactPostLink post={post} locale={locale} key={`${post.date}-${post.slug}`} />
                ))}
              </div>
            </>
          ) : recentPosts.length > 0 ? (
            <div className="home-post-list">
              {recentPosts.map((post) => (
                <CompactPostLink post={post} locale={locale} key={`${post.date}-${post.slug}`} />
              ))}
            </div>
          ) : (
            <div className="empty-state">近期更新会在这里展示。</div>
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

function selectPreferredAlbums(albums: TaxonomyItem[]) {
  const selected = preferredAlbumSlugs
    .map((slug) => albums.find((item) => item.slug === slug))
    .filter((item): item is TaxonomyItem => Boolean(item));
  const selectedKeys = new Set(selected.map((item) => item.slug ?? item.name));
  return [
    ...selected,
    ...albums
      .filter((item) => !selectedKeys.has(item.slug ?? item.name))
      .sort((left, right) => right.postCount - left.postCount)
  ];
}

function selectPreferredTools(tools: ToolNode[]) {
  const selected = preferredToolSlugs
    .map((slug) => tools.find((item) => item.slug === slug))
    .filter((item): item is ToolNode => Boolean(item));
  const selectedKeys = new Set(selected.map((item) => item.slug));
  return [...selected, ...tools.filter((tool) => !selectedKeys.has(tool.slug))];
}

function collectToolLeaves(nodes: ToolNode[]) {
  const results: ToolNode[] = [];

  const visit = (items: ToolNode[]) => {
    for (const node of items) {
      if (node.hidden) {
        continue;
      }

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

function collectDocLeaves(nodes: DocNode[]) {
  const results: DocNode[] = [];

  const visit = (items: DocNode[]) => {
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

function homeCopy() {
  return {
    eyebrow: ".NET 技术文章 / 在线工具",
    title: "在码坊，看 .NET 文章、找开源项目、用在线工具",
    description:
      "这里整理了 .NET、桌面开发、Blazor、Avalonia 等内容，也提供 JSON、时间戳、Base64 等常用工具。想学习、查资料或临时处理数据，都可以从这里开始。"
  };
}
