import Link from "next/link";
import { Github, Rss } from "lucide-react";
import { dictionary, localeLabels, locales, withLocale } from "@/i18n";
import type { FriendLinkItem, Locale, SiteInfo } from "@/types";

export function SiteFooter({
  locale,
  site,
  friendLinks
}: {
  locale: Locale;
  site: SiteInfo;
  friendLinks: FriendLinkItem[];
}) {
  const t = dictionary(locale);
  const year = new Date().getFullYear();
  const weChatImage = site.weChatImg ?? "https://img1.dotnet9.com/site/favicon/wechatpublic.jpg";

  return (
    <>
      <section className="friend-links-shell" aria-label="Friend links">
        <div className="friend-links">
          <div className="friend-links__label">友情链接</div>
          <div className="friend-links__grid">
            {friendLinks.length > 0 ? (
              friendLinks
                .slice()
                .sort((left, right) => left.index - right.index)
                .map((link) => (
                  <a href={link.link} key={`${link.title}-${link.link}`} target="_blank" rel="noreferrer" title={link.description ?? link.title}>
                    {link.title}
                  </a>
                ))
            ) : (
              <span className="muted">友情链接暂未配置</span>
            )}
          </div>
        </div>
      </section>

      <footer className="site-footer">
        <div className="footer-grid">
          <div className="footer-brand">
            <strong>{site.appTitle}</strong>
            <span>{site.memo}</span>
            <span>
              &copy; {site.startYear}-{year} {site.owner}
            </span>
          </div>

          <nav aria-label="Footer navigation">
            <h2>导航</h2>
            <Link href={withLocale(locale, "/post")}>{t.posts}</Link>
            <Link href={withLocale(locale, "/project")}>{t.projects}</Link>
            <Link href={withLocale(locale, "/tool")}>{t.tools}</Link>
            <Link href={withLocale(locale, "/tag")}>{t.tags}</Link>
            <Link href={withLocale(locale, "/timeline")}>{t.timeline}</Link>
          </nav>

          <nav aria-label="Community">
            <h2>社区</h2>
            <a href="https://www.cnblogs.com/Dotnet9-com" target="_blank" rel="noreferrer">
              博客园
            </a>
            <a href="https://space.bilibili.com/470546606" target="_blank" rel="noreferrer">
              B 站
            </a>
            <a href="https://github.com/dotnet9" target="_blank" rel="noreferrer">
              GitHub
            </a>
            <a href="https://irihi.tech/" target="_blank" rel="noreferrer">
              银沫科技
            </a>
          </nav>

          <nav aria-label="Entries">
            <h2>入口</h2>
            <Link href={withLocale(locale, "/about")}>{t.about}</Link>
            <Link href={withLocale(locale, "/donation")}>{t.donation}</Link>
            <Link href={withLocale(locale, "/privacy")}>{t.privacy}</Link>
            <a href="/rss" target="_blank" rel="noreferrer">
              <Rss size={15} /> RSS
            </a>
            <a href="/sitemap.xml" target="_blank" rel="noreferrer">
              {t.sitemap}
            </a>
            <a href="https://github.com/dotnet9/CodeWF" target="_blank" rel="noreferrer">
              {t.sourceCode}
            </a>
            <a href="https://github.com/dotnet9/CodeWF/issues" target="_blank" rel="noreferrer">
              {t.feedback}
            </a>
            {site.remoteAssetsRepository ? (
              <a href={site.remoteAssetsRepository} target="_blank" rel="noreferrer">
                <Github size={15} /> Assets
              </a>
            ) : null}
          </nav>

          <div className="footer-language">
            <h2>语言</h2>
            <div className="footer-language__links">
              {locales.map((item) => (
                <Link href={`/${item}`} key={item} aria-current={item === locale ? "page" : undefined}>
                  {localeLabels[item]}
                </Link>
              ))}
            </div>
          </div>

          <div className="footer-qr">
            <img src={weChatImage} alt={`${site.weChatName ?? site.appTitle} WeChat`} loading="lazy" />
            <span>
              关注公众号
              <br />
              {site.weChatName ?? site.appTitle}
            </span>
          </div>
        </div>
      </footer>
    </>
  );
}
