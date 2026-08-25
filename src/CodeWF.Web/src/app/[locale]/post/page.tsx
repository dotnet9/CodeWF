import Link from "next/link";
import { api } from "@/api";
import { Pagination } from "@/components/Pagination";
import { PostCard } from "@/components/PostCard";
import { dictionary, withLocale } from "@/i18n";
import type { LocalePageProps } from "../layout";

export default async function PostsPage({ params, searchParams }: LocalePageProps) {
  const { locale } = await params;
  const search = (await searchParams) ?? {};
  const pageIndex = Number(search.pageIndex ?? "1") || 1;
  const keyword = first(search.keyword);
  const [posts, site, categories, tags] = await Promise.all([
    api.posts(locale, { pageIndex, pageSize: 12, keyword }),
    api.site(),
    api.categories(locale),
    api.tags(locale)
  ]);
  const t = dictionary(locale);
  const archiveYears = [new Date().getFullYear(), new Date().getFullYear() - 1, new Date().getFullYear() - 2, new Date().getFullYear() - 3, new Date().getFullYear() - 4];

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{t.posts}</h1>
          <p>{locale === "zh-CN" ? `${posts.total} 篇文章` : `${posts.total} items`}</p>
        </div>
      </div>
      <div className="posts-page-layout">
        <div className="posts-page-main">
          <div className="posts-filter-row" aria-label="文章分类">
            <Link className={!keyword ? "is-active" : ""} href={withLocale(locale, "/post")}>all</Link>
            {categories.slice(0, 8).map((item) => (
              <Link href={withLocale(locale, `/cat/${encodeURIComponent(item.slug ?? item.name ?? "")}`)} key={item.slug ?? item.name}>
                {item.name}
              </Link>
            ))}
          </div>
          <div className="post-grid post-grid--three">
            {posts.data.length > 0 ? (
              posts.data.map((post) => <PostCard post={post} locale={locale} site={site} key={`${post.date}-${post.slug}`} />)
            ) : (
              <div className="empty-state">{t.empty}</div>
            )}
          </div>
          <Pagination locale={locale} pageIndex={posts.pageIndex} pageSize={posts.pageSize} total={posts.total} basePath="/post" query={{ keyword }} />
        </div>
        <aside className="posts-page-aside">
          <section className="posts-sidebar-card">
            <h2><span className="sidebar-dot" />categories</h2>
            <div className="posts-category-list">
              {categories.slice(0, 8).map((item) => (
                <Link href={withLocale(locale, `/cat/${encodeURIComponent(item.slug ?? item.name ?? "")}`)} key={item.slug ?? item.name}>
                  <span>{item.name}</span><small>{item.postCount}</small>
                </Link>
              ))}
            </div>
          </section>
          <section className="posts-sidebar-card">
            <h2><span className="sidebar-dot sidebar-dot--amber" />tags</h2>
            <div className="posts-tag-cloud">
              {tags.slice(0, 18).map((item) => <Link href={withLocale(locale, `/tag/${encodeURIComponent(item.name)}`)} key={item.name}>{item.name}</Link>)}
            </div>
          </section>
          <section className="posts-sidebar-card">
            <h2><span className="sidebar-dot" />archive</h2>
            <div className="posts-category-list">
              {archiveYears.map((year) => <Link href={`${withLocale(locale, "/post")}?year=${year}`} key={year}><span>{year}</span><small>›</small></Link>)}
            </div>
          </section>
        </aside>
      </div>
    </main>
  );
}

function first(value: string | string[] | undefined) {
  return Array.isArray(value) ? value[0] : value;
}
