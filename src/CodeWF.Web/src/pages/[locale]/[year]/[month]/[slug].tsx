import type { GetServerSideProps, InferGetServerSidePropsType, NextPage } from "next";
import Head from "next/head";
import Image from "next/image";
import Link from "next/link";
import { api, resolveAssetUrl } from "@/api";
import { CodeHighlighter } from "@/components/CodeHighlighter";
import { ContentToc, PostPager, RelatedPosts } from "@/components/ContentToc";
import { HtmlContent } from "@/components/HtmlContent";
import { ReadingProgress } from "@/components/ReadingProgress";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { dictionary, formatDate, withLocale } from "@/i18n";
import type { BlogPost, FriendLinkItem, HomePageData, SiteInfo } from "@/types";
import type { Locale } from "@/types";

type Params = {
  locale: Locale;
  year: string;
  month: string;
  slug: string;
};

type PageProps = {
  locale: Locale;
  site: SiteInfo;
  friendLinks: FriendLinkItem[];
  post: BlogPost;
  home: HomePageData;
  canonical: string;
  cover?: string;
};

export const getServerSideProps: GetServerSideProps<PageProps, Params> = async ({ params }) => {
  const locale = normalizeLocale(params?.locale);
  const year = params?.year ?? "";
  const month = params?.month ?? "";
  const slug = params?.slug ?? "";
  const [post, home, friendLinks] = await Promise.all([api.post(locale, year, month, slug), api.home(locale), api.friendLinks(locale)]);

  if (!post) {
    return { notFound: true };
  }

  const site = home.site;
  const canonical = new URL(withLocale(locale, post.url ?? `/${year}/${month}/${slug}`), site.domain).toString();
  const cover = resolveAssetUrl(site, post.cover);

  return {
    props: {
      locale,
      site,
      friendLinks,
      post,
      home,
      canonical,
      cover
    }
  };
};

const PostPage: NextPage<InferGetServerSidePropsType<typeof getServerSideProps>> = ({ locale, site, friendLinks, post, home, canonical, cover }) => {
  const t = dictionary(locale);
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
    <>
      <Head>
        <title>{post.title ?? post.slug}</title>
        <meta name="description" content={post.description ?? ""} />
        <link rel="canonical" href={canonical} />
        <meta property="og:type" content="article" />
        <meta property="og:title" content={post.title ?? post.slug ?? ""} />
        <meta property="og:description" content={post.description ?? ""} />
        <meta property="og:url" content={canonical} />
        {cover ? <meta property="og:image" content={cover} /> : null}
        <meta name="twitter:card" content={cover ? "summary_large_image" : "summary"} />
        <meta name="twitter:title" content={post.title ?? post.slug ?? ""} />
        <meta name="twitter:description" content={post.description ?? ""} />
        {cover ? <meta name="twitter:image" content={cover} /> : null}
      </Head>
      <div className="site-shell" data-locale={locale}>
        <SiteHeader locale={locale} site={site} categories={home.categories} albums={home.albums} latestPost={home.recentPosts[0]} />
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
                    {post.estimatedReadingMinutes ? <span>预计阅读 {post.estimatedReadingMinutes} 分钟</span> : null}
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
        <SiteFooter locale={locale} site={site} friendLinks={friendLinks} />
      </div>
    </>
  );
};

export default PostPage;

function normalizeLocale(value?: string): Locale {
  const match = ["zh-CN", "en", "ja", "zh-TW"].find((item) => item.toLowerCase() === value?.toLowerCase());
  return (match as Locale | undefined) ?? "zh-CN";
}
