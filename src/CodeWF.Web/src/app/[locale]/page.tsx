import Link from "next/link";
import type { CSSProperties } from "react";
import { api, resolveAssetUrl } from "@/api";
import { PostCard } from "@/components/PostCard";
import { dictionary, withLocale } from "@/i18n";
import type { ToolNode } from "@/types";
import type { LocalePageProps } from "./layout";

export default async function HomePage({ params }: LocalePageProps) {
  const { locale } = await params;
  const home = await api.home(locale);
  const t = dictionary(locale);
  const featuredPosts = home.bannerPosts.slice(0, 3);
  const featuredKeys = new Set(featuredPosts.map((post) => post.slug ?? post.url ?? post.title).filter(Boolean));
  const recentPosts = home.recentPosts
    .filter((post) => !featuredKeys.has(post.slug ?? post.url ?? post.title))
    .slice(0, 3);
  const recommendedTools = shuffle(collectToolLeaves(home.tools)).slice(0, 12);
  const heroImage = resolveAssetUrl(home.site, "site/banners/banner.jpg");
  const heroStyle = heroImage ? ({ "--hero-image": `url("${heroImage}")` } as CSSProperties) : undefined;
  const metrics = [
    { value: home.counts.posts ?? 0, label: t.posts },
    { value: home.counts.tools ?? 0, label: t.tools },
    { value: home.counts.docs ?? 0, label: t.projects },
    { value: home.counts.categories ?? 0, label: t.categories }
  ];

  return (
    <main className="page-wrap page-stack">
      <section className="home-hero" style={heroStyle}>
        <div className="home-hero__content">
          <span className="eyebrow">CodeWF</span>
          <h1>{home.site.appTitle}</h1>
          <p>{home.site.memo}</p>
          <div className="home-hero__actions">
            <Link className="primary-action" href={withLocale(locale, "/post")}>
              {t.posts}
            </Link>
            <Link className="secondary-action" href={withLocale(locale, "/tool")}>
              {t.tools}
            </Link>
            <Link className="secondary-action" href={withLocale(locale, "/project")}>
              {t.projects}
            </Link>
          </div>
        </div>
        <div className="home-hero__rail" aria-label="C# example">
          <div className="home-code-card">
            <div className="home-code-card__bar">
              <span>C#</span>
              <small>.NET / Markdown / Tools</small>
            </div>
            <pre>
              <code>{`var home = await CodeWF
    .LoadAsync(locale);

var posts = home.Posts
    .Published()
    .Latest(3);

return home.WithTools(12);`}</code>
            </pre>
          </div>
        </div>
      </section>

      <section className="metric-row" aria-label="Site metrics">
        {metrics.map((item) => (
          <div className="metric" key={item.label}>
            <strong>{item.value}</strong>
            <span>{item.label}</span>
          </div>
        ))}
      </section>

      <section className="home-section">
        <div className="section-head">
          <div>
            <h2>{t.featuredPosts}</h2>
            <p>{locale === "zh-CN" ? "首页置顶的 Banner 文章" : "Featured banner posts"}</p>
          </div>
          <Link className="text-link" href={withLocale(locale, "/post")}>
            {t.posts}
          </Link>
        </div>
        <div className="post-grid post-grid--three">
          {featuredPosts.length > 0 ? (
            featuredPosts.map((post) => <PostCard post={post} locale={locale} site={home.site} key={`${post.date}-${post.slug}`} />)
          ) : (
            <div className="empty-state">{t.empty}</div>
          )}
        </div>
      </section>

      <section className="home-section">
        <div className="section-head">
          <div>
            <h2>{t.recentPosts}</h2>
            <p>{home.site.ownerDesc ?? home.site.memo}</p>
          </div>
          <Link className="text-link" href={withLocale(locale, "/post")}>
            {t.posts}
          </Link>
        </div>
        <div className="post-grid post-grid--three">
          {recentPosts.length > 0 ? (
            recentPosts.map((post) => <PostCard post={post} locale={locale} site={home.site} key={`${post.date}-${post.slug}`} />)
          ) : (
            <div className="empty-state">{t.empty}</div>
          )}
        </div>
      </section>

      <section className="home-section home-tools-panel">
        <div className="section-head">
          <div>
            <h2>{t.recommendedTools}</h2>
            <p>{locale === "zh-CN" ? "随机展示 12 个常用工具" : "Twelve random tools from the catalog"}</p>
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

function shuffle<T>(items: T[]) {
  const output = [...items];
  for (let index = output.length - 1; index > 0; index -= 1) {
    const swapIndex = Math.floor(Math.random() * (index + 1));
    [output[index], output[swapIndex]] = [output[swapIndex], output[index]];
  }
  return output;
}
