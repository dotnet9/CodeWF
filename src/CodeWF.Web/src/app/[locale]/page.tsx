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
  const recommendedTools = collectToolLeaves(home.tools).slice(0, 20);
  const gettingStartedLinks = buildGettingStartedLinks(home, t);
  const serialAlbums = home.albums.filter((album) => album.postCount > 0).slice(0, 4);

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

        {featuredPosts.length > 0 ? (
          <div className="home-hero__rail">
            <span className="eyebrow eyebrow--subtle">{t.featuredPosts}</span>
            {featuredPosts.map((post) => (
              <Link className="home-feature" href={withLocale(locale, post.url ?? "/post")} key={`${post.date}-${post.slug}`}>
                <span className="card-kicker">{post.contextLabel ?? (locale === "zh-CN" ? "Banner 文章" : "Featured post")}</span>
                <strong>{post.title}</strong>
                <p>{post.description}</p>
              </Link>
            ))}
          </div>
        ) : null}
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

      <div className="home-grid">
        <section className="home-main">
          <div className="section-head">
            <div>
              <h1>{t.recentPosts}</h1>
              <p>{home.site.ownerDesc ?? home.site.memo}</p>
            </div>
            <Link className="text-link" href={withLocale(locale, "/post")}>
              {t.posts}
            </Link>
          </div>
          <div className="post-grid">
            {recentPosts.map((post) => (
              <PostCard post={post} locale={locale} site={home.site} key={`${post.date}-${post.slug}`} />
            ))}
          </div>
        </section>

        <aside className="side-stack">
          <section className="taxonomy-panel">
            <h2>{locale === "zh-CN" ? "新手起步" : "Getting started"}</h2>
            <div className="discovery-link-list">
              {gettingStartedLinks.map((link) => (
                <Link href={withLocale(locale, link.href)} className="discovery-link-card" key={link.href}>
                  <span className="card-kicker">{link.eyebrow}</span>
                  <strong>{link.title}</strong>
                  <p>{link.description}</p>
                </Link>
              ))}
            </div>
          </section>

          <section className="taxonomy-panel">
            <h2>{locale === "zh-CN" ? "连续阅读" : "Read next"}</h2>
            <div className="sidebar-link-list">
              {serialAlbums.map((album) => (
                <Link href={withLocale(locale, `/album/${encodeURIComponent(album.slug ?? album.name ?? "")}`)} className="sidebar-link" key={album.slug ?? album.name}>
                  <span>{album.name}</span>
                  <span>{album.postCount}</span>
                </Link>
              ))}
            </div>
          </section>

          <section className="taxonomy-panel">
            <h2>{locale === "zh-CN" ? "分类导航" : t.categories}</h2>
            <div className="taxonomy-list">
              {home.categories.slice(0, 12).map((item) => (
                <Link href={withLocale(locale, `/cat/${encodeURIComponent(item.slug ?? item.name ?? "")}`)} key={item.slug ?? item.name}>
                  <span>{item.name}</span>
                  <span>{item.postCount}</span>
                </Link>
              ))}
            </div>
          </section>
        </aside>
      </div>

      {recommendedTools.length > 0 ? (
        <section className="taxonomy-panel home-tools-panel">
          <div className="section-head">
            <div>
              <h2>{t.recommendedTools}</h2>
              <p>{locale === "zh-CN" ? "首页展示 20 个常用工具入口" : "Twenty recommended tools from the catalog"}</p>
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
      ) : null}
    </main>
  );
}

function buildGettingStartedLinks(home: Awaited<ReturnType<typeof api.home>>, t: ReturnType<typeof dictionary>) {
  const links: Array<{
    eyebrow: string;
    title: string;
    description: string;
    href: string;
  }> = [];

  const latestPost = home.recentPosts[0];
  const topCategory = home.categories
    .filter((item) => item.postCount > 0)
    .slice()
    .sort((left, right) => right.postCount - left.postCount)[0];
  const topAlbum = home.albums
    .filter((item) => item.postCount > 0)
    .slice()
    .sort((left, right) => right.postCount - left.postCount)[0];

  if (latestPost) {
    links.push({
      eyebrow: "Latest",
      title: latestPost.title ?? t.posts,
      description: latestPost.description ?? "",
      href: latestPost.url ?? "/post"
    });
  }

  if (topCategory) {
    links.push({
      eyebrow: "Category",
      title: topCategory.name ?? t.categories,
      description: `${topCategory.postCount} posts`,
      href: `/cat/${encodeURIComponent(topCategory.slug ?? topCategory.name ?? "")}`
    });
  }

  if (topAlbum) {
    links.push({
      eyebrow: "Album",
      title: topAlbum.name ?? t.albums,
      description: `${topAlbum.postCount} posts`,
      href: `/album/${encodeURIComponent(topAlbum.slug ?? topAlbum.name ?? "")}`
    });
  }

  return links;
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
