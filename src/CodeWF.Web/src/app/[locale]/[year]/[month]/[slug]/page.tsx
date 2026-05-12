import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { api, resolveAssetUrl } from "@/api";
import { CodeHighlighter } from "@/components/CodeHighlighter";
import { ContentToc, PostPager, RelatedPosts } from "@/components/ContentToc";
import { HtmlContent } from "@/components/HtmlContent";
import { ReadingProgress } from "@/components/ReadingProgress";
import { formatDate, withLocale } from "@/i18n";

type Props = {
  params: Promise<{ locale: "zh-CN" | "en" | "ja" | "zh-TW"; year: string; month: string; slug: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale, year, month, slug } = await params;
  const [post, site] = await Promise.all([api.post(locale, year, month, slug), api.site()]);
  const cover = resolveAssetUrl(site, post?.cover);
  const canonical = new URL(withLocale(locale, post?.url ?? `/${year}/${month}/${slug}`), site.domain).toString();

  return {
    title: post?.title ?? slug,
    description: post?.description,
    alternates: { canonical },
    openGraph: {
      type: "article",
      title: post?.title ?? slug,
      description: post?.description,
      url: canonical,
      images: cover ? [{ url: cover, alt: post?.title ?? slug }] : undefined
    },
    twitter: {
      card: cover ? "summary_large_image" : "summary",
      title: post?.title ?? slug,
      description: post?.description,
      images: cover ? [cover] : undefined
    }
  };
}

export default async function PostDetailPage({ params }: Props) {
  const { locale, year, month, slug } = await params;
  const [post, site] = await Promise.all([api.post(locale, year, month, slug), api.site()]);
  if (!post) {
    notFound();
  }

  const cover = resolveAssetUrl(site, post.cover);
  const canonical = new URL(withLocale(locale, post.url ?? `/${year}/${month}/${slug}`), site.domain).toString();
  const categoryLinks = (post.categories ?? []).map((item) => ({ label: item, href: `/cat/${encodeURIComponent(item)}` }));
  const albumLinks = (post.albums ?? []).map((item) => ({ label: item, href: `/album/${encodeURIComponent(item)}` }));
  const tagLinks = (post.tags ?? []).map((item) => ({ label: item, href: `/tag/${encodeURIComponent(item)}` }));
  const topicLinks = [...categoryLinks, ...albumLinks, ...tagLinks].slice(0, 12);
  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "BlogPosting",
    headline: post.title,
    description: post.description,
    datePublished: post.date,
    dateModified: post.lastmod ?? post.date,
    author: post.author ? { "@type": "Person", name: post.author } : undefined,
    image: cover ? [cover] : undefined,
    mainEntityOfPage: canonical,
    url: canonical,
    publisher: {
      "@type": "Organization",
      name: site.appTitle,
      url: site.domain
    }
  };

  return (
    <main className="page-wrap">
      <ReadingProgress />
      <div className="article-layout">
        <div className="article-main">
          <article className="article-shell" itemScope itemType="https://schema.org/BlogPosting">
            <meta itemProp="mainEntityOfPage" content={canonical} />
            <meta itemProp="url" content={canonical} />
            <meta itemProp="datePublished" content={post.date ?? ""} />
            <meta itemProp="dateModified" content={post.lastmod ?? post.date ?? ""} />
            {cover ? <meta itemProp="image" content={cover} /> : null}
            {cover ? (
              <div className="article-cover">
                <Image src={cover} alt="" fill sizes="(max-width: 920px) 100vw, 820px" priority />
              </div>
            ) : null}
            <header className="article-header">
              <div className="post-meta">
                <time dateTime={post.date} itemProp="datePublished">
                  {formatDate(post.date, locale)}
                </time>
                {post.author ? <span itemProp="author">{post.author}</span> : null}
                {post.estimatedReadingMinutes ? <span>棰勮闃呰 {post.estimatedReadingMinutes} 鍒嗛挓</span> : null}
              </div>
              <h1 itemProp="headline">{post.title}</h1>
              <p itemProp="description">{post.description}</p>
              <div className="tag-row">
                {topicLinks.map((tag) => (
                  <Link href={withLocale(locale, tag.href)} key={`${tag.href}-${tag.label}`}>
                    {tag.label}
                  </Link>
                ))}
              </div>
            </header>
            <section className="article-body" itemProp="articleBody">
              <HtmlContent html={post.htmlContent} />
            </section>
          </article>
          <RelatedPosts locale={locale} posts={post.relatedPosts ?? []} />
          <PostPager locale={locale} previous={post.previousPost} next={post.nextPost} />
        </div>
        <ContentToc html={post.htmlContent} />
      </div>
      <CodeHighlighter />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }} />
    </main>
  );
}
