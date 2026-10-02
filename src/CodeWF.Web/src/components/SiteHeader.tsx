"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { withLocale } from "@/i18n";
import type { BlogPostBrief, Locale, SiteInfo, TaxonomyItem } from "@/types";
import { GlobalSearch } from "./GlobalSearch";

const SITE_ICON_URL = "/logo.svg";
/** 文档站地址：通过环境变量配置，便于他人 clone 后使用自己的域名 */
const DOC_SITE_URL = process.env.NEXT_PUBLIC_DOC_SITE_URL || "https://doc.codewf.com";

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
  const links: { label: string; href: string; external?: boolean }[] = [
    { label: "首页", href: "/" },
    { label: "文章", href: "/post" },
    { label: "专题", href: "/album" },
    { label: "文档", href: DOC_SITE_URL, external: true },
    { label: "工具", href: "/tool" },
    { label: "时间线", href: "/timeline" },
    { label: "友链", href: "/friends" },
    { label: "关于", href: "/about" }
  ];

  return (
    <header className="site-header prototype-header">
      <Link href={withLocale(locale)} className="brand prototype-brand" aria-label={site.appTitle}>
        <img className="brand-icon" src={SITE_ICON_URL} alt="" />
        <span>Code<em>WF</em></span>
      </Link>

      <nav className="main-nav prototype-nav" aria-label="主导航">
        {links.map(({ label, href, external }) => {
          if (external) {
            return <a href={href} target="_blank" rel="noopener noreferrer" key={href}>{label}</a>;
          }
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
