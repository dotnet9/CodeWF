import type { SiteInfo } from '@/types';

/** 显式覆盖：构建/运行环境变量优先级最高 */
const envOverride = (key: 'NEXT_PUBLIC_DOC_SITE_URL' | 'NEXT_PUBLIC_TOOLS_SITE_URL') => {
  const env = process.env[key];
  return env ? env.replace(/\/+$/, '') : null;
};

/** 由站点配置域名推导站外子域（服务端渲染首屏用）：codewf.com → doc.codewf.com / tools.codewf.com */
const siteUrlFromSite = (site: SiteInfo | null | undefined, subdomain: string, fallback: string) => {
  try {
    if (site?.domain) {
      const u = new URL(site.domain.includes('://') ? site.domain : `https://${site.domain}`);
      return `${u.protocol}//${subdomain}.${u.hostname}`;
    }
  } catch {
    // 配置的 domain 不是合法 URL 时走兜底
  }
  return fallback;
};

/**
 * 由访客当前访问域名推导站外子域（浏览器运行时）：codewf.com → doc.codewf.com / tools.codewf.com。
 * 本地开发（localhost）返回 null，调用方保持配置推导值，
 * 避免指向不存在的 doc.localhost / tools.localhost。
 */
const siteUrlFromLocation = (subdomain: string) => {
  if (typeof window === 'undefined') return null;
  const host = window.location.hostname;
  if (!host || host === 'localhost' || host === '127.0.0.1' || host === '::1') {
    return null;
  }
  return `${window.location.protocol}//${subdomain}.${host}`;
};

/** 文档站（doc.codewf.com） */
export function docSiteUrlFromSite(site?: SiteInfo | null) {
  return envOverride('NEXT_PUBLIC_DOC_SITE_URL') ?? siteUrlFromSite(site, 'doc', 'https://doc.codewf.com');
}

export function docSiteUrlFromLocation() {
  return siteUrlFromLocation('doc');
}

/** 工具箱（tools.codewf.com）：与文档站同样的推导规则 */
export function toolsSiteUrlFromSite(site?: SiteInfo | null) {
  return envOverride('NEXT_PUBLIC_TOOLS_SITE_URL') ?? siteUrlFromSite(site, 'tools', 'https://tools.codewf.com');
}

export function toolsSiteUrlFromLocation() {
  return siteUrlFromLocation('tools');
}
