import { api } from "@/api";
import { Pagination } from "@/components/Pagination";
import { PostCard } from "@/components/PostCard";
import { dictionary } from "@/i18n";
import type { LocalePageProps } from "../layout";

export default async function PostsPage({ params, searchParams }: LocalePageProps) {
  const { locale } = await params;
  const search = (await searchParams) ?? {};
  const pageIndex = Number(search.pageIndex ?? "1") || 1;
  const keyword = first(search.keyword);
  const posts = await api.posts(locale, { pageIndex, pageSize: 12, keyword });
  const site = await api.site();
  const t = dictionary(locale);

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{t.posts}</h1>
          <p>{locale === "zh-CN" ? `${posts.total} 篇文章` : `${posts.total} items`}</p>
        </div>
      </div>
      <div className="post-grid post-grid--three">
        {posts.data.length > 0 ? (
          posts.data.map((post) => <PostCard post={post} locale={locale} site={site} key={`${post.date}-${post.slug}`} />)
        ) : (
          <div className="empty-state">{t.empty}</div>
        )}
      </div>
      <Pagination locale={locale} pageIndex={posts.pageIndex} pageSize={posts.pageSize} total={posts.total} basePath="/post" query={{ keyword }} />
    </main>
  );
}

function first(value: string | string[] | undefined) {
  return Array.isArray(value) ? value[0] : value;
}
