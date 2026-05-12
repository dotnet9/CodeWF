import { api } from "@/api";
import { Pagination } from "@/components/Pagination";
import { PostCard } from "@/components/PostCard";
import { dictionary } from "@/i18n";

type Props = {
  params: Promise<{ locale: "zh-CN" | "en" | "ja" | "zh-TW"; tag: string }>;
  searchParams?: Promise<Record<string, string | string[] | undefined>>;
};

export default async function TagPage({ params, searchParams }: Props) {
  const { locale, tag } = await params;
  const search = (await searchParams) ?? {};
  const pageIndex = Number(search.pageIndex ?? "1") || 1;
  const decoded = decodeURIComponent(tag);
  const posts = await api.posts(locale, { pageIndex, pageSize: 12, tag: decoded });
  const site = await api.site();
  const t = dictionary(locale);

  return (
    <main className="page-wrap">
      <div className="section-head">
        <h1>{t.tags}: {decoded}</h1>
      </div>
      <div className="post-grid post-grid--three">
        {posts.data.map((post) => (
          <PostCard post={post} locale={locale} site={site} key={`${post.date}-${post.slug}`} />
        ))}
      </div>
      <Pagination locale={locale} pageIndex={posts.pageIndex} pageSize={posts.pageSize} total={posts.total} basePath={`/tag/${tag}`} />
    </main>
  );
}
