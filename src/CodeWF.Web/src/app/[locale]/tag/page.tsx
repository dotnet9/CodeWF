import Link from "next/link";
import type { Metadata } from "next";
import { api } from "@/api";
import { dictionary, withLocale } from "@/i18n";
import type { LocalePageProps } from "../layout";

type Props = LocalePageProps;

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = dictionary(locale);
  return {
    title: t.tags,
    description: locale === "zh-CN" ? "按标签浏览文章目录。" : "Browse posts by tag."
  };
}

export default async function TagDirectoryPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const tags = await api.tags(locale);
  const t = dictionary(locale);
  const count = tags.reduce((sum, item) => sum + item.postCount, 0);

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{t.tags}</h1>
          <p>{locale === "zh-CN" ? `${tags.length} 个标签，${count} 篇文章` : `${tags.length} tags, ${count} posts`}</p>
        </div>
      </div>
      <div className="tag-cloud">
        {tags.map((item) => (
          <Link href={withLocale(locale, `/tag/${encodeURIComponent(item.name)}`)} key={item.name}>
            {item.name}
            <span>{item.postCount}</span>
          </Link>
        ))}
      </div>
      {tags.length === 0 ? <div className="empty-state empty-state--compact">暂无标签。</div> : null}
    </main>
  );
}
