import Link from "next/link";
import { api } from "@/api";
import { PostCard } from "@/components/PostCard";
import { dictionary, withLocale } from "@/i18n";
import type { LocalePageProps } from "./layout";

export default async function HomePage({ params }: LocalePageProps) {
  const { locale } = await params;
  const [home, postsPage] = await Promise.all([
    api.home(locale),
    api.posts(locale, { pageIndex: 1, pageSize: 24 })
  ]);
  const t = dictionary(locale);
  const topTools = home.tools.slice(0, 4);
  const recentSlugs = new Set(home.recentPosts.map((post) => post.slug ?? post.url ?? ""));
  const randomPosts = postsPage.data
    .filter((post) => !recentSlugs.has(post.slug ?? post.url ?? ""))
    .slice(0, 6)
    .sort(() => Math.random() - 0.5)
    .slice(0, 3);
  const gettingStartedLinks = buildGettingStartedLinks(home, t);
  const serialAlbums = home.albums.filter((album) => album.postCount > 0).slice(0, 4);
  const featuredPosts = home.bannerPosts.slice(0, 3);

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

        <div className="home-hero__rail">
          <span className="eyebrow eyebrow--subtle">{t.featuredPosts}</span>
          {featuredPosts.map((post) => (
            <Link className="home-feature" href={withLocale(locale, post.url ?? "/post")} key={`${post.date}-${post.slug}`}>
              <span className="card-kicker">{post.contextLabel ?? "精选"}</span>
              <strong>{post.title}</strong>
              <p>{post.description}</p>
            </Link>
          ))}
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
            {home.recentPosts.map((post) => (
              <PostCard post={post} locale={locale} site={home.site} key={`${post.date}-${post.slug}`} />
            ))}
          </div>
        </section>

        <aside className="side-stack">
          <section className="taxonomy-panel">
            <h2>新用户起步路线</h2>
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
            <h2>连续阅读</h2>
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
            <h2>随机发现</h2>
            <div className="discovery-post-list">
              {randomPosts.map((post) => (
                <article className="discovery-post-card" key={`${post.date}-${post.slug}`}>
                  <span className="card-kicker">{post.categories?.[0] ?? "随机发现"}</span>
                  <h3>
                    <Link href={withLocale(locale, post.url ?? "/post")}>{post.title}</Link>
                  </h3>
                  <p>{post.description}</p>
                </article>
              ))}
            </div>
          </section>

          <section className="taxonomy-panel">
            <h2>{t.categories}</h2>
            <div className="taxonomy-list">
              {home.categories.slice(0, 12).map((item) => (
                <Link href={withLocale(locale, `/cat/${encodeURIComponent(item.slug ?? item.name ?? "")}`)} key={item.slug ?? item.name}>
                  <span>{item.name}</span>
                  <span>{item.postCount}</span>
                </Link>
              ))}
            </div>
          </section>

          <section className="list-panel">
            <h2>{t.toolCatalog}</h2>
            <div className="tool-links">
              {topTools.map((group) => (
                <Link href={withLocale(locale, `/tool#${group.slug}`)} key={group.slug}>
                  {group.name}
                </Link>
              ))}
            </div>
          </section>
        </aside>
      </div>
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
      eyebrow: "先看更新",
      title: "从最新文章进入",
      description: latestPost.title ?? t.posts,
      href: latestPost.url ?? "/post"
    });
  }

  if (topCategory) {
    links.push({
      eyebrow: "按主题看",
      title: topCategory.name ?? t.categories,
      description: `${topCategory.postCount} 篇文章`,
      href: `/cat/${encodeURIComponent(topCategory.slug ?? topCategory.name ?? "")}`
    });
  }

  if (topAlbum) {
    links.push({
      eyebrow: "连续阅读",
      title: topAlbum.name ?? t.albums,
      description: `${topAlbum.postCount} 篇文章`,
      href: `/album/${encodeURIComponent(topAlbum.slug ?? topAlbum.name ?? "")}`
    });
  }

  return links;
}
