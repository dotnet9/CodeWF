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

const apiBase = (process.env.API_BASE_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5002/api").replace(/\/$/, "");

const emptySite: SiteInfo = {
  appTitle: "CodeWF",
  domain: "https://dotnet9.com",
  memo: "Articles and tools",
  owner: "dotnet9",
  localAssetsDir: "D:\\wwwroot\\img1.dotnet9.com",
  assetBaseUrl: "https://img1.dotnet9.com",
  startYear: 2019,
  defaultCulture: "zh-CN",
  supportedCultures: ["zh-CN", "en", "ja", "zh-TW"]
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

function query(locale: Locale, values?: Record<string, string | number | undefined>) {
  const params = new URLSearchParams({ culture: locale });
  Object.entries(values ?? {}).forEach(([key, value]) => {
    if (value !== undefined && value !== "") {
      params.set(key, String(value));
    }
  });
  return params.toString();
}

export const api = {
  site: () => getJson<SiteInfo>("/site", emptySite),
  home: (locale: Locale) =>
    getJson<HomePageData>(`/home?${query(locale, { recent: 6 })}`, {
      site: emptySite,
      recentPosts: [],
      bannerPosts: [],
      categories: [],
      albums: [],
      tools: [],
      counts: {}
    }),
  posts: (locale: Locale, values?: Record<string, string | number | undefined>) =>
    getJson<PagedResult<BlogPostBrief>>(`/posts?${query(locale, values)}`, {
      pageIndex: 1,
      pageSize: 12,
      total: 0,
      data: []
    }),
  post: (locale: Locale, year: string, month: string, slug: string) =>
    getJson<BlogPost | null>(`/posts/${year}/${month}/${slug}?${query(locale)}`, null),
  categories: (locale: Locale) => getJson<TaxonomyItem[]>(`/categories?${query(locale)}`, []),
  albums: (locale: Locale) => getJson<TaxonomyItem[]>(`/albums?${query(locale)}`, []),
  tags: (locale: Locale) => getJson<{ name: string; postCount: number }[]>(`/tags?${query(locale)}`, []),
  tools: (locale: Locale) => getJson<ToolNode[]>(`/tools?${query(locale)}`, []),
  tool: (locale: Locale, slug: string) => getJson<ToolNode | null>(`/tools/${slug}?${query(locale)}`, null),
  docs: (locale: Locale) => getJson<DocNode[]>(`/docs?${query(locale)}`, []),
  doc: (locale: Locale, slug: string) => getJson<DocNode | null>(`/docs/${slug}?${query(locale)}`, null),
  markdownPage: (locale: Locale, name: "about" | "donation" | "privacy") =>
    getJson<MarkdownPage>(`/pages/${name}?${query(locale)}`, {}),
  friendLinks: (locale: Locale) => getJson<FriendLinkItem[]>(`/friend-links?${query(locale)}`, []),
  timelines: (locale: Locale) => getJson<{ time?: string; title?: string; content?: string }[]>(`/timelines?${query(locale)}`, []),
  search: (locale: Locale, values?: Record<string, string | number | undefined>) =>
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
  searchSuggestions: (locale: Locale, q: string, take = 10) =>
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
