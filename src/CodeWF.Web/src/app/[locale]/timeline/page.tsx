import { api } from "@/api";
import { dictionary, formatDate } from "@/i18n";
import type { LocalePageProps } from "../layout";

export default async function TimelinePage({ params }: LocalePageProps) {
  const { locale } = await params;
  const items = await api.timelines(locale);
  const t = dictionary(locale);

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{t.timeline}</h1>
          <p>{locale === "zh-CN" ? "按时间回看站点更新与关键事件。" : "A chronological view of site updates and key events."}</p>
        </div>
      </div>
      <div className="timeline-list">
        {items.map((item) => (
          <article className="timeline-item" key={`${item.time}-${item.title}`}>
            <div className="post-meta">{formatDate(item.time, locale)}</div>
            <h2>{item.title}</h2>
            <p>{item.content}</p>
          </article>
        ))}
      </div>
    </main>
  );
}
