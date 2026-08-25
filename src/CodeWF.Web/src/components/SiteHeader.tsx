"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { withLocale } from "@/i18n";
import type { BlogPostBrief, Locale, SiteInfo, TaxonomyItem } from "@/types";
import { GlobalSearch } from "./GlobalSearch";

const SITE_ICON_URL = "/logo.svg";

export function SiteHeader({
  locale,
  site
}: {
  locale: Locale;
  site: SiteInfo;
  categories: TaxonomyItem[];
  albums: TaxonomyItem[];
  latestPost?: BlogPostBrief;
}) {
  const pathname = usePathname();
  const links = [
    ["首页", "/"],
    ["文章", "/post"],
    ["专题", "/album"],
    ["文档", "/doc"],
    ["工具", "/tool"],
    ["时间线", "/timeline"],
    ["友链", "/friends"],
    ["关于", "/about"]
  ] as const;

  return (
    <header className="site-header prototype-header">
      <Link href={withLocale(locale)} className="brand prototype-brand" aria-label={site.appTitle}>
        <img className="brand-icon" src={SITE_ICON_URL} alt="" />
        <span>Code<em>WF</em></span>
      </Link>

      <nav className="main-nav prototype-nav" aria-label="主导航">
        {links.map(([label, href]) => {
          const localizedHref = withLocale(locale, href);
          const active = href === "/" ? pathname === localizedHref : Boolean(pathname?.startsWith(localizedHref));
          return <Link href={localizedHref} aria-current={active ? "page" : undefined} key={href}>{label}</Link>;
        })}
      </nav>

      <div className="header-actions prototype-header__actions">
        <GlobalSearch locale={locale} action={withLocale(locale, "/s")} placeholder="全局搜索" label="搜索" />
        <div className="prototype-language" aria-label="语言">
          <span className="is-current">中</span><span>EN</span><span>JA</span><span>繁</span>
        </div>
      </div>
    </header>
  );
}
