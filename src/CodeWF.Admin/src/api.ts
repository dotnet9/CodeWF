export type BlogPostBrief = {
  title?: string;
  slug?: string;
  description?: string;
  date?: string;
  lastmod?: string;
  draft: boolean;
  banner: boolean;
  categories?: string[];
  albums?: string[];
  tags?: string[];
  cover?: string;
  author?: string;
  content?: string;
};

export type BlogPost = BlogPostBrief & {
  htmlContent?: string;
};

export type PagedResult<T> = {
  pageIndex: number;
  pageSize: number;
  total: number;
  data: T[];
};

export type ToolNode = {
  name?: string;
  memo?: string;
  slug?: string;
  repository?: string;
  children?: ToolNode[];
};

export type TaxonomyItem = {
  sort: number;
  name?: string;
  memo?: string;
  slug?: string;
  postCount: number;
};

export type SiteInfo = {
  appTitle: string;
  domain: string;
  memo: string;
  owner: string;
  ownerDesc?: string;
  favicon?: string;
  assetBaseUrl: string;
  remoteAssetsRepository?: string;
  startYear: number;
  defaultCulture: string;
  supportedCultures: string[];
  baiAn?: string;
  weChatName?: string;
  weChatImg?: string;
};

export type GitCommandResult = {
  success: boolean;
  command: string;
  output: string;
  error: string;
  exitCode: number;
};

export type HomePageData = {
  site: SiteInfo;
  recentPosts: BlogPostBrief[];
  bannerPosts: BlogPostBrief[];
  categories: TaxonomyItem[];
  albums: TaxonomyItem[];
  tools: ToolNode[];
  counts: Record<string, number>;
};

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5100/api").replace(/\/$/, "");

function token() {
  return localStorage.getItem("codewf-admin-token") ?? "";
}

function headers(json = false): HeadersInit {
  const value: HeadersInit = {};
  const adminToken = token();
  if (adminToken) {
    value["X-CodeWF-Admin-Key"] = adminToken;
  }
  if (json) {
    value["Content-Type"] = "application/json";
  }
  return value;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers: {
      ...headers(Boolean(init?.body)),
      ...(init?.headers ?? {})
    }
  });
  if (!response.ok) {
    throw new Error(await response.text());
  }
  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}

export const api = {
  setToken(value: string) {
    localStorage.setItem("codewf-admin-token", value);
  },
  getToken: token,
  home: () => request<HomePageData>("/home?culture=zh-CN"),
  posts: (pageIndex = 1, keyword = "") =>
    request<PagedResult<BlogPostBrief>>(`/admin/posts?pageIndex=${pageIndex}&pageSize=20&keyword=${encodeURIComponent(keyword)}`),
  post: (slug: string) => request<BlogPost>(`/admin/posts/${slug}`),
  createPost: (post: BlogPost) =>
    request("/admin/posts", {
      method: "POST",
      body: JSON.stringify(post)
    }),
  updatePost: (slug: string, post: BlogPost) =>
    request(`/admin/posts/${slug}`, {
      method: "PUT",
      body: JSON.stringify(post)
    }),
  deletePost: (slug: string) =>
    request(`/admin/posts/${slug}`, {
      method: "DELETE"
    }),
  tools: () => request<ToolNode[]>("/tools?culture=zh-CN"),
  gitStatus: () => request<GitCommandResult>("/admin/repository/status"),
  gitLog: () => request<GitCommandResult>("/admin/repository/log?count=20"),
  gitFetch: () => request<GitCommandResult>("/admin/repository/fetch", { method: "POST" }),
  gitPull: () => request<GitCommandResult>("/admin/repository/pull", { method: "POST" }),
  gitCommit: (message: string) =>
    request<GitCommandResult>("/admin/repository/commit", {
      method: "POST",
      body: JSON.stringify({ message })
    })
};
