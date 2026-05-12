import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { api, resolveAssetUrl } from "@/api";
import { ContentToc, PostPager, RelatedPosts } from "@/components/ContentToc";
import { HtmlContent } from "@/components/HtmlContent";
import { ReadingProgress } from "@/components/ReadingProgress";
import { formatDate, withLocale } from "@/i18n";

type Props = {
  params: Promise<{ locale: "zh-CN" | "en" | "ja" | "zh-TW"; year: string; month: string; slug: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale, year, month, slug } = await params;
  const post = await api.post(locale, year, month, slug);
  return {
    title: post?.title ?? slug,
    description: post?.description
  };
}

export default async function PostDetailPage({ params }: Props) {
  const { locale, year, month, slug } = await params;
  const [post, site] = await Promise.all([api.post(locale, year, month, slug), api.site()]);
  if (!post) {
    notFound();
  }

  const cover = resolveAssetUrl(site, post.cover);
  const categoryLinks = (post.categories ?? []).map((item) => ({ label: item, href: `/cat/${encodeURIComponent(item)}` }));
  const albumLinks = (post.albums ?? []).map((item) => ({ label: item, href: `/album/${encodeURIComponent(item)}` }));
  const tagLinks = (post.tags ?? []).map((item) => ({ label: item, href: `/tag/${encodeURIComponent(item)}` }));
  const topicLinks = [...categoryLinks, ...albumLinks, ...tagLinks].slice(0, 12);

  return (
    <main className="page-wrap">
      <ReadingProgress />
      <div className="article-layout">
        <div className="article-main">
          <article className="article-shell">
            {cover ? (
              <div className="article-cover">
                <Image src={cover} alt="" fill sizes="(max-width: 920px) 100vw, 820px" priority />
              </div>
            ) : null}
            <header className="article-header">
              <div className="post-meta">
                <time dateTime={post.date}>{formatDate(post.date, locale)}</time>
                {post.author ? <span>{post.author}</span> : null}
                {post.estimatedReadingMinutes ? <span>预计阅读 {post.estimatedReadingMinutes} 分钟</span> : null}
              </div>
              <h1>{post.title}</h1>
              <p>{post.description}</p>
              <div className="tag-row">
                {topicLinks.map((tag) => (
                  <Link href={withLocale(locale, tag.href)} key={`${tag.href}-${tag.label}`}>
                    {tag.label}
                  </Link>
                ))}
              </div>
            </header>
            <HtmlContent html={post.htmlContent} />
          </article>
          <RelatedPosts locale={locale} posts={post.relatedPosts ?? []} />
          <PostPager locale={locale} previous={post.previousPost} next={post.nextPost} />
        </div>
        <ContentToc html={post.htmlContent} />
      </div>
    </main>
  );
}
