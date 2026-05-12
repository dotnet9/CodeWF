import { notFound } from "next/navigation";
import { api } from "@/api";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { isLocale, locales, normalizeLocale } from "@/i18n";
import type { Locale } from "@/types";

type Props = {
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
};

export function generateStaticParams() {
  return locales.map((locale) => ({ locale }));
}

export default async function LocaleLayout({ children, params }: Props) {
  const { locale: rawLocale } = await params;
  if (!isLocale(rawLocale)) {
    notFound();
  }

  const locale = normalizeLocale(rawLocale);
  const [home, friendLinks] = await Promise.all([api.home(locale), api.friendLinks(locale)]);
  const site = home.site;
  return (
    <div className="site-shell" data-locale={locale}>
      <SiteHeader
        locale={locale}
        site={site}
        categories={home.categories}
        albums={home.albums}
        latestPost={home.recentPosts[0]}
      />
      {children}
      <SiteFooter locale={locale} site={site} friendLinks={friendLinks} />
    </div>
  );
}

export type LocalePageProps = {
  params: Promise<{ locale: Locale }>;
  searchParams?: Promise<Record<string, string | string[] | undefined>>;
};
