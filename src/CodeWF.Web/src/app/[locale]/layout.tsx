import { notFound } from "next/navigation";
import type { Metadata } from "next";
import { api } from "@/api";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { RouteTransitionShell } from "@/components/RouteTransitionShell";
import { isLocale, locales, normalizeLocale } from "@/i18n";
import type { Locale } from "@/types";

type Props = {
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
};

export function generateStaticParams() {
  return locales.map((locale) => ({ locale }));
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale: rawLocale } = await params;
  if (!isLocale(rawLocale)) {
    return {};
  }

  const locale = normalizeLocale(rawLocale);
  const home = await api.home(locale);
  const site = home.site;
  const apiOrigin = (process.env.NEXT_PUBLIC_API_BASE_URL ?? process.env.API_BASE_URL ?? "http://localhost:5100/api")
    .replace(/\/$/, "")
    .replace(/\/api$/, "");
  const iconUrl = `${apiOrigin}/site/favicon/logo.ico?v=20260513`;

  return {
    title: {
      default: site.appTitle,
      template: `%s | ${site.appTitle}`
    },
    description: site.memo,
    icons: {
      icon: iconUrl,
      shortcut: iconUrl,
      apple: iconUrl
    }
  };
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
      <RouteTransitionShell>{children}</RouteTransitionShell>
      <SiteFooter locale={locale} site={site} friendLinks={friendLinks} />
    </div>
  );
}

export type LocalePageProps = {
  params: Promise<{ locale: Locale }>;
  searchParams?: Promise<Record<string, string | string[] | undefined>>;
};
