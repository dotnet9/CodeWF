import type {
  BlogPost,
  BlogPostBrief,
  DocNode,
  FriendLinkItem,
  HomePageData,
  Locale,
  MarkdownPage,
  PagedResult,
  SearchResultItem,
  SearchResultPageData,
  SiteInfo,
  TaxonomyItem,
  ToolNode
} from "./types";
import { normalizeLocale } from "./i18n";

const apiBase = (process.env.API_BASE_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5002/api").replace(/\/$/, "");

const emptySite: SiteInfo = {
  appTitle: "CodeWF",
  domain: "https://dotnet9.com",
  memo: ".NET 文章与在线工具",
  owner: "dotnet9",
  localAssetsDir: "D:\\wwwroot\\img1.dotnet9.com",
  assetBaseUrl: "https://img1.dotnet9.com",
  startYear: 2019,
  defaultCulture: "zh-CN",
  supportedCultures: ["zh-CN"]
};

async function getJson<T>(path: string, fallback: T, init?: RequestInit & { next?: { revalidate?: number } }): Promise<T> {
  try {
    const response = await fetch(`${apiBase}${path}`, {
      ...init,
      next: init?.next ?? { revalidate: 120 }
    });

    if (!response.ok) {
      return fallback;
    }

    return (await response.json()) as T;
  } catch {
    return fallback;
  }
}

type LocaleInput = Locale | string | undefined | null;

function query(locale: LocaleInput, values?: Record<string, string | number | undefined>) {
  const params = new URLSearchParams({ culture: normalizeLocale(locale) });
  Object.entries(values ?? {}).forEach(([key, value]) => {
    if (value !== undefined && value !== "") {
      params.set(key, String(value));
    }
  });
  return params.toString();
}

export const api = {
  site: () => getJson<SiteInfo>("/site", emptySite),
  home: (locale: LocaleInput) =>
    getJson<HomePageData>(`/home?${query(locale, { recent: 6 })}`, {
      site: emptySite,
      recentPosts: [],
      bannerPosts: [],
      categories: [],
      albums: [],
      tools: [],
      counts: {}
    }),
  posts: (locale: LocaleInput, values?: Record<string, string | number | undefined>) =>
    getJson<PagedResult<BlogPostBrief>>(`/posts?${query(locale, values)}`, {
      pageIndex: 1,
      pageSize: 12,
      total: 0,
      data: []
    }),
  post: (locale: LocaleInput, year: string, month: string, slug: string) =>
    getJson<BlogPost | null>(`/posts/${year}/${month}/${slug}?${query(locale)}`, null),
  categories: (locale: LocaleInput) => getJson<TaxonomyItem[]>(`/categories?${query(locale)}`, []),
  albums: (locale: LocaleInput) => getJson<TaxonomyItem[]>(`/albums?${query(locale)}`, []),
  tags: (locale: LocaleInput) => getJson<{ name: string; postCount: number }[]>(`/tags?${query(locale)}`, []),
  tools: (locale: LocaleInput) => getJson<ToolNode[]>(`/tools?${query(locale)}`, []),
  tool: (locale: LocaleInput, slug: string) => getJson<ToolNode | null>(`/tools/${slug}?${query(locale)}`, null),
  docs: (locale: LocaleInput) => getJson<DocNode[]>(`/docs?${query(locale)}`, []),
  doc: (locale: LocaleInput, slug: string) => getJson<DocNode | null>(`/docs/${slug}?${query(locale)}`, null),
  markdownPage: (locale: LocaleInput, name: "about" | "donation" | "privacy") =>
    getJson<MarkdownPage>(`/pages/${name}?${query(locale)}`, {}),
  friendLinks: (locale: LocaleInput) => getJson<FriendLinkItem[]>(`/friend-links?${query(locale)}`, []),
  timelines: (locale: LocaleInput) => getJson<{ time?: string; title?: string; content?: string }[]>(`/timelines?${query(locale)}`, []),
  search: (locale: LocaleInput, values?: Record<string, string | number | undefined>) =>
    getJson<SearchResultPageData>(`/search?${query(locale, values)}`, {
      pageIndex: 1,
      pageSize: 10,
      total: 0,
      data: [],
      postCount: 0,
      toolCount: 0,
      docCount: 0,
      isBlocked: false
    }),
  searchSuggestions: (locale: LocaleInput, q: string, take = 10) =>
    getJson<SearchResultItem[]>(`/search/suggest?${query(locale, { q, take })}`, [])
};

export function resolveAssetUrl(site: SiteInfo, value?: string) {
  if (!value) {
    return undefined;
  }

  if (/^https?:\/\//i.test(value) || value.startsWith("data:")) {
    return value;
  }

  return `${site.assetBaseUrl.replace(/\/$/, "")}/${value.replace(/^\//, "")}`;
}
