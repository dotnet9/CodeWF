"use client";

import Link from "next/link";
import {
  ArrowUpRight,
  BookOpenText,
  ChevronDown,
  Clock3,
  HeartHandshake,
  MapPinned,
  Rss,
  ShieldCheck,
  Tv2
} from "lucide-react";
import { dictionary, withLocale } from "@/i18n";
import type { BlogPostBrief, Locale, SiteInfo, TaxonomyItem } from "@/types";
import { GlobalSearch } from "./GlobalSearch";

const SITE_ICON_URL = "/logo.svg";

export function SiteHeader({
  locale,
  site,
  categories,
  albums,
  latestPost
}: {
  locale: Locale;
  site: SiteInfo;
  categories: TaxonomyItem[];
  albums: TaxonomyItem[];
  latestPost?: BlogPostBrief;
}) {
  const t = dictionary(locale);
  const topCategories = categories.slice(0, 8);
  const topAlbums = albums.slice(0, 8);

  return (
    <header className="site-header">
      <Link href={withLocale(locale)} className="brand" aria-label={site.appTitle}>
        <img className="brand-icon" src={SITE_ICON_URL} alt="" />
        <span>{site.appTitle}</span>
      </Link>

      <nav className="main-nav" aria-label="Primary navigation">
        <Link href={withLocale(locale, "/")}>{t.home}</Link>

        <div className="nav-item">
          <button type="button" className="nav-trigger">
            {t.blog}
            <ChevronDown size={14} aria-hidden="true" />
          </button>
          <div className="nav-dropdown nav-mega">
            <div className="nav-mega__header">
              <div>
                <span>Blog</span>
                <strong>{t.blogLead}</strong>
              </div>
              <small>{topCategories.length + topAlbums.length} 项内容</small>
            </div>
            <div className="nav-mega__actions">
              <Link href={withLocale(locale, "/post")}>{t.allPosts}</Link>
              <Link href={withLocale(locale, "/album")}>{t.allAlbums}</Link>
              <Link href={withLocale(locale, "/cat")}>{t.allCategories}</Link>
              <Link href={withLocale(locale, "/tag")}>{t.allTags}</Link>
            </div>
            {latestPost ? (
              <Link href={withLocale(locale, latestPost.url ?? "/post")} className="nav-feature">
                <span>{t.latestPost}</span>
                <strong>{latestPost.title}</strong>
                <small>{latestPost.description}</small>
              </Link>
            ) : null}
            <div className="nav-browse">
              <section>
                <h2>{t.albums}</h2>
                {topAlbums.map((item) => (
                  <Link href={withLocale(locale, `/album/${encodeURIComponent(item.slug ?? item.name ?? "")}`)} key={item.slug ?? item.name}>
                    <span>{item.name}</span>
                    <small>{item.postCount}</small>
                  </Link>
                ))}
              </section>
              <section>
                <h2>{t.categories}</h2>
                {topCategories.map((item) => (
                  <Link href={withLocale(locale, `/cat/${encodeURIComponent(item.slug ?? item.name ?? "")}`)} key={item.slug ?? item.name}>
                    <span>{item.name}</span>
                    <small>{item.postCount}</small>
                  </Link>
                ))}
              </section>
            </div>
          </div>
        </div>

        <Link href={withLocale(locale, "/project")}>{t.projects}</Link>
        <Link href={withLocale(locale, "/album")}>{t.albums}</Link>
        <Link href={withLocale(locale, "/tool")}>{t.tools}</Link>

        <div className="nav-item">
          <button type="button" className="nav-trigger">
            {t.more}
            <ChevronDown size={14} aria-hidden="true" />
          </button>
          <div className="nav-dropdown nav-dropdown--compact nav-more">
            <div className="nav-more__header">
              <span>{t.more}</span>
              <strong>{locale === "zh-CN" ? "更多内容" : "Site links"}</strong>
            </div>
            <div className="nav-more__grid">
              <section>
                <h2>{locale === "zh-CN" ? "页面" : "Pages"}</h2>
                <Link href={withLocale(locale, "/about")} className="nav-more__link">
                  <BookOpenText size={16} />
                  <span>{t.about}</span>
                </Link>
                <Link href={withLocale(locale, "/timeline")} className="nav-more__link">
                  <Clock3 size={16} />
                  <span>{t.timeline}</span>
                </Link>
                <Link href={withLocale(locale, "/donation")} className="nav-more__link">
                  <HeartHandshake size={16} />
                  <span>{t.donation}</span>
                </Link>
                <Link href={withLocale(locale, "/privacy")} className="nav-more__link">
                  <ShieldCheck size={16} />
                  <span>{t.privacy}</span>
                </Link>
              </section>
              <section>
                <h2>{locale === "zh-CN" ? "外部" : "External"}</h2>
                <a href="/rss" target="_blank" rel="noreferrer" className="nav-more__link">
                  <Rss size={16} />
                  <span>RSS</span>
                </a>
                <a href="/sitemap" target="_blank" rel="noreferrer" className="nav-more__link">
                  <MapPinned size={16} />
                  <span>{t.sitemap}</span>
                </a>
                <a href="https://www.cnblogs.com/Dotnet9-com" target="_blank" rel="noreferrer" className="nav-more__link">
                  <ArrowUpRight size={16} />
                  <span>{locale === "zh-CN" ? "博客园" : "Cnblogs"}</span>
                </a>
                <a href="https://space.bilibili.com/470546606" target="_blank" rel="noreferrer" className="nav-more__link">
                  <Tv2 size={16} />
                  <span>Bilibili</span>
                </a>
              </section>
            </div>
          </div>
        </div>
      </nav>

      <div className="header-actions">
        <GlobalSearch locale={locale} action={withLocale(locale, "/s")} placeholder={t.queryPlaceholder} label={t.search} />
      </div>
    </header>
  );
}
