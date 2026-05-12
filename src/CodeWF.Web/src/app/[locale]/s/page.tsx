import Link from "next/link";
import { api } from "@/api";
import { Pagination } from "@/components/Pagination";
import { dictionary, formatDate, withLocale } from "@/i18n";
import type { LocalePageProps } from "../layout";

export default async function SearchPage({ params, searchParams }: LocalePageProps) {
  const { locale } = await params;
  const search = (await searchParams) ?? {};
  const q = first(search.q);
  const pageIndex = Number(search.pageIndex ?? "1") || 1;
  const results = await api.search(locale, { q, pageIndex, pageSize: 10 });
  const t = dictionary(locale);

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{t.search}</h1>
          <p>{q ? `${results.total} ${locale === "zh-CN" ? "条结果" : "results"}` : t.queryPlaceholder}</p>
        </div>
      </div>
      <section className="search-summary-panel">
        <div className="search-summary-row">
          <div className="page-banner__meta">
            {q ? <span className="meta-chip">关键字：{q}</span> : null}
            <span className="meta-chip">共 {results.total} 条结果</span>
          </div>
          <div className="page-banner__meta">
            <span className="meta-chip">工具 {results.toolCount}</span>
            <span className="meta-chip">项目 {results.docCount}</span>
            <span className="meta-chip">文章 {results.postCount}</span>
          </div>
        </div>
        {results.isBlocked ? (
          <div className="empty-state empty-state--compact">
            <h2>换一个技术关键词试试</h2>
            <p>{results.notice ?? "当前关键词暂不支持搜索。"}</p>
          </div>
        ) : null}
      </section>
      <form className="search-form" action={withLocale(locale, "/s")}>
        <input name="q" defaultValue={q} placeholder={t.queryPlaceholder} />
        <button type="submit">{t.search}</button>
      </form>
      <div className="search-list">
        {results.data.map((item) => (
          <article className="search-result" key={`${item.kind}-${item.url}`}>
            <div className="post-meta">
              <span>{item.kind}</span>
              {item.updatedAt ? <time>{formatDate(item.updatedAt, locale)}</time> : null}
            </div>
            <h2>
              <Link href={withLocale(locale, item.url)}>{item.title}</Link>
            </h2>
            <p>{item.summary}</p>
            {item.matchedSnippet ? <p className="search-result__snippet">{item.matchedSnippet}</p> : null}
            {item.context ? <div className="tag-row"><span>{item.context}</span></div> : null}
          </article>
        ))}
      </div>
      <Pagination locale={locale} pageIndex={results.pageIndex} pageSize={results.pageSize} total={results.total} basePath="/s" query={{ q }} />
    </main>
  );
}

function first(value: string | string[] | undefined) {
  return Array.isArray(value) ? value[0] : value;
}
