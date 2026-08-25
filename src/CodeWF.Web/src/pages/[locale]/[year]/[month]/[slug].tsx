import type { GetServerSideProps, InferGetServerSidePropsType, NextPage } from "next";
import Head from "next/head";
import Image from "next/image";
import Link from "next/link";
import { api, resolveAssetUrl } from "@/api";
import { CodeHighlighter } from "@/components/CodeHighlighter";
import { ArticleActions } from "@/components/ArticleActions";
import { ContentToc, PostPager } from "@/components/ContentToc";
import { HtmlContent } from "@/components/HtmlContent";
import { ReadingProgress } from "@/components/ReadingProgress";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { dictionary, formatDate, normalizeLocale, withLocale } from "@/i18n";
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
          <div className="breadcrumb article-breadcrumb">
            <span>~/blog/{post.categories?.[0] ?? "article"}</span>
            <span aria-hidden="true">/</span>
            <b>{post.slug}</b>
          </div>
          <div className="article-layout">
            <div className="article-main">
              <article className="article-shell" itemScope itemType="https://schema.org/BlogPosting">
                <meta itemProp="mainEntityOfPage" content={canonical} />
                <meta itemProp="url" content={canonical} />
                <meta itemProp="datePublished" content={post.date ?? ""} />
                <meta itemProp="dateModified" content={post.lastmod ?? post.date ?? ""} />
                {cover ? <meta itemProp="image" content={cover} /> : null}
                <header className="article-header">
                  <div className="tag-row article-topic-row">
                    {topicLinks.map((tag) => (
                      <Link href={withLocale(locale, tag.href)} key={`${tag.href}-${tag.label}`}>
                        {tag.label}
                      </Link>
                    ))}
                  </div>
                  <h1 itemProp="headline">{post.title}</h1>
                  <div className="article-head-meta">
                    <div className="article-author">
                      <span className="article-author__avatar" aria-hidden="true">
                        {(post.author ?? site.owner ?? "D9").slice(0, 2).toUpperCase()}
                      </span>
                      <span>
                        <strong itemProp="author">{post.author ?? site.owner}</strong>
                        <small>{site.ownerDesc ?? "CodeWF"}</small>
                      </span>
                    </div>
                    <div className="post-meta article-meta-list">
                      <span>
                        published <time dateTime={post.date} itemProp="datePublished">{formatDate(post.date, locale)}</time>
                      </span>
                      {post.lastmod ? <span>updated <time dateTime={post.lastmod} itemProp="dateModified">{formatDate(post.lastmod, locale)}</time></span> : null}
                      {post.estimatedReadingMinutes ? <span>~{post.estimatedReadingMinutes} min read</span> : null}
                    </div>
                  </div>
                </header>
                {cover ? (
                  <div className="article-cover">
                    <Image src={cover} alt="" fill sizes="(max-width: 920px) 100vw, 820px" priority />
                  </div>
                ) : null}
                <section className="article-body" itemProp="articleBody">
                  <HtmlContent html={post.htmlContent} />
                </section>
                <div className="article-license">© 转载请保留原文链接 · {canonical}</div>
                <ArticleActions />
              </article>
              <PostPager locale={locale} previous={post.previousPost} next={post.nextPost} />
            </div>
            <ContentToc locale={locale} html={post.htmlContent} relatedPosts={post.relatedPosts ?? []} showSupport />
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
