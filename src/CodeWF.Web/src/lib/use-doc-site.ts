import type { SiteInfo } from '@/types';

/** 显式覆盖：构建/运行环境变量优先级最高 */
const envOverride = () => {
  const env = process.env.NEXT_PUBLIC_DOC_SITE_URL;
  return env ? env.replace(/\/+$/, '') : null;
};

/** 由站点配置域名推导（服务端渲染首屏用）：codewf.com → doc.codewf.com */
export function docSiteUrlFromSite(site?: SiteInfo | null) {
  const override = envOverride();
  if (override) return override;
  try {
    if (site?.domain) {
      const u = new URL(site.domain.includes('://') ? site.domain : `https://${site.domain}`);
      return `${u.protocol}//doc.${u.hostname}`;
    }
  } catch {
    // 配置的 domain 不是合法 URL 时走兜底
  }
  return 'https://doc.codewf.com';
}

/**
 * 由访客当前访问域名推导（浏览器运行时）：codewf.com → doc.codewf.com。
 * 本地开发（localhost）返回 null，调用方保持配置推导值，
 * 避免指向不存在的 doc.localhost。
 */
export function docSiteUrlFromLocation() {
  if (typeof window === 'undefined') return null;
  const host = window.location.hostname;
  if (!host || host === 'localhost' || host === '127.0.0.1' || host === '::1') {
    return null;
  }
  return `${window.location.protocol}//doc.${host}`;
}
