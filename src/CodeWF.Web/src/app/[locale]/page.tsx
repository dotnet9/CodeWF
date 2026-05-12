import Link from "next/link";
import { api } from "@/api";
import { PostCard } from "@/components/PostCard";
import { dictionary, withLocale } from "@/i18n";
import type { ToolNode } from "@/types";
import type { LocalePageProps } from "./layout";

export default async function HomePage({ params }: LocalePageProps) {
  const { locale } = await params;
  const home = await api.home(locale);
  const t = dictionary(locale);
  const featuredPosts = home.bannerPosts.slice(0, 3);
  const recentPosts = home.recentPosts.slice(0, 3);
  const recommendedTools = shuffle(collectToolLeaves(home.tools)).slice(0, 12);

  return (
    <main className="page-wrap page-stack">
      <section className="home-hero surface-panel">
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
      </section>

      <section className="metric-row" aria-label="Site metrics">
        {[
          [home.counts.posts ?? 0, t.posts],
          [home.counts.tools ?? 0, t.tools],
          [home.counts.docs ?? 0, t.projects],
          [home.counts.categories ?? 0, t.categories]
        ].map(([value, label]) => (
          <div className="metric" key={label}>
            <strong>{value}</strong>
            <span>{label}</span>
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
          {featuredPosts.map((post) => (
            <PostCard post={post} locale={locale} site={home.site} key={`${post.date}-${post.slug}`} />
          ))}
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
          {recentPosts.map((post) => (
            <PostCard post={post} locale={locale} site={home.site} key={`${post.date}-${post.slug}`} />
          ))}
        </div>
      </section>

      <section className="home-section taxonomy-panel home-tools-panel">
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
          {recommendedTools.map((tool) => (
            <Link className="home-tool-card" href={withLocale(locale, `/tool/${encodeURIComponent(tool.slug ?? "")}`)} key={tool.slug}>
              <span className="card-kicker">{locale === "zh-CN" ? "工具推荐" : "Tool"}</span>
              <strong>{tool.name}</strong>
              {tool.memo ? <p>{tool.memo}</p> : null}
            </Link>
          ))}
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
