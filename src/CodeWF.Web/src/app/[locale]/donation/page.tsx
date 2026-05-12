import { api } from "@/api";
import { HtmlContent } from "@/components/HtmlContent";
import { dictionary } from "@/i18n";
import type { LocalePageProps } from "../layout";

export default async function DonationPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const page = await api.markdownPage(locale, "donation");
  const t = dictionary(locale);

  return (
    <main className="page-wrap">
      <article className="article-shell">
        <header className="article-header">
          <h1>{t.donation}</h1>
        </header>
        <HtmlContent html={page.htmlContent} />
      </article>
    </main>
  );
}
