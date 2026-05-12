import { notFound } from "next/navigation";
import { api } from "@/api";
import { CodeHighlighter } from "@/components/CodeHighlighter";
import { ContentToc, DocPager } from "@/components/ContentToc";
import { HtmlContent } from "@/components/HtmlContent";

type Props = {
  params: Promise<{ locale: "zh-CN" | "en" | "ja" | "zh-TW"; slug: string }>;
};

export async function generateMetadata({ params }: Props) {
  const { locale, slug } = await params;
  const doc = await api.doc(locale, slug);
  return {
    title: doc?.name ?? slug,
    description: doc?.memo
  };
}

export default async function ProjectDetailPage({ params }: Props) {
  const { locale, slug } = await params;
  const doc = await api.doc(locale, slug);
  if (!doc) {
    notFound();
  }

  return (
    <main className="page-wrap">
      <div className="article-layout">
        <div className="article-main">
          <article className="article-shell">
            <header className="article-header">
              <h1>{doc.name}</h1>
              <p>{doc.memo}</p>
              {doc.repository ? (
                <a className="text-link" href={doc.repository} target="_blank" rel="noreferrer">
                  相关仓库
                </a>
              ) : null}
            </header>
            <section className="article-body">
              <HtmlContent html={doc.htmlContent} />
            </section>
          </article>
          <DocPager locale={locale} previous={doc.previousDoc} next={doc.nextDoc} />
        </div>
        <ContentToc html={doc.htmlContent} />
      </div>
      <CodeHighlighter />
    </main>
  );
}
