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
  hidden?: boolean;
  children?: ToolNode[];
};

export type TaxonomyItem = {
  sort: number;
  name?: string;
  memo?: string;
  slug?: string;
  postCount: number;
};

export type FriendLinkItem = {
  Index: number;
  Title?: string;
  Description?: string;
  Link?: string;
  Logo?: string;
};

export type TimelineItem = {
  Time?: string;
  Title?: string;
  Content?: string;
};

export type SearchBlockedKeywordGroup = {
  Sort: number;
  Name?: string;
  Memo?: string;
  Keywords?: string[];
};

export type SiteInfo = {
  appTitle: string;
  domain: string;
  memo: string;
  owner: string;
  ownerDesc?: string;
  favicon?: string;
  localAssetsDir: string;
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

export type GitChangeEntry = {
  status: string;
  path: string;
  originalPath?: string | null;
};

export type GitRepositoryStatusData = {
  branch: string;
  changes: GitChangeEntry[];
  changeCount: number;
};

export type GitFilePreviewResult = {
  success: boolean;
  path: string;
  name: string;
  kind: "text" | "image" | "binary" | "missing";
  textContent?: string | null;
  dataUrl?: string | null;
  mimeType?: string | null;
  size?: number | null;
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

export type EditableMarkdownResource = {
  name: string;
  path: string;
  markdown: string | null;
  htmlContent: string | null;
};

export type EditableJsonResource = {
  name: string;
  path: string;
  json: string | null;
};

export type ContentSaveResult = {
  success: boolean;
  message?: string | null;
  post?: BlogPostBrief | null;
};

export type AssetEntry = {
  name: string;
  path: string;
  isDirectory: boolean;
  size: number | null;
  lastModified: string | null;
};

export type AssetDirectoryData = {
  root: string;
  path: string;
  entries: AssetEntry[];
};

export type AssetUploadResult = {
  success: boolean;
  message: string;
  path?: string | null;
};

export type AdminCredentials = {
  userName: string;
  password: string;
};

export type AdminSession = {
  role: "reader" | "super-admin";
  canWrite: boolean;
};

export type SiteSettings = SiteInfo;

export type SiteSettingsRequest = {
  appTitle?: string;
  domain?: string;
  memo?: string;
  owner?: string;
  ownerDesc?: string;
  favicon?: string;
  localAssetsDir?: string;
  assetBaseUrl?: string;
  remoteAssetsRepository?: string;
  startYear?: number;
  baiAn?: string;
  weChatName?: string;
  weChatImg?: string;
  defaultCulture?: string;
  supportedCultures?: string[];
};

export type SiteSettingsResult = {
  success: boolean;
  message: string;
  site?: SiteSettings | null;
};

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? "/api").replace(/\/$/, "");

function credentials(): AdminCredentials {
  return {
    userName: localStorage.getItem("codewf-admin-user") ?? "",
    password: localStorage.getItem("codewf-admin-password") ?? ""
  };
}

function headers(json = false): HeadersInit {
  const value: HeadersInit = {};
  const auth = credentials();
  if (auth.userName) {
    value["X-CodeWF-Admin-User"] = auth.userName;
  }
  if (auth.password) {
    value["X-CodeWF-Admin-Password"] = auth.password;
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

async function upload<T>(path: string, body: BodyInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    method: "POST",
    body,
    headers: headers(false)
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
  setCredentials(value: AdminCredentials) {
    localStorage.setItem("codewf-admin-user", value.userName);
    localStorage.setItem("codewf-admin-password", value.password);
  },
  getCredentials: credentials,
  clearCredentials() {
    localStorage.removeItem("codewf-admin-user");
    localStorage.removeItem("codewf-admin-password");
  },
  verify: () => request<AdminSession>("/admin/session"),
  home: (culture = "zh-CN") => request<HomePageData>(`/home?culture=${encodeURIComponent(culture)}`),
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
  siteSettings: () => request<SiteSettings>("/site-settings"),
  saveSiteSettings: (payload: SiteSettingsRequest) =>
    request<SiteSettingsResult>("/site-settings", {
      method: "PUT",
      body: JSON.stringify(payload)
    }),
  markdownResource: (name: string, culture = "zh-CN") =>
    request<EditableMarkdownResource>(`/admin/content/markdown/${encodeURIComponent(name)}?culture=${encodeURIComponent(culture)}`),
  saveMarkdownResource: (name: string, content: string, culture = "zh-CN") =>
    request<ContentSaveResult>(`/admin/content/markdown/${encodeURIComponent(name)}?culture=${encodeURIComponent(culture)}`, {
      method: "PUT",
      body: JSON.stringify({ content })
    }),
  jsonResource: (name: string, culture = "zh-CN") =>
    request<EditableJsonResource>(`/admin/content/json/${encodeURIComponent(name)}?culture=${encodeURIComponent(culture)}`),
  saveJsonResource: (name: string, content: string, culture = "zh-CN") =>
    request<ContentSaveResult>(`/admin/content/json/${encodeURIComponent(name)}?culture=${encodeURIComponent(culture)}`, {
      method: "PUT",
      body: JSON.stringify({ content })
    }),
  assets: (path = "") => request<AssetDirectoryData>(`/admin/assets?path=${encodeURIComponent(path)}`),
  assetFile: (path: string) => request<GitFilePreviewResult>(`/admin/assets/file?path=${encodeURIComponent(path)}`),
  uploadAsset: (path: string, name: string, file: File) =>
    upload<AssetUploadResult>(`/admin/assets?path=${encodeURIComponent(path)}&name=${encodeURIComponent(name)}`, file),
  deleteAsset: (path: string) => request<AssetUploadResult>(`/admin/assets?path=${encodeURIComponent(path)}`, { method: "DELETE" }),
  tools: (culture = "zh-CN") => request<ToolNode[]>(`/tools?culture=${encodeURIComponent(culture)}`),
  gitStatus: () => request<GitCommandResult>("/admin/repository/status"),
  gitStatusDetails: () => request<GitRepositoryStatusData>("/admin/repository/status/details"),
  gitLog: () => request<GitCommandResult>("/admin/repository/log?count=20"),
  gitFile: (path: string) => request<GitFilePreviewResult>(`/admin/repository/file?path=${encodeURIComponent(path)}`),
  gitFetch: () => request<GitCommandResult>("/admin/repository/fetch", { method: "POST" }),
  gitPull: () => request<GitCommandResult>("/admin/repository/pull", { method: "POST" }),
  gitCommit: (message: string) =>
    request<GitCommandResult>("/admin/repository/commit", {
      method: "POST",
      body: JSON.stringify({ message })
    })
};
