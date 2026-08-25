import Link from "next/link";
import { formatDate, withLocale } from "@/i18n";
import type { BlogPostBrief, DocNode, Locale } from "@/types";

type TocItem = {
  level: number;
  id: string;
  text: string;
};

export function extractToc(html?: string): TocItem[] {
  if (!html) {
    return [];
  }

  const result: TocItem[] = [];
  const headingRegex = /<h([2-4])\s+[^>]*id=["']([^"']+)["'][^>]*>(.*?)<\/h\1>/gis;
  for (const match of html.matchAll(headingRegex)) {
    result.push({
      level: Number(match[1]),
      id: match[2],
      text: decodeText(stripTags(match[3])).trim()
    });
  }
  return result.filter((item) => item.text);
}

type ContentTocProps = {
  locale?: Locale;
  html?: string;
  title?: string;
  relatedPosts?: BlogPostBrief[];
  showSupport?: boolean;
};

export function ContentToc({ locale = "zh-CN", html, title = "目录", relatedPosts = [], showSupport = false }: ContentTocProps) {
  const toc = extractToc(html);
  if (toc.length === 0 && relatedPosts.length === 0 && !showSupport) {
    return null;
  }

  return (
    <aside className="article-aside">
      {toc.length > 0 ? (
        <div className="toc-panel article-aside__panel">
          <h2>{title}</h2>
          <nav aria-label="Table of contents">
            {toc.map((item, index) => (
              <a
                href={`#${item.id}`}
                className={`${`toc-level-${item.level}`} ${index === 0 ? "is-active" : ""}`.trim()}
                aria-current={index === 0 ? "location" : undefined}
                key={`${item.id}-${index}`}
              >
                {item.text}
              </a>
            ))}
          </nav>
        </div>
      ) : null}
      {relatedPosts.length > 0 ? (
        <section className="article-related-panel article-aside__panel" aria-labelledby="article-related-title">
          <h2 id="article-related-title">相关阅读</h2>
          <ul className="article-related-list">
            {relatedPosts.slice(0, 5).map((post) => (
              <li key={post.url ?? post.slug ?? post.title}>
                <Link href={withLocale(locale, post.url ?? "/post")}>{post.title}</Link>
                {post.contextLabel ? <small>{post.contextLabel}</small> : null}
              </li>
            ))}
          </ul>
        </section>
      ) : null}
      {showSupport ? (
        <section className="article-support-panel article-aside__panel" aria-labelledby="article-support-title">
          <h2 id="article-support-title">支持站长</h2>
          <p>如果文章对你有帮助，欢迎赞助一杯咖啡。</p>
          <Link href={withLocale(locale, "/donation")}>前往捐赠</Link>
        </section>
      ) : null}
    </aside>
  );
}

export function PostPager({ locale, previous, next }: { locale: Locale; previous?: BlogPostBrief; next?: BlogPostBrief }) {
  if (!previous && !next) {
    return null;
  }

  return (
    <nav className="content-pager" aria-label="Post navigation">
      {previous ? (
        <Link href={withLocale(locale, previous.url ?? "/post")} className="content-pager__item">
          <span>上一篇</span>
          <strong>{previous.title}</strong>
          <small>{formatDate(previous.date, locale)}</small>
        </Link>
      ) : (
        <span />
      )}
      {next ? (
        <Link href={withLocale(locale, next.url ?? "/post")} className="content-pager__item content-pager__item--next">
          <span>下一篇</span>
          <strong>{next.title}</strong>
          <small>{formatDate(next.date, locale)}</small>
        </Link>
      ) : (
        <span />
      )}
    </nav>
  );
}

export function RelatedPosts({ locale, posts }: { locale: Locale; posts: BlogPostBrief[] }) {
  if (posts.length === 0) {
    return null;
  }

  return (
    <section className="related-panel">
      <div className="section-head">
        <div>
          <h2>延伸阅读</h2>
          <p>继续探索相近主题</p>
        </div>
      </div>
      <div className="related-grid">
        {posts.map((post) => (
          <article className="related-card" key={post.url ?? post.slug}>
            <span>{post.contextLabel ?? "推荐"}</span>
            <h3>
              <Link href={withLocale(locale, post.url ?? "/post")}>{post.title}</Link>
            </h3>
            <p>{post.description}</p>
          </article>
        ))}
      </div>
    </section>
  );
}

export function DocPager({ locale, previous, next }: { locale: Locale; previous?: DocNode; next?: DocNode }) {
  if (!previous && !next) {
    return null;
  }

  return (
    <nav className="content-pager" aria-label="Project navigation">
      {previous ? (
        <Link href={withLocale(locale, `/project/${previous.slug}`)} className="content-pager__item">
          <span>上一篇</span>
          <strong>{previous.name}</strong>
          <small>{previous.memo}</small>
        </Link>
      ) : (
        <span />
      )}
      {next ? (
        <Link href={withLocale(locale, `/project/${next.slug}`)} className="content-pager__item content-pager__item--next">
          <span>下一篇</span>
          <strong>{next.name}</strong>
          <small>{next.memo}</small>
        </Link>
      ) : (
        <span />
      )}
    </nav>
  );
}

function stripTags(value: string) {
  return value.replace(/<[^>]+>/g, "");
}

function decodeText(value: string) {
  return value
    .replace(/&amp;/g, "&")
    .replace(/&lt;/g, "<")
    .replace(/&gt;/g, ">")
    .replace(/&quot;/g, "\"")
    .replace(/&#39;/g, "'");
}
