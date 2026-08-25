import Link from "next/link";
import { api } from "@/api";
import { dictionary, formatDate, withLocale } from "@/i18n";
import type { BlogPostBrief, Locale } from "@/types";

type AlbumPageProps = { params: Promise<{ locale: Locale; slug: string }> };

const chapterPlan = [
  { title: "基础入门", count: 4 },
  { title: "组件与数据绑定", count: 11 },
  { title: "实战：博客系统", count: 5 },
  { title: "认证与授权", count: 8 },
  { title: "测试与收尾", count: 3 }
];

export default async function AlbumPage({ params }: AlbumPageProps) {
  const { locale, slug } = await params;
  const albumSlug = decodeURIComponent(slug);
  const albums = await api.albums(locale);
  const album = albums.find((item) => item.slug?.toLowerCase() === albumSlug.toLowerCase());
  const posts = await api.posts(locale, { pageIndex: 1, pageSize: 100, album: album?.name ?? albumSlug });
  const title = album?.name ?? albumSlug;
  const orderedPosts = posts.data.slice().reverse();
  const chapters = buildChapters(orderedPosts);
  const relatedAlbums = albums.filter((item) => item.slug !== album?.slug).slice(0, 4);
  const t = dictionary(locale);

  return (
    <main className="page-wrap album-detail-page">
      <div className="breadcrumb"><span>~/album</span><span aria-hidden="true">/</span><b>{albumSlug}</b></div>
      <div className="album-detail-layout">
        <div className="album-detail-main">
          <section className="album-head-card">
            <span className="album-head-card__kicker">#01 · {posts.total > 0 ? "已完结" : "准备中"}</span>
            <h1>{title}</h1>
            <p>{album?.memo ?? "围绕同一主题整理的连续阅读文章。"}</p>
            <div className="tag-row"><span>.NET</span><span>Blazor</span><span>实战</span></div>
            <div className="album-head-card__stats">
              <div><strong>{posts.total}</strong><small>EPISODES</small></div>
              <div><strong>{posts.total > 0 ? "100" : "0"}<i>%</i></strong><small>COMPLETED</small></div>
              <div><strong>—</strong><small>TOTAL READS</small></div>
              {orderedPosts[0]?.url ? <Link href={withLocale(locale, orderedPosts[0].url)}>从第 1 篇开始 →</Link> : null}
            </div>
          </section>

          {chapters.map((chapter, chapterIndex) => (
            <section className="album-chapter" key={chapter.title}>
              <div className="album-chapter__title"><b>CH.{String(chapterIndex + 1).padStart(2, "0")}</b><span>{chapter.title}</span><small>{chapter.posts.length} eps</small></div>
              <div className="album-episode-list">
                {chapter.posts.map((post, index) => (
                  <Link className={chapterIndex === 0 && index === 0 ? "album-episode is-start" : "album-episode"} href={withLocale(locale, post.url ?? "/post")} key={post.url ?? post.slug ?? `${chapterIndex}-${index}`}>
                    <span className="album-episode__number">{String(orderedPosts.indexOf(post) + 1).padStart(2, "0")}</span>
                    <span className="album-episode__body"><strong>{post.title ?? post.slug}</strong><small>{formatDate(post.date, locale)} · {post.author ?? "CodeWF"}</small></span>
                    {chapterIndex === 0 && index === 0 ? <span className="album-episode__tag">开始这里</span> : null}
                    <span className="album-episode__arrow" aria-hidden="true">→</span>
                  </Link>
                ))}
              </div>
            </section>
          ))}
          {posts.total === 0 ? <div className="empty-state empty-state--compact">{t.empty}</div> : null}
        </div>

        <aside className="album-detail-aside">
          <section className="album-progress-card">
            <h2><span className="sidebar-dot" />阅读进度</h2>
            <div className="album-progress-card__track"><span /></div>
            <div className="album-progress-card__meta"><span><b>0</b> / {posts.total} 已读</span><span>0%</span></div>
            <div className="album-last-read"><small>LAST READ</small>还没有开始阅读。<Link href={withLocale(locale, orderedPosts[0]?.url ?? "/post")}>从第一篇开始 →</Link></div>
          </section>
          <section className="album-related-card">
            <h2><span className="sidebar-dot sidebar-dot--amber" />相关专题</h2>
            <ul>{relatedAlbums.map((item) => <li key={item.slug}><Link href={withLocale(locale, `/album/${item.slug}`)}>{item.name}</Link><small>{item.postCount} 篇</small></li>)}</ul>
          </section>
          <Link className="album-back-link" href={withLocale(locale, "/album")}>← cd ~/album · 返回全部专题</Link>
        </aside>
      </div>
    </main>
  );
}

function buildChapters(posts: BlogPostBrief[]) {
  const result: { title: string; posts: BlogPostBrief[] }[] = [];
  let cursor = 0;
  for (const plan of chapterPlan) {
    const items = posts.slice(cursor, cursor + plan.count);
    if (items.length > 0) result.push({ title: plan.title, posts: items });
    cursor += plan.count;
  }
  if (cursor < posts.length) result.push({ title: "更多内容", posts: posts.slice(cursor) });
  return result;
}
