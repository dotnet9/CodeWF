import { api } from "@/api";
import { Pagination } from "@/components/Pagination";
import { PostCard } from "@/components/PostCard";

type Props = {
  params: Promise<{ locale: "zh-CN" | "en" | "ja" | "zh-TW"; slug: string }>;
  searchParams?: Promise<Record<string, string | string[] | undefined>>;
};

export default async function AlbumPage({ params, searchParams }: Props) {
  const { locale, slug } = await params;
  const search = (await searchParams) ?? {};
  const pageIndex = Number(search.pageIndex ?? "1") || 1;
  const albumSlug = decodeURIComponent(slug);
  const album = (await api.albums(locale)).find((item) => item.slug?.toLowerCase() === albumSlug.toLowerCase());
  const filter = album?.name ?? album?.slug ?? albumSlug;
  const posts = await api.posts(locale, { pageIndex, pageSize: 12, album: filter });
  const site = await api.site();
  const title = album?.name ?? albumSlug;

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{title}</h1>
          <p>{locale === "zh-CN" ? `${posts.total} 篇文章` : `${posts.total} posts`}</p>
        </div>
      </div>
      <div className="post-grid">
        {posts.data.map((post) => (
          <PostCard post={post} locale={locale} site={site} key={`${post.date}-${post.slug}`} />
        ))}
      </div>
      {posts.total === 0 ? <div className="empty-state empty-state--compact">暂无匹配文章。</div> : null}
      <Pagination locale={locale} pageIndex={posts.pageIndex} pageSize={posts.pageSize} total={posts.total} basePath={`/album/${slug}`} />
    </main>
  );
}
