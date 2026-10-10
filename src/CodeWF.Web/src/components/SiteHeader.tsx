"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { usePathname } from "next/navigation";
import { withLocale } from "@/i18n";
import type { BlogPostBrief, Locale, SiteInfo, TaxonomyItem } from "@/types";
import { GlobalSearch } from "./GlobalSearch";
import { docSiteUrlFromLocation, docSiteUrlFromSite, toolsSiteUrlFromLocation, toolsSiteUrlFromSite } from "@/lib/use-doc-site";

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
  // 文档站 / 工具箱地址：SSR 用站点配置域名推导，水合后按访客实际域名切换
  const [docSiteUrl, setDocSiteUrl] = useState(() => docSiteUrlFromSite(site));
  const [toolsSiteUrl, setToolsSiteUrl] = useState(() => toolsSiteUrlFromSite(site));
  useEffect(() => {
    setDocSiteUrl(docSiteUrlFromLocation() ?? docSiteUrlFromSite(site));
    setToolsSiteUrl(toolsSiteUrlFromLocation() ?? toolsSiteUrlFromSite(site));
  }, [site]);
  const links: { label: string; href: string; external?: boolean }[] = [
    { label: "首页", href: "/" },
    { label: "文章", href: "/post" },
    { label: "专题", href: "/album" },
    { label: "文档", href: docSiteUrl, external: true },
    { label: "工具", href: toolsSiteUrl, external: true },
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
