export type Locale = "zh-CN" | "en" | "ja" | "zh-TW";

export type BlogPostBrief = {
  sourcePath?: string;
  title?: string;
  slug?: string;
  description?: string;
  date?: string;
  lastmod?: string;
  banner: boolean;
  author?: string;
  draft: boolean;
  cover?: string;
  albums?: string[];
  categories?: string[];
  tags?: string[];
  url?: string;
  year?: number;
  month?: number;
  contextLabel?: string;
};

export type BlogPost = BlogPostBrief & {
  content?: string;
  htmlContent?: string;
  estimatedReadingMinutes: number;
  headingCount: number;
  previousPost?: BlogPostBrief;
  nextPost?: BlogPostBrief;
  relatedPosts: BlogPostBrief[];
};

export type TaxonomyItem = {
  sort: number;
  name?: string;
  memo?: string;
  slug?: string;
  postCount: number;
};

export type ToolNode = {
  name?: string;
  memo?: string;
  slug?: string;
  repository?: string;
  children?: ToolNode[];
};

export type DocNode = {
  name?: string;
  memo?: string;
  slug?: string;
  repository?: string;
  content?: string;
  htmlContent?: string;
  children?: DocNode[];
  previousDoc?: DocNode;
  nextDoc?: DocNode;
};

export type FriendLinkItem = {
  index: number;
  title?: string;
  description?: string;
  link?: string;
  logo?: string;
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
  defaultCulture: Locale;
  supportedCultures: Locale[];
  baiAn?: string;
  weChatName?: string;
  weChatImg?: string;
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

export type PagedResult<T> = {
  pageIndex: number;
  pageSize: number;
  total: number;
  data: T[];
};

export type SearchResultItem = {
  kind: "Tool" | "Doc" | "Post";
  title: string;
  url: string;
  summary?: string;
  matchedSnippet?: string;
  context?: string;
  slug?: string;
  sourcePath?: string;
  updatedAt?: string;
  score: number;
};

export type SearchResultPageData = {
  pageIndex: number;
  pageSize: number;
  total: number;
  data: SearchResultItem[];
  toolCount: number;
  docCount: number;
  postCount: number;
  isBlocked?: boolean;
  notice?: string;
};

export type MarkdownPage = {
  markdown?: string;
  htmlContent?: string;
};
