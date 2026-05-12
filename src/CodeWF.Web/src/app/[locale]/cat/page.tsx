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
    title: t.categories,
    description: locale === "zh-CN" ? "按分类浏览文章目录。" : "Browse posts by category."
  };
}

export default async function CategoryDirectoryPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const categories = await api.categories(locale);
  const t = dictionary(locale);
  const count = categories.reduce((sum, item) => sum + item.postCount, 0);

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{t.categories}</h1>
          <p>{locale === "zh-CN" ? `${categories.length} 个分类，${count} 篇文章` : `${categories.length} groups, ${count} posts`}</p>
        </div>
      </div>
      <div className="taxonomy-card-grid">
        {categories.map((item) => (
          <Link href={withLocale(locale, `/cat/${encodeURIComponent(item.slug ?? item.name ?? "")}`)} className="taxonomy-card" key={item.slug ?? item.name}>
            <strong>{item.name}</strong>
            <p>{item.memo}</p>
            <span>{item.postCount} posts</span>
          </Link>
        ))}
      </div>
      {categories.length === 0 ? <div className="empty-state empty-state--compact">暂无分类。</div> : null}
    </main>
  );
}
