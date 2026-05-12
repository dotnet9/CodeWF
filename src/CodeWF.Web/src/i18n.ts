import type { Locale } from "./types";

export const locales: Locale[] = ["zh-CN", "en", "ja", "zh-TW"];
export const defaultLocale: Locale = "zh-CN";

export const localeLabels: Record<Locale, string> = {
  "zh-CN": "简体中文",
  en: "English",
  ja: "日本語",
  "zh-TW": "繁體中文"
};

const dictionaries = {
  "zh-CN": {
    home: "首页",
    blog: "博客",
    blogLead: "先看文章，再按专题或分类继续浏览",
    more: "更多",
    posts: "文章",
    tools: "工具",
    projects: "项目",
    search: "搜索",
    about: "关于",
    timeline: "时间线",
    donation: "赞赏",
    recentPosts: "最新文章",
    featuredPosts: "精选内容",
    toolCatalog: "在线工具",
    projectCatalog: "项目中心",
    categories: "分类",
    albums: "专题",
    tags: "标签",
    allPosts: "全部文章",
    allAlbums: "全部专题",
    allCategories: "全部分类",
    allTags: "全部标签",
    latestPost: "最新",
    readMore: "阅读",
    openTool: "打开工具",
    empty: "暂无内容",
    queryPlaceholder: "搜索文章、项目和工具",
    privacy: "隐私",
    sitemap: "站点地图",
    sourceCode: "查看源码",
    feedback: "反馈问题",
    page: "页",
    previous: "上一篇",
    next: "下一篇"
  },
  en: {
    home: "Home",
    blog: "Blog",
    blogLead: "Start with posts, then browse by topic or category",
    more: "More",
    posts: "Posts",
    tools: "Tools",
    projects: "Projects",
    search: "Search",
    about: "About",
    timeline: "Timeline",
    donation: "Donation",
    recentPosts: "Recent posts",
    featuredPosts: "Featured",
    toolCatalog: "Online tools",
    projectCatalog: "Projects",
    categories: "Categories",
    albums: "Albums",
    tags: "Tags",
    allPosts: "All posts",
    allAlbums: "All topics",
    allCategories: "All categories",
    allTags: "All tags",
    latestPost: "Latest",
    readMore: "Read",
    openTool: "Open tool",
    empty: "No content yet",
    queryPlaceholder: "Search posts, projects, and tools",
    privacy: "Privacy",
    sitemap: "Sitemap",
    sourceCode: "Source code",
    feedback: "Feedback",
    page: "Page",
    previous: "Previous",
    next: "Next"
  },
  ja: {
    home: "ホーム",
    blog: "ブログ",
    blogLead: "記事から入り、特集や分類で広げる",
    more: "もっと",
    posts: "記事",
    tools: "ツール",
    projects: "プロジェクト",
    search: "検索",
    about: "概要",
    timeline: "タイムライン",
    donation: "寄付",
    recentPosts: "最新記事",
    featuredPosts: "注目内容",
    toolCatalog: "オンラインツール",
    projectCatalog: "プロジェクトセンター",
    categories: "カテゴリ",
    albums: "特集",
    tags: "タグ",
    allPosts: "すべての記事",
    allAlbums: "すべての特集",
    allCategories: "すべてのカテゴリ",
    allTags: "すべてのタグ",
    latestPost: "最新",
    readMore: "読む",
    openTool: "開く",
    empty: "コンテンツがありません",
    queryPlaceholder: "記事、プロジェクト、ツールを検索",
    privacy: "プライバシー",
    sitemap: "サイトマップ",
    sourceCode: "ソースコード",
    feedback: "フィードバック",
    page: "ページ",
    previous: "前へ",
    next: "次へ"
  },
  "zh-TW": {
    home: "首頁",
    blog: "部落格",
    blogLead: "先看文章，再按專題或分類繼續瀏覽",
    more: "更多",
    posts: "文章",
    tools: "工具",
    projects: "專案",
    search: "搜尋",
    about: "關於",
    timeline: "時間軸",
    donation: "贊助",
    recentPosts: "最新文章",
    featuredPosts: "精選內容",
    toolCatalog: "線上工具",
    projectCatalog: "專案中心",
    categories: "分類",
    albums: "專題",
    tags: "標籤",
    allPosts: "全部文章",
    allAlbums: "全部專題",
    allCategories: "全部分類",
    allTags: "全部標籤",
    latestPost: "最新",
    readMore: "閱讀",
    openTool: "打開工具",
    empty: "暫無內容",
    queryPlaceholder: "搜尋文章、專案和工具",
    privacy: "隱私",
    sitemap: "網站地圖",
    sourceCode: "查看原始碼",
    feedback: "回報問題",
    page: "頁",
    previous: "上一篇",
    next: "下一篇"
  }
} as const;

export function normalizeLocale(value?: string): Locale {
  const match = locales.find((locale) => locale.toLowerCase() === value?.toLowerCase());
  return match ?? defaultLocale;
}

export function isLocale(value?: string): value is Locale {
  return locales.some((locale) => locale.toLowerCase() === value?.toLowerCase());
}

export function dictionary(locale: Locale) {
  return dictionaries[locale] ?? dictionaries[defaultLocale];
}

export function withLocale(locale: Locale, path = "/") {
  const normalized = path.startsWith("/") ? path : `/${path}`;
  return `/${locale}${normalized === "/" ? "" : normalized}`;
}

export function formatDate(value: string | undefined, locale: Locale) {
  if (!value) {
    return "";
  }

  return new Intl.DateTimeFormat(locale, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit"
  }).format(new Date(value));
}
