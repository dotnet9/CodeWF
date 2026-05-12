import { api } from "@/api";
import { CodeHighlighter } from "@/components/CodeHighlighter";
import { HtmlContent } from "@/components/HtmlContent";
import type { LocalePageProps } from "../layout";

export default async function PrivacyPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const page = await api.markdownPage(locale, "privacy");

  return (
    <main className="page-wrap">
      <article className="article-shell">
        <header className="article-header">
          <h1>Privacy</h1>
        </header>
        <section className="article-body">
          <HtmlContent html={page.htmlContent} />
        </section>
      </article>
      <CodeHighlighter />
    </main>
  );
}
