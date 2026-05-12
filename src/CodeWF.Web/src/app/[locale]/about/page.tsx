import { api } from "@/api";
import { HtmlContent } from "@/components/HtmlContent";
import { dictionary } from "@/i18n";
import type { LocalePageProps } from "../layout";

export default async function AboutPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const page = await api.markdownPage(locale, "about");
  const t = dictionary(locale);

  return (
    <main className="page-wrap">
      <article className="article-shell">
        <header className="article-header">
          <h1>{t.about}</h1>
        </header>
        <HtmlContent html={page.htmlContent} />
      </article>
    </main>
  );
}
