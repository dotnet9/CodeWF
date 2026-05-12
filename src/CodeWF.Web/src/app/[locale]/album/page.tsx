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
    title: t.albums,
    description: locale === "zh-CN" ? "按专题浏览文章目录。" : "Browse posts by album or topic."
  };
}

export default async function AlbumDirectoryPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const albums = await api.albums(locale);
  const t = dictionary(locale);
  const count = albums.reduce((sum, item) => sum + item.postCount, 0);

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{t.albums}</h1>
          <p>{locale === "zh-CN" ? `${albums.length} 个专题，${count} 篇文章` : `${albums.length} groups, ${count} posts`}</p>
        </div>
      </div>
      <div className="taxonomy-card-grid">
        {albums.map((item) => (
          <Link href={withLocale(locale, `/album/${encodeURIComponent(item.slug ?? item.name ?? "")}`)} className="taxonomy-card" key={item.slug ?? item.name}>
            <strong>{item.name}</strong>
            <p>{item.memo}</p>
            <span>{item.postCount} posts</span>
          </Link>
        ))}
      </div>
      {albums.length === 0 ? <div className="empty-state empty-state--compact">暂无专题。</div> : null}
    </main>
  );
}
