import { useEffect, useMemo, useRef, useState, type Key, type ReactNode } from "react";
import {
  Alert,
  App as AntApp,
  Button,
  Card,
  Col,
  Empty,
  Form,
  Image,
  Input,
  InputNumber,
  Layout,
  Menu,
  type MenuProps,
  Modal,
  Row,
  Select,
  Space,
  Spin,
  Statistic,
  Switch,
  Table,
  Tabs,
  Tag,
  Tooltip,
  Typography
} from "antd";
import { ConfigProvider } from "antd";
import enUS from "antd/locale/en_US";
import zhCN from "antd/locale/zh_CN";
import type { ColumnsType } from "antd/es/table";
import {
  BookOutlined,
  BranchesOutlined,
  CloudDownloadOutlined,
  DatabaseOutlined,
  DashboardOutlined,
  EditOutlined,
  DeleteOutlined,
  FileTextOutlined,
  FolderOpenOutlined,
  LogoutOutlined,
  PlusOutlined,
  ReloadOutlined,
  SaveOutlined,
  SettingOutlined,
  SyncOutlined,
  TagsOutlined,
} from "@ant-design/icons";
import {
  api,
  type AdminCredentials,
  type AssetDirectoryData,
  type AssetEntry,
  type BlogPost,
  type BlogPostBrief,
  type EditableJsonResource,
  type EditableMarkdownResource,
  type GitChangeEntry,
  type GitCommandResult,
  type GitFilePreviewResult,
  type GitRepositoryStatusData,
  type HomePageData,
  type TaxonomyItem,
  type FriendLinkItem,
  type SearchBlockedKeywordGroup,
  type SiteSettings,
  type SiteSettingsRequest,
  type TimelineItem,
  type ToolNode
} from "./api";

const { Header, Content, Sider } = Layout;
const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5100/api").replace(/\/$/, "");
const API_ORIGIN = API_BASE.replace(/\/api$/, "");
const SITE_LOGO_URL = `${API_ORIGIN}/site/favicon/logo.ico`;

type AdminLocale = "zh-CN" | "en";

const CULTURES = [
  { label: "简体中文", value: "zh-CN" },
  { label: "English", value: "en" }
] as const;

const SITE_CULTURES = [
  { label: "简体中文", value: "zh-CN" },
  { label: "English", value: "en" },
  { label: "日本語", value: "ja" },
  { label: "繁體中文", value: "zh-TW" }
] as const;

const ADMIN_TEXT: Record<
  AdminLocale,
  {
    workspaceTitle: string;
    workspaceSubtitle: string;
    overview: string;
    posts: string;
    pages: string;
    resources: string;
    site: string;
    repository: string;
    logout: string;
    contentShortcuts: string;
    refresh: string;
    articleManagement: string;
    sitePages: string;
    resourceRepo: string;
    versionControl: string;
    sitePreview: string;
    defaultCulture: string;
    siteTitle: string;
    assetsDir: string;
    record: string;
    recentPosts: string;
    bannerPosts: string;
    recommendedTools: string;
    noPosts: string;
    noTools: string;
    loginTitle: string;
    loginSubtitle: string;
    usernamePlaceholder: string;
    passwordPlaceholder: string;
    loginButton: string;
    defaultAccount: string;
    failedAuth: string;
  }
> = {
  "zh-CN": {
    workspaceTitle: "后台工作台",
    workspaceSubtitle: "文件型内容仓库维护面板",
    overview: "概览",
    posts: "文章",
    pages: "站点页",
    resources: "资源仓库",
    site: "站点设置",
    repository: "版本控制",
    logout: "退出",
    contentShortcuts: "内容快捷入口",
    refresh: "刷新",
    articleManagement: "文章管理",
    sitePages: "站点页",
    resourceRepo: "资源仓库",
    versionControl: "版本控制",
    sitePreview: "站点结构预览",
    defaultCulture: "默认语言",
    siteTitle: "站点标题",
    assetsDir: "资源目录",
    record: "备案号",
    recentPosts: "最新文章",
    bannerPosts: "置顶文章",
    recommendedTools: "工具推荐",
    noPosts: "暂无文章",
    noTools: "暂无工具",
    loginTitle: "CodeWF 后台",
    loginSubtitle: "进入工作台前需要先完成账号验证。",
    usernamePlaceholder: "用户名",
    passwordPlaceholder: "密码",
    loginButton: "验证并进入",
    defaultAccount: "默认账号：codewf / codewf.com",
    failedAuth: "验证失败"
  },
  en: {
    workspaceTitle: "Admin Workspace",
    workspaceSubtitle: "File-based content repository console",
    overview: "Overview",
    posts: "Posts",
    pages: "Pages",
    resources: "Repository",
    site: "Site Settings",
    repository: "Git",
    logout: "Sign out",
    contentShortcuts: "Content shortcuts",
    refresh: "Refresh",
    articleManagement: "Post management",
    sitePages: "Site pages",
    resourceRepo: "Resource repo",
    versionControl: "Version control",
    sitePreview: "Site structure preview",
    defaultCulture: "Default locale",
    siteTitle: "Site title",
    assetsDir: "Assets directory",
    record: "Record number",
    recentPosts: "Recent posts",
    bannerPosts: "Featured posts",
    recommendedTools: "Recommended tools",
    noPosts: "No posts",
    noTools: "No tools",
    loginTitle: "CodeWF Admin",
    loginSubtitle: "Verify your account before entering the workspace.",
    usernamePlaceholder: "Username",
    passwordPlaceholder: "Password",
    loginButton: "Verify and enter",
    defaultAccount: "Default account: codewf / codewf.com",
    failedAuth: "Verification failed"
  }
};

type AdminText = (typeof ADMIN_TEXT)[AdminLocale];
type ViewKey =
  | "overview"
  | "posts"
  | "assets"
  | "tools"
  | "site"
  | "repository"
  | `page:${string}`
  | `json:${string}`;
type LoginState = "checking" | "locked" | "ready";

type PostFormValues = {
  title?: string;
  slug?: string;
  description?: string;
  date?: string;
  lastmod?: string;
  cover?: string;
  author?: string;
  draft?: boolean;
  banner?: boolean;
  categoriesText?: string;
  albumsText?: string;
  tagsText?: string;
  content?: string;
};

type MarkdownEditorSpec = {
  name: string;
  label: string;
  description: string;
  pathHint: string;
};

type JsonEditorSpec = {
  name: string;
  label: string;
  description: string;
  pathHint: string;
  kind: "taxonomy" | "links" | "timeline" | "blocked" | "tree";
  allowHidden?: boolean;
};

const MARKDOWN_PAGES: MarkdownEditorSpec[] = [
  { name: "about", label: "关于", description: "站点简介和作者信息。", pathHint: "site/about.md" },
  { name: "donation", label: "赞赏", description: "赞赏页面内容。", pathHint: "site/pays/Donation.md" },
  { name: "privacy", label: "隐私", description: "隐私政策页面。", pathHint: "site/Privacy.md" }
];

const JSON_RESOURCES: JsonEditorSpec[] = [
  { name: "friend-links", label: "友情链接", description: "页脚友情链接配置。", pathHint: "site/friend-links.json", kind: "links" },
  { name: "timelines", label: "时间线", description: "站点时间线条目。", pathHint: "site/timelines.json", kind: "timeline" },
  { name: "tools", label: "工具目录", description: "工具分类树。", pathHint: "site/tools/tools.json", kind: "tree", allowHidden: true },
  { name: "navigation", label: "文档导航", description: "项目和文档树。", pathHint: "site/doc/navigation.json", kind: "tree" },
  { name: "blocked-search-keywords", label: "屏蔽词", description: "屏蔽搜索关键词分组。", pathHint: "site/blocked-search-keywords.json", kind: "blocked" },
  { name: "categories", label: "分类", description: "分类定义。", pathHint: "site/categories.json", kind: "taxonomy" },
  { name: "albums", label: "专题", description: "专题定义。", pathHint: "site/albums.json", kind: "taxonomy" }
];

const DEFAULT_MENU_OPEN_KEYS = ["content-pages", "content-taxonomy", "data-json"];
const TAXONOMY_RESOURCE_NAMES = new Set(["categories", "albums"]);
const ADMIN_LOCALE_STORAGE_KEY = "codewf.admin.locale";

function readAdminLocale(): AdminLocale {
  if (typeof window === "undefined") {
    return "zh-CN";
  }

  const stored = window.localStorage.getItem(ADMIN_LOCALE_STORAGE_KEY);
  return stored === "en" ? "en" : "zh-CN";
}

export default function App() {
  const [culture, setCulture] = useState<AdminLocale>(readAdminLocale);
  const antdLocale = culture === "en" ? enUS : zhCN;

  useEffect(() => {
    window.localStorage.setItem(ADMIN_LOCALE_STORAGE_KEY, culture);
    document.title = culture === "en" ? "CodeWF Admin" : "CodeWF 后台";
  }, [culture]);

  return (
    <ConfigProvider locale={antdLocale} theme={{ token: { colorPrimary: "#0e7667", borderRadius: 8 } }}>
      <AntApp>
        <AuthGate culture={culture} onCultureChange={setCulture} />
      </AntApp>
    </ConfigProvider>
  );
}

function AuthGate({
  culture,
  onCultureChange
}: {
  culture: AdminLocale;
  onCultureChange: (value: AdminLocale) => void;
}) {
  const [state, setState] = useState<LoginState>("checking");
  const [credentials, setCredentials] = useState<AdminCredentials>(api.getCredentials());
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const text = ADMIN_TEXT[culture];

  useEffect(() => {
    if (!credentials.userName || !credentials.password) {
      setState("locked");
      return;
    }

    verify(credentials).catch(() => undefined);
  }, []);

  async function verify(value: AdminCredentials) {
    setPending(true);
    setError(null);
    try {
      api.setCredentials(value);
      await api.verify();
      setCredentials(value);
      setState("ready");
    } catch (err) {
      api.clearCredentials();
      setCredentials({ userName: "", password: "" });
      setState("locked");
      setError(err instanceof Error ? err.message : text.failedAuth);
    } finally {
      setPending(false);
    }
  }

  if (state === "checking") {
    return (
      <CenteredShell culture={culture} onCultureChange={onCultureChange}>
        <Spin size="large" />
      </CenteredShell>
    );
  }

  if (state === "locked") {
    return (
      <CenteredShell culture={culture} onCultureChange={onCultureChange}>
        <LoginPanel
          culture={culture}
          pending={pending}
          error={error}
          defaultValue={credentials}
          onSubmit={verify}
        />
      </CenteredShell>
    );
  }

  return (
    <Workspace
      culture={culture}
      onCultureChange={onCultureChange}
      onLogout={() => {
        api.clearCredentials();
        setCredentials({ userName: "", password: "" });
        setState("locked");
      }}
    />
  );
}

function CenteredShell({
  children,
  culture,
  onCultureChange
}: {
  children: ReactNode;
  culture: AdminLocale;
  onCultureChange: (value: AdminLocale) => void;
}) {
  return (
    <Layout className="login-shell">
      <Card className="login-card">
        <div className="login-shell__locale">
          <Select
            value={culture}
            onChange={onCultureChange}
            className="culture-select"
            options={CULTURES.map((item) => ({ label: item.label, value: item.value }))}
          />
        </div>
        {children}
      </Card>
    </Layout>
  );
}

function LoginPanel({
  culture,
  pending,
  error,
  defaultValue,
  onSubmit
}: {
  culture: AdminLocale;
  pending: boolean;
  error: string | null;
  defaultValue: AdminCredentials;
  onSubmit: (value: AdminCredentials) => Promise<void>;
}) {
  const [value, setValue] = useState<AdminCredentials>(defaultValue);
  const text = ADMIN_TEXT[culture];

  return (
    <div className="login-panel">
      <div className="login-brand">
        <img className="brand-icon" src={SITE_LOGO_URL} alt="" />
        <div>
          <Typography.Title level={3}>{text.loginTitle}</Typography.Title>
          <Typography.Paragraph type="secondary">{text.loginSubtitle}</Typography.Paragraph>
        </div>
      </div>
      {error ? <Alert type="error" showIcon message={text.failedAuth} description={error} /> : null}
      <Space direction="vertical" className="wide" size={12}>
        <Input
          value={value.userName}
          onChange={(event) => setValue((current) => ({ ...current, userName: event.target.value }))}
          onPressEnter={() => onSubmit(value)}
          placeholder={text.usernamePlaceholder}
        />
        <Input.Password
          value={value.password}
          onChange={(event) => setValue((current) => ({ ...current, password: event.target.value }))}
          onPressEnter={() => onSubmit(value)}
          placeholder={text.passwordPlaceholder}
        />
        <Button type="primary" loading={pending} onClick={() => onSubmit(value)}>
          {text.loginButton}
        </Button>
      </Space>
      <Typography.Text type="secondary">{text.defaultAccount}</Typography.Text>
    </div>
  );
}

function Workspace({
  culture,
  onCultureChange,
  onLogout
}: {
  culture: AdminLocale;
  onCultureChange: (value: AdminLocale) => void;
  onLogout: () => void;
}) {
  const [view, setView] = useState<ViewKey>("overview");
  const [collapsed, setCollapsed] = useState(false);
  const [openKeys, setOpenKeys] = useState<string[]>(DEFAULT_MENU_OPEN_KEYS);
  const text = ADMIN_TEXT[culture];
  const menuItems = useMemo(() => buildAdminMenuItems(culture, text), [culture, text]);
  const activeTitle = getWorkspaceViewTitle(view, culture);

  return (
    <Layout className="admin-shell">
      <Sider collapsible collapsed={collapsed} onCollapse={setCollapsed} theme="light" width={280} className="admin-sider">
        <div className="admin-brand">
          <img className="brand-icon" src={SITE_LOGO_URL} alt="" />
          {collapsed ? (
            <span className="admin-brand__abbr">CW</span>
          ) : (
            <div className="admin-brand__copy">
              <strong>{text.workspaceTitle}</strong>
              <span>{text.workspaceSubtitle}</span>
            </div>
          )}
        </div>
        <Menu
          className="admin-menu"
          mode="inline"
          selectedKeys={[view]}
          openKeys={collapsed ? [] : openKeys}
          onOpenChange={setOpenKeys}
          onClick={(event) => setView(event.key as ViewKey)}
          items={menuItems}
        />
      </Sider>
      <Layout className="admin-main">
        <Header className="admin-header">
          <div className="admin-header__title">
            <Typography.Title level={4}>{activeTitle}</Typography.Title>
            <Typography.Text type="secondary">{text.workspaceSubtitle}</Typography.Text>
          </div>
          <Space wrap>
            <Select
              value={culture}
              onChange={onCultureChange}
              className="culture-select"
              options={CULTURES.map((item) => ({ label: item.label, value: item.value }))}
            />
            <Button icon={<LogoutOutlined />} onClick={onLogout}>
              {text.logout}
            </Button>
          </Space>
        </Header>
        <Content className="admin-content">
          <div className="admin-content-scroll" key={view}>
            {renderWorkspaceView(view, culture, setView)}
          </div>
        </Content>
      </Layout>
    </Layout>
  );
}

function buildAdminMenuItems(culture: AdminLocale, text: AdminText): MenuProps["items"] {
  const taxonomyResources = JSON_RESOURCES.filter((item) => TAXONOMY_RESOURCE_NAMES.has(item.name));
  const dataResources = JSON_RESOURCES.filter((item) => !TAXONOMY_RESOURCE_NAMES.has(item.name));

  return [
    {
      type: "group",
      label: "CORE",
      children: [{ key: "overview", icon: <DashboardOutlined />, label: text.overview }]
    },
    {
      type: "group",
      label: "CONTENT",
      children: [
        { key: "posts", icon: <FileTextOutlined />, label: text.articleManagement },
        {
          key: "content-pages",
          icon: <BookOutlined />,
          label: menuLabel(culture === "en" ? "Site pages" : "站点页面", MARKDOWN_PAGES.length),
          children: MARKDOWN_PAGES.map((item) => ({
            key: `page:${item.name}`,
            label: item.label
          }))
        },
        {
          key: "content-taxonomy",
          icon: <TagsOutlined />,
          label: menuLabel(culture === "en" ? "Taxonomy" : "栏目专题", taxonomyResources.length),
          children: taxonomyResources.map((item) => ({
            key: `json:${item.name}`,
            label: item.label
          }))
        }
      ]
    },
    {
      type: "group",
      label: "DATA",
      children: [
        {
          key: "data-json",
          icon: <DatabaseOutlined />,
          label: menuLabel(culture === "en" ? "Config data" : "配置数据", dataResources.length),
          children: dataResources.map((item) => ({
            key: `json:${item.name}`,
            label: item.name === "tools" ? (culture === "en" ? "Tools JSON" : "工具 JSON") : item.label
          }))
        },
        { key: "assets", icon: <FolderOpenOutlined />, label: text.resourceRepo }
      ]
    },
    {
      type: "group",
      label: "SYSTEM",
      children: [
        { key: "site", icon: <SettingOutlined />, label: text.site },
        { key: "repository", icon: <BranchesOutlined />, label: text.repository }
      ]
    }
  ];
}

function menuLabel(label: string, badge?: number | string) {
  return (
    <span className="admin-menu-label">
      <span>{label}</span>
      {badge ? <span className="admin-menu-badge">{badge}</span> : null}
    </span>
  );
}

function getWorkspaceViewTitle(view: ViewKey, culture: AdminLocale) {
  const text = ADMIN_TEXT[culture];
  if (view === "overview") {
    return text.overview;
  }
  if (view === "posts") {
    return text.articleManagement;
  }
  if (view === "tools") {
    return culture === "en" ? "Tool tree" : "工具树";
  }
  if (view === "assets") {
    return text.resourceRepo;
  }
  if (view === "site") {
    return text.site;
  }
  if (view === "repository") {
    return text.repository;
  }
  return getMarkdownSpec(view)?.label ?? getJsonSpec(view)?.label ?? text.workspaceTitle;
}

function renderWorkspaceView(view: ViewKey, culture: AdminLocale, onJump: (view: ViewKey) => void) {
  if (view === "overview") {
    return <Overview culture={culture} onJump={onJump} />;
  }
  if (view === "posts") {
    return <Posts />;
  }
  if (view === "tools") {
    const spec = JSON_RESOURCES.find((item) => item.name === "tools");
    return spec ? <JsonEditor culture={culture} spec={spec} /> : <Overview culture={culture} onJump={onJump} />;
  }

  const markdownSpec = getMarkdownSpec(view);
  if (markdownSpec) {
    return <MarkdownEditor culture={culture} spec={markdownSpec} />;
  }

  const jsonSpec = getJsonSpec(view);
  if (jsonSpec) {
    return <JsonEditor culture={culture} spec={jsonSpec} />;
  }
  if (view === "assets") {
    return <AssetBrowser />;
  }
  if (view === "site") {
    return <SiteSettingsEditor />;
  }
  if (view === "repository") {
    return <Repository />;
  }

  return <Overview culture={culture} onJump={onJump} />;
}

function getMarkdownSpec(view: ViewKey) {
  if (!view.startsWith("page:")) {
    return null;
  }
  return MARKDOWN_PAGES.find((item) => item.name === view.slice("page:".length)) ?? null;
}

function getJsonSpec(view: ViewKey) {
  if (!view.startsWith("json:")) {
    return null;
  }
  return JSON_RESOURCES.find((item) => item.name === view.slice("json:".length)) ?? null;
}

function Overview({ culture, onJump }: { culture: AdminLocale; onJump: (view: ViewKey) => void }) {
  const [home, setHome] = useState<HomePageData | null>(null);
  const { message } = AntApp.useApp();
  const text = ADMIN_TEXT[culture];

  const refresh = async () => {
    try {
      setHome(await api.home(culture));
    } catch (error) {
      message.error(error instanceof Error ? error.message : `${text.overview} ${culture === "en" ? "load failed" : "加载失败"}`);
      setHome(null);
    }
  };

  useEffect(() => {
    refresh();
  }, [culture]);

  const counts = home?.counts ?? {};

  return (
    <div className="workspace-stack">
      <div className="stat-grid">
        {[
          ["文章", counts.posts ?? 0],
          ["工具", counts.tools ?? 0],
          ["文档", counts.docs ?? 0],
          ["分类", counts.categories ?? 0],
          ["专题", counts.albums ?? 0]
        ].map(([label, value]) => (
          <Card key={label as string}>
            <Statistic title={label as string} value={value as number} />
          </Card>
        ))}
      </div>

      <Row gutter={[16, 16]}>
        <Col span={12}>
          <Card title={text.contentShortcuts} extra={<Button icon={<ReloadOutlined />} onClick={refresh}>{text.refresh}</Button>}>
            <Space wrap>
              <Button onClick={() => onJump("posts")}>{text.articleManagement}</Button>
              <Button onClick={() => onJump("page:about")}>{text.sitePages}</Button>
              <Button onClick={() => onJump("assets")}>{text.resourceRepo}</Button>
              <Button onClick={() => onJump("repository")}>{text.versionControl}</Button>
            </Space>
            <Typography.Paragraph type="secondary" className="section-note">
              {culture === "en"
                ? "This shows the latest site state so you can verify the homepage content quickly."
                : "这里展示站点内容的最近状态，便于快速确认首页内容是否更新。"}
            </Typography.Paragraph>
          </Card>
        </Col>
        <Col span={12}>
          <Card title={text.sitePreview}>
            <div className="summary-grid">
              <div>
                <Typography.Text type="secondary">{text.defaultCulture}</Typography.Text>
                <div>{home?.site.defaultCulture ?? culture}</div>
              </div>
              <div>
                <Typography.Text type="secondary">{text.siteTitle}</Typography.Text>
                <div>{home?.site.appTitle ?? "-"}</div>
              </div>
              <div>
                <Typography.Text type="secondary">{text.assetsDir}</Typography.Text>
                <div>{home?.site.localAssetsDir ?? "-"}</div>
              </div>
              <div>
                <Typography.Text type="secondary">{text.record}</Typography.Text>
                <div>{home?.site.baiAn ?? "-"}</div>
              </div>
            </div>
          </Card>
        </Col>
      </Row>

      <Card title={text.recentPosts} extra={<Tag color="blue">{(home?.recentPosts ?? []).length} 篇</Tag>}>
        <CompactPostGrid culture={culture} items={home?.recentPosts ?? []} />
      </Card>

      <Card title={text.bannerPosts} extra={<Tag color="cyan">{(home?.bannerPosts ?? []).length} 篇</Tag>}>
        <CompactPostGrid culture={culture} items={home?.bannerPosts ?? []} />
      </Card>

      <Card title={text.recommendedTools}>
        <ToolGrid culture={culture} tools={home?.tools ?? []} />
      </Card>
    </div>
  );
}

function CompactPostGrid({ culture, items }: { culture: AdminLocale; items: BlogPostBrief[] }) {
  const list = items.slice(0, 3);
  if (list.length === 0) {
    return <Empty description={ADMIN_TEXT[culture].noPosts} />;
  }

  return (
    <div className="post-grid post-grid--three">
      {list.map((item) => (
        <article key={item.slug ?? item.title} className="post-tile">
          <Space size={6} wrap>
            {item.banner ? <Tag color="cyan">置顶</Tag> : null}
            {item.draft ? <Tag color="orange">草稿</Tag> : <Tag color="green">已发布</Tag>}
          </Space>
          <Typography.Title level={5} ellipsis={{ rows: 1, tooltip: item.title }}>
            {item.title}
          </Typography.Title>
          <Typography.Paragraph className="post-tile__desc" ellipsis={{ rows: 2, tooltip: item.description }}>
            {item.description || "暂无简介"}
          </Typography.Paragraph>
          <Typography.Text type="secondary">{formatDate(item.lastmod ?? item.date)}</Typography.Text>
        </article>
      ))}
    </div>
  );
}

function ToolGrid({ culture, tools }: { culture: AdminLocale; tools: ToolNode[] }) {
  const flattened = useMemo(() => flattenTools(tools).slice(0, 12), [tools]);
  if (flattened.length === 0) {
    return <Empty description={ADMIN_TEXT[culture].noTools} />;
  }

  return (
    <div className="tool-grid tool-grid--compact">
      {flattened.map((item) => (
        <article key={item.key} className="tool-tile">
          <div className="tool-tile__heading">
            <Typography.Title level={5} ellipsis={{ rows: 1, tooltip: item.name }}>
              {item.name}
            </Typography.Title>
            <Tag>{item.group}</Tag>
          </div>
          <Typography.Paragraph className="tool-tile__desc" ellipsis={{ rows: 2, tooltip: item.memo }}>
            {item.memo || "暂无说明"}
          </Typography.Paragraph>
        </article>
      ))}
    </div>
  );
}

type FlatTool = {
  key: string;
  group: string;
  name: string;
  memo: string;
};

function flattenTools(nodes: ToolNode[], group = "工具") {
  const items: FlatTool[] = [];
  for (const node of nodes) {
    if (node.hidden) {
      continue;
    }

    if (node.children && node.children.length > 0) {
      const nextGroup = node.name?.trim() || group;
      items.push(...flattenTools(node.children, nextGroup));
      continue;
    }

    items.push({
      key: node.slug ?? `${group}-${node.name ?? items.length}`,
      group,
      name: node.name ?? "未命名工具",
      memo: node.memo ?? ""
    });
  }
  return items;
}

function Posts() {
  const { message } = AntApp.useApp();
  const [loading, setLoading] = useState(false);
  const [keyword, setKeyword] = useState("");
  const [posts, setPosts] = useState<BlogPostBrief[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [editorOpen, setEditorOpen] = useState(false);
  const [editing, setEditing] = useState<BlogPost | null>(null);
  const [form] = Form.useForm<PostFormValues>();

  const load = async (pageIndex = page) => {
    setLoading(true);
    try {
      const result = await api.posts(pageIndex, keyword);
      setPosts(result.data);
      setTotal(result.total);
      setPage(result.pageIndex);
    } catch (error) {
      message.error(error instanceof Error ? error.message : "文章加载失败");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load(1);
  }, []);

  const columns: ColumnsType<BlogPostBrief> = [
    {
      title: "标题",
      dataIndex: "title",
      render: (value, record) => (
        <Space direction="vertical" size={0}>
          <Typography.Text strong ellipsis={{ tooltip: value }}>{value}</Typography.Text>
          <Typography.Text type="secondary">{record.slug}</Typography.Text>
        </Space>
      )
    },
    {
      title: "更新",
      width: 132,
      render: (_, record) => formatDate(record.lastmod ?? record.date)
    },
    {
      title: "状态",
      width: 150,
      render: (_, record) => (
        <Space wrap size={4}>
          {record.banner ? <Tag color="cyan">置顶</Tag> : null}
          {record.draft ? <Tag color="orange">草稿</Tag> : <Tag color="green">已发布</Tag>}
        </Space>
      )
    },
    {
      title: "操作",
      width: 120,
      render: (_, record) => (
        <Button icon={<EditOutlined />} onClick={() => openEdit(record.slug)}>
          编辑
        </Button>
      )
    }
  ];

  async function openEdit(slug?: string) {
    if (!slug) {
      return;
    }

    const post = await api.post(slug);
    setEditing(post);
    form.setFieldsValue(toFormValues(post));
    setEditorOpen(true);
  }

  function openCreate() {
    setEditing(null);
    form.setFieldsValue({
      title: "",
      slug: "",
      description: "",
      date: "",
      lastmod: "",
      cover: "",
      author: "",
      draft: true,
      banner: false,
      categoriesText: "",
      albumsText: "",
      tagsText: "",
      content: ""
    });
    setEditorOpen(true);
  }

  async function save() {
    const values = await form.validateFields();
    const payload = toPost(values);
    if (editing?.slug) {
      await api.updatePost(editing.slug, payload);
    } else {
      await api.createPost(payload);
    }
    message.success("保存成功");
    setEditorOpen(false);
    load();
  }

  function remove(slug?: string) {
    if (!slug) {
      return;
    }

    Modal.confirm({
      title: "删除文章",
      content: slug,
      okButtonProps: { danger: true },
      onOk: async () => {
        await api.deletePost(slug);
        message.success("已删除");
        load();
      }
    });
  }

  return (
    <Card
      title="文章管理"
      extra={
        <Space>
          <Input.Search
            allowClear
            placeholder="搜索标题 / slug / 标签"
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
            onSearch={() => load(1)}
            style={{ width: 300 }}
          />
          <Button icon={<ReloadOutlined />} onClick={() => load()}>
            刷新
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
            新建
          </Button>
        </Space>
      }
    >
      <Table
        rowKey={(record) => record.slug ?? record.title ?? ""}
        loading={loading}
        columns={columns}
        dataSource={posts}
        pagination={{ current: page, total, pageSize: 20, onChange: load, showSizeChanger: false }}
      />
      <Modal
        title={editing ? "编辑文章" : "新建文章"}
        open={editorOpen}
        width={1280}
        onCancel={() => setEditorOpen(false)}
        forceRender
        destroyOnClose
        footer={
          <Space>
            <Button onClick={() => setEditorOpen(false)}>取消</Button>
            <Button type="primary" icon={<SaveOutlined />} onClick={save}>
              保存
            </Button>
          </Space>
        }
      >
        <Form form={form} layout="vertical">
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="title" label="标题" rules={[{ required: true, message: "请输入标题" }]}>
                <Input />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="slug" label="Slug" rules={[{ required: true, message: "请输入 slug" }]}>
                <Input />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="description" label="简介">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="date" label="发布日期">
                <Input placeholder="2026-05-13T08:00:00Z" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="lastmod" label="更新时间">
                <Input placeholder="2026-05-13T08:00:00Z" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="author" label="作者">
                <Input />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="cover" label="封面">
                <Input />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="draft" label="草稿" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="banner" label="置顶" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="categoriesText" label="分类">
            <Input placeholder="用逗号分隔" />
          </Form.Item>
          <Form.Item name="albumsText" label="专题">
            <Input placeholder="用逗号分隔" />
          </Form.Item>
          <Form.Item name="tagsText" label="标签">
            <Input placeholder="用逗号分隔" />
          </Form.Item>
          <Form.Item name="content" label="Markdown 正文">
            <Input.TextArea rows={22} className="code-area" />
          </Form.Item>
          <Typography.Text type="secondary">文章编辑采用弹窗而不是右侧抽屉，避免长文内容被截断。</Typography.Text>
          <Button danger onClick={() => remove(editing?.slug)} className="post-delete-btn">
            删除当前文章
          </Button>
        </Form>
      </Modal>
    </Card>
  );
}

function SiteSettingsEditor() {
  const { message } = AntApp.useApp();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [settings, setSettings] = useState<SiteSettings | null>(null);
  const [form] = Form.useForm<SiteSettingsRequest>();

  const load = async () => {
    setLoading(true);
    try {
      const data = await api.siteSettings();
      setSettings(data);
      form.setFieldsValue({
        appTitle: data.appTitle,
        domain: data.domain,
        memo: data.memo,
        owner: data.owner,
        ownerDesc: data.ownerDesc,
        favicon: data.favicon,
        localAssetsDir: data.localAssetsDir,
        assetBaseUrl: data.assetBaseUrl,
        remoteAssetsRepository: data.remoteAssetsRepository,
        startYear: data.startYear,
        baiAn: data.baiAn,
        weChatName: data.weChatName,
        weChatImg: data.weChatImg,
        defaultCulture: data.defaultCulture,
        supportedCultures: data.supportedCultures
      });
    } catch (error) {
      message.error(error instanceof Error ? error.message : "站点设置加载失败");
      setSettings(null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const save = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      const result = await api.saveSiteSettings({
        ...values,
        supportedCultures: values.supportedCultures?.filter(Boolean)
      });
      if (!result.success) {
        message.error(result.message);
        return;
      }
      message.success(result.message);
      await load();
    } finally {
      setSaving(false);
    }
  };

  return (
    <Card
      title="站点设置"
      extra={
        <Space>
          <Button icon={<ReloadOutlined />} onClick={load}>
            刷新
          </Button>
          <Button type="primary" icon={<SaveOutlined />} loading={saving} onClick={save}>
            保存
          </Button>
        </Space>
      }
    >
      {loading ? <Spin /> : null}
      <Form form={form} layout="vertical" className={loading ? "admin-form-hidden" : undefined}>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="appTitle" label="站点标题">
                <Input />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="domain" label="站点域名">
                <Input />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="memo" label="站点描述">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="owner" label="作者">
                <Input />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="ownerDesc" label="作者简介">
                <Input />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="favicon" label="图标">
                <Input />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="remoteAssetsRepository" label="远程资源仓库">
                <Input />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="assetBaseUrl" label="资源基础地址">
                <Input />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="localAssetsDir" label="本地资源目录">
                <Input />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="startYear" label="起始年份">
                <InputNumber className="wide" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="defaultCulture" label="默认语言">
                <Select options={SITE_CULTURES.map((item) => ({ label: item.label, value: item.value }))} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="supportedCultures" label="支持语言">
                <Select mode="multiple" options={SITE_CULTURES.map((item) => ({ label: item.label, value: item.value }))} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="weChatName" label="微信名称">
                <Input />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="weChatImg" label="微信二维码">
                <Input />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="baiAn" label="备案号">
            <Input />
          </Form.Item>
          <Typography.Paragraph type="secondary">
            保存后会直接更新 `appsettings.json` 中的 `Site` 节点。
          </Typography.Paragraph>
          {settings ? <Typography.Text type="secondary">当前标题：{settings.appTitle}</Typography.Text> : null}
      </Form>
    </Card>
  );
}

function MarkdownEditor({ culture, spec }: { culture: string; spec: MarkdownEditorSpec }) {
  const { message } = AntApp.useApp();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [resource, setResource] = useState<EditableMarkdownResource | null>(null);
  const [draft, setDraft] = useState("");
  const editorRef = useRef<any>(null);
  const previewRef = useRef<HTMLDivElement | null>(null);
  const previewHtml = useMemo(() => renderMarkdownPreview(draft), [draft]);

  const load = async () => {
    setLoading(true);
    try {
      const data = await api.markdownResource(spec.name, culture);
      setResource(data);
      setDraft(data.markdown ?? "");
    } catch (error) {
      message.error(error instanceof Error ? error.message : "璧勬簮加载失败");
      setResource(null);
      setDraft("");
    } finally {
      setLoading(false);
    }
  };

  const syncPreviewScroll = () => {
    const editor = editorRef.current?.resizableTextArea?.textArea as HTMLTextAreaElement | undefined;
    const preview = previewRef.current;
    if (!editor || !preview) {
      return;
    }

    const maxEditor = Math.max(1, editor.scrollHeight - editor.clientHeight);
    const ratio = editor.scrollTop / maxEditor;
    preview.scrollTop = ratio * Math.max(1, preview.scrollHeight - preview.clientHeight);
  };

  const syncEditorScroll = () => {
    const editor = editorRef.current?.resizableTextArea?.textArea as HTMLTextAreaElement | undefined;
    const preview = previewRef.current;
    if (!editor || !preview) {
      return;
    }

    const maxPreview = Math.max(1, preview.scrollHeight - preview.clientHeight);
    const ratio = preview.scrollTop / maxPreview;
    editor.scrollTop = ratio * Math.max(1, editor.scrollHeight - editor.clientHeight);
  };

  useEffect(() => {
    load();
  }, [culture, spec.name]);

  const save = async () => {
    setSaving(true);
    try {
      const result = await api.saveMarkdownResource(spec.name, draft, culture);
      if (!result.success) {
        message.error(result.message ?? "保存失败");
        return;
      }
      message.success(result.message ?? "保存成功");
      await load();
    } finally {
      setSaving(false);
    }
  };

  return (
    <Card
      title={spec.label}
      extra={
        <Space>
          <Tooltip title={spec.pathHint}>
            <Tag>{culture}</Tag>
          </Tooltip>
          <Button icon={<ReloadOutlined />} onClick={load}>
            刷新
          </Button>
          <Button type="primary" icon={<SaveOutlined />} loading={saving} onClick={save}>
            保存
          </Button>
        </Space>
      }
    >
      <Typography.Paragraph type="secondary">{spec.description}</Typography.Paragraph>
      <Row gutter={16}>
        <Col span={12}>
          <Input.TextArea
            value={draft}
            onChange={(event) => setDraft(event.target.value)}
            onScroll={syncPreviewScroll}
            className="editor-textarea"
            autoSize={false}
            ref={editorRef}
          />
        </Col>
        <Col span={12}>
          <Card size="small" title="预览" className="preview-card">
            {loading ? (
              <Spin />
            ) : (
              <div ref={previewRef} className="markdown-preview-scroll rich-preview" onScroll={syncEditorScroll}>
                <RichPreview html={previewHtml} />
              </div>
            )}
          </Card>
        </Col>
      </Row>
    </Card>
  );
}

function JsonEditor({ culture, spec }: { culture: string; spec: JsonEditorSpec }) {
  if (spec.kind === "tree") {
    return <TreeJsonEditor culture={culture} spec={spec} />;
  }

  return <TableJsonEditor culture={culture} spec={spec} />;
}

function TableJsonEditor({ culture, spec }: { culture: string; spec: JsonEditorSpec }) {
  const { message } = AntApp.useApp();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [resource, setResource] = useState<EditableJsonResource | null>(null);
  const [rows, setRows] = useState<FlatJsonRow[]>([]);
  const [editingIndex, setEditingIndex] = useState<number | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [form] = Form.useForm();

  const load = async () => {
    setLoading(true);
    try {
      const data = await api.jsonResource(spec.name, culture);
      setResource(data);
      setRows(normalizeFlatRows(spec, parseFlatJsonRows(spec, data.json)));
    } catch (error) {
      message.error(error instanceof Error ? error.message : "JSON 加载失败");
      setResource(null);
      setRows([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [culture, spec.name]);

  const save = async (nextRows = rows) => {
    setSaving(true);
    try {
      const payload = JSON.stringify(serializeFlatRows(spec, normalizeFlatRows(spec, nextRows)), null, 2);
      const result = await api.saveJsonResource(spec.name, payload, culture);
      if (!result.success) {
        message.error(result.message ?? "保存失败");
        return;
      }

      message.success(result.message ?? "保存成功");
      await load();
    } finally {
      setSaving(false);
    }
  };

  const openCreate = () => {
    setEditingIndex(null);
    form.setFieldsValue(flatRowToFormValues(spec, emptyFlatRow(spec)));
    setModalOpen(true);
  };

  const openEdit = (index: number) => {
    setEditingIndex(index);
    form.setFieldsValue(flatRowToFormValues(spec, rows[index]));
    setModalOpen(true);
  };

  const remove = (index: number) => {
    const nextRows = rows.filter((_, current) => current !== index);
    setRows(nextRows);
    void save(nextRows);
  };

  const submit = async () => {
    const values = await form.validateFields();
    const nextRow = flatFormValuesToRow(spec, values, editingIndex === null ? undefined : rows[editingIndex]);
    const nextRows = editingIndex === null ? [...rows, nextRow] : rows.map((row, index) => (index === editingIndex ? nextRow : row));
    setRows(nextRows);
    setModalOpen(false);
    await save(nextRows);
  };

  return (
    <Card
      title={spec.label}
      extra={
        <Space>
          <Tag>{spec.pathHint}</Tag>
          <Button icon={<ReloadOutlined />} onClick={load}>
            刷新
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
            新增
          </Button>
        </Space>
      }
    >
      <Typography.Paragraph type="secondary">{spec.description}</Typography.Paragraph>
      {loading ? (
        <Spin />
      ) : (
        <Table
          rowKey={(record, index) => `${spec.name}-${index}`}
          dataSource={rows}
          pagination={false}
          size="small"
          columns={flatColumns(spec, openEdit, remove)}
        />
      )}
      {resource ? <Typography.Text type="secondary">资源文件：{resource.path}</Typography.Text> : null}
      <Modal
        open={modalOpen}
        title={editingIndex === null ? "新增" : "编辑"}
        onCancel={() => setModalOpen(false)}
        onOk={submit}
        forceRender
        destroyOnClose
      >
        <Form form={form} layout="vertical">
          {flatFormFields(spec)}
        </Form>
      </Modal>
    </Card>
  );
}

function TreeJsonEditor({ culture, spec }: { culture: string; spec: JsonEditorSpec }) {
  const { message } = AntApp.useApp();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [resource, setResource] = useState<EditableJsonResource | null>(null);
  const [nodes, setNodes] = useState<EditableTreeNode[]>([]);
  const [expandedKeys, setExpandedKeys] = useState<string[]>([]);
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [parentKey, setParentKey] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [form] = Form.useForm();

  const load = async () => {
    setLoading(true);
    try {
      const data = await api.jsonResource(spec.name, culture);
      const parsed = buildEditableTree(parseEditableTreeJson(data.json), spec.allowHidden ?? false);
      setResource(data);
      setNodes(parsed);
      setExpandedKeys(collectEditableTreeKeys(parsed));
    } catch (error) {
      message.error(error instanceof Error ? error.message : "JSON 加载失败");
      setResource(null);
      setNodes([]);
      setExpandedKeys([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [culture, spec.name]);

  const save = async (nextNodes = nodes) => {
    setSaving(true);
    try {
      const payload = JSON.stringify(stripEditableTree(nextNodes, spec.allowHidden ?? false), null, 2);
      const result = await api.saveJsonResource(spec.name, payload, culture);
      if (!result.success) {
        message.error(result.message ?? "保存失败");
        return;
      }

      message.success(result.message ?? "保存成功");
      await load();
    } finally {
      setSaving(false);
    }
  };

  const openCreateRoot = () => {
    setEditingKey(null);
    setParentKey(null);
    form.setFieldsValue(treeFormValues(emptyTreeRow(spec), spec.allowHidden ?? false));
    setModalOpen(true);
  };

  const openCreateChild = (key: string) => {
    setEditingKey(null);
    setParentKey(key);
    form.setFieldsValue(treeFormValues(emptyTreeRow(spec), spec.allowHidden ?? false));
    setModalOpen(true);
  };

  const openEdit = (key: string) => {
    const node = findEditableTreeNode(nodes, key);
    if (!node) {
      return;
    }

    setEditingKey(key);
    setParentKey(null);
    form.setFieldsValue(treeFormValues(node, spec.allowHidden ?? false));
    setModalOpen(true);
  };

  const remove = (key: string) => {
    const nextNodes = removeEditableTreeNode(nodes, key);
    setNodes(nextNodes);
    void save(nextNodes);
  };

  const submit = async () => {
    const values = await form.validateFields();
    const nextNode = formValuesToTreeNode(values, spec.allowHidden ?? false);

    let nextNodes = nodes;
    if (editingKey) {
      nextNodes = updateEditableTreeNode(nodes, editingKey, nextNode);
    } else if (parentKey) {
      nextNodes = insertEditableTreeNode(nodes, parentKey, nextNode);
    } else {
      nextNodes = [...nodes, nextNode];
    }

    nextNodes = buildEditableTree(nextNodes, spec.allowHidden ?? false);
    setNodes(nextNodes);
    setExpandedKeys(collectEditableTreeKeys(nextNodes));
    setModalOpen(false);
    await save(nextNodes);
  };

  return (
    <Card
      title={spec.label}
      extra={
        <Space>
          <Tag>{spec.pathHint}</Tag>
          <Button icon={<ReloadOutlined />} onClick={load}>
            刷新
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreateRoot}>
            新增根节点
          </Button>
          <Button type="primary" icon={<SaveOutlined />} loading={saving} onClick={() => save()}>
            保存
          </Button>
        </Space>
      }
    >
      <Typography.Paragraph type="secondary">
        这里按行维护树形资源，不直接编辑原始 JSON。
        {spec.allowHidden ? "隐藏状态也会保留。" : "子节点可以继续新增。"}
      </Typography.Paragraph>
      {loading ? (
        <Spin />
      ) : (
        <Table
          rowKey="key"
          dataSource={nodes}
          pagination={false}
          size="small"
          expandable={{
            defaultExpandAllRows: true,
            expandedRowKeys: expandedKeys,
            onExpandedRowsChange: (keys) => setExpandedKeys(keys.map((value) => String(value))),
            childrenColumnName: "children"
          }}
          columns={treeColumns(spec, openCreateChild, openEdit, remove)}
        />
      )}
      {resource ? <Typography.Text type="secondary">资源文件：{resource.path}</Typography.Text> : null}
      <Modal
        open={modalOpen}
        title={editingKey ? "编辑节点" : "新增节点"}
        onCancel={() => setModalOpen(false)}
        onOk={submit}
        forceRender
        destroyOnClose
      >
        <Form form={form} layout="vertical">
          <Form.Item name="name" label="名称" rules={[{ required: true, message: "请输入名称" }]}>
            <Input />
          </Form.Item>
          <Form.Item name="slug" label="Slug">
            <Input />
          </Form.Item>
          <Form.Item name="memo" label="说明">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item name="repository" label="来源/仓库">
            <Input />
          </Form.Item>
          {spec.allowHidden ? (
            <Form.Item name="hidden" label="隐藏" valuePropName="checked">
              <Switch />
            </Form.Item>
          ) : null}
        </Form>
      </Modal>
    </Card>
  );
}

type FlatJsonRow =
  | TaxonomyItem
  | FriendLinkItem
  | TimelineItem
  | SearchBlockedKeywordGroup;

type EditableTreeNode = {
  key: string;
  name?: string;
  memo?: string;
  slug?: string;
  repository?: string;
  hidden?: boolean;
  children?: EditableTreeNode[];
};

type TreeJsonNode = {
  name?: string;
  memo?: string;
  slug?: string;
  repository?: string;
  hidden?: boolean;
  children?: TreeJsonNode[];
};

type TreeRowFormValues = {
  name?: string;
  memo?: string;
  slug?: string;
  repository?: string;
  hidden?: boolean;
};

function parseFlatJsonRows(spec: JsonEditorSpec, json: string | null): FlatJsonRow[] {
  if (!json?.trim()) {
    return [];
  }

  try {
    const parsed = JSON.parse(json);
    if (!Array.isArray(parsed)) {
      return [];
    }

    return parsed as FlatJsonRow[];
  } catch {
    return [];
  }
}

function normalizeFlatRows(spec: JsonEditorSpec, rows: FlatJsonRow[]): FlatJsonRow[] {
  const next = rows.slice();
  if (spec.kind === "taxonomy") {
    return next
      .map((item) => ({
        sort: Number((item as Record<string, unknown>).sort ?? (item as Record<string, unknown>).Sort ?? 0) || 0,
        name: trimText((item as Record<string, unknown>).name ?? (item as Record<string, unknown>).Name),
        memo: trimText((item as Record<string, unknown>).memo ?? (item as Record<string, unknown>).Memo),
        slug: trimText((item as Record<string, unknown>).slug ?? (item as Record<string, unknown>).Slug),
        postCount: Number((item as Record<string, unknown>).postCount ?? (item as Record<string, unknown>).PostCount ?? 0) || 0
      }))
      .sort((left, right) => left.sort - right.sort);
  }

  if (spec.kind === "links") {
    return next
      .map((item) => ({
        Index: Number(((item as Record<string, unknown>).Index ?? (item as Record<string, unknown>).index) ?? 0) || 0,
        Title: trimText((item as Record<string, unknown>).Title ?? (item as Record<string, unknown>).title),
        Description: trimText((item as Record<string, unknown>).Description ?? (item as Record<string, unknown>).description),
        Link: trimText((item as Record<string, unknown>).Link ?? (item as Record<string, unknown>).link),
        Logo: trimText((item as Record<string, unknown>).Logo ?? (item as Record<string, unknown>).logo)
      }))
      .sort((left, right) => (left as FriendLinkItem).Index - (right as FriendLinkItem).Index) as FlatJsonRow[];
  }

  if (spec.kind === "timeline") {
    return next
      .map((item) => ({
        Time: parseDateInput(((item as Record<string, unknown>).Time ?? (item as Record<string, unknown>).time) as string | null | undefined),
        Title: trimText((item as Record<string, unknown>).Title ?? (item as Record<string, unknown>).title),
        Content: trimText((item as Record<string, unknown>).Content ?? (item as Record<string, unknown>).content)
      })) as FlatJsonRow[];
  }

  return next
    .map((item) => ({
      Sort: Number((item as Record<string, unknown>).Sort ?? (item as Record<string, unknown>).sort ?? 0) || 0,
      Name: trimText((item as Record<string, unknown>).Name ?? (item as Record<string, unknown>).name),
      Memo: trimText((item as Record<string, unknown>).Memo ?? (item as Record<string, unknown>).memo),
      Keywords: (((item as Record<string, unknown>).Keywords ?? (item as Record<string, unknown>).keywords) as unknown[])
        .filter((value) => Boolean(trimText(value)))
        .map((value) => trimText(value))
    }))
    .sort((left, right) => (left as SearchBlockedKeywordGroup).Sort - (right as SearchBlockedKeywordGroup).Sort);
}

function serializeFlatRows(spec: JsonEditorSpec, rows: FlatJsonRow[]) {
  if (spec.kind === "taxonomy") {
    return rows.map((item) => {
      const row = item as TaxonomyItem;
      return {
        sort: row.sort,
        name: row.name,
        memo: row.memo,
        slug: row.slug,
        postCount: row.postCount
      };
    });
  }

  if (spec.kind === "links") {
    return rows.map((item) => {
      const row = item as FriendLinkItem;
      return {
        Index: row.Index,
        Title: row.Title,
        Description: row.Description,
        Link: row.Link,
        Logo: row.Logo
      };
    });
  }

  if (spec.kind === "timeline") {
    return rows.map((item) => {
      const row = item as TimelineItem;
      return {
        Time: row.Time,
        Title: row.Title,
        Content: row.Content
      };
    });
  }

  return rows.map((item) => {
    const row = item as SearchBlockedKeywordGroup;
    return {
      Sort: row.Sort,
      Name: row.Name,
      Memo: row.Memo,
      Keywords: row.Keywords
    };
  });
}

function emptyFlatRow(spec: JsonEditorSpec): FlatJsonRow {
  if (spec.kind === "taxonomy") {
    return { sort: 0, name: "", memo: "", slug: "", postCount: 0 };
  }

  if (spec.kind === "links") {
    return { Index: 0, Title: "", Description: "", Link: "", Logo: "" };
  }

  if (spec.kind === "timeline") {
    return { Time: "", Title: "", Content: "" };
  }

  return { Sort: 0, Name: "", Memo: "", Keywords: [] };
}

function flatRowToFormValues(spec: JsonEditorSpec, row: FlatJsonRow): Record<string, string | number> {
  if (spec.kind === "taxonomy") {
    const taxonomy = row as Record<string, unknown>;
    return {
      sort: Number(taxonomy.sort ?? taxonomy.Sort ?? 0) || 0,
      name: trimText(taxonomy.name ?? taxonomy.Name),
      memo: trimText(taxonomy.memo ?? taxonomy.Memo),
      slug: trimText(taxonomy.slug ?? taxonomy.Slug)
    };
  }

  if (spec.kind === "links") {
    const link = row as FriendLinkItem;
    return { index: link.Index ?? 0, title: link.Title ?? "", description: link.Description ?? "", link: link.Link ?? "", logo: link.Logo ?? "" };
  }

  if (spec.kind === "timeline") {
    const timeline = row as TimelineItem;
    return { time: formatDateInput(timeline.Time), title: timeline.Title ?? "", content: timeline.Content ?? "" };
  }

  const blocked = row as SearchBlockedKeywordGroup;
  return {
    sort: blocked.Sort ?? 0,
    name: blocked.Name ?? "",
    memo: blocked.Memo ?? "",
    keywordsText: (blocked.Keywords ?? []).join("\n")
  };
}

function flatFormValuesToRow(spec: JsonEditorSpec, values: Record<string, unknown>, existing?: FlatJsonRow): FlatJsonRow {
  if (spec.kind === "taxonomy") {
    const current = existing as TaxonomyItem | undefined;
    return {
      sort: Number(values.sort ?? 0) || 0,
      name: trimText(values.name),
      memo: trimText(values.memo),
      slug: trimText(values.slug),
      postCount: current?.postCount ?? 0
    } as TaxonomyItem;
  }

  if (spec.kind === "links") {
    return {
      Index: Number(values.index ?? 0) || 0,
      Title: trimText(values.title),
      Description: trimText(values.description),
      Link: trimText(values.link),
      Logo: trimText(values.logo)
    } as FriendLinkItem;
  }

  if (spec.kind === "timeline") {
    return {
      Time: parseDateInput(trimText(values.time)),
      Title: trimText(values.title),
      Content: trimText(values.content)
    } as TimelineItem;
  }

  return {
    Sort: Number(values.sort ?? 0) || 0,
    Name: trimText(values.name),
    Memo: trimText(values.memo),
    Keywords: splitLines(values.keywordsText)
  } as SearchBlockedKeywordGroup;
}

function flatColumns(spec: JsonEditorSpec, onEdit: (index: number) => void, onDelete: (index: number) => void): ColumnsType<FlatJsonRow> {
  if (spec.kind === "taxonomy") {
    return [
      { title: "排序", width: 90, render: (_, record) => (record as TaxonomyItem).sort ?? 0 },
      { title: "名称", dataIndex: "name", render: (_, record) => (record as TaxonomyItem).name ?? "-" },
      { title: "Slug", dataIndex: "slug", render: (_, record) => (record as TaxonomyItem).slug ?? "-" },
      { title: "说明", dataIndex: "memo", render: (_, record) => (record as TaxonomyItem).memo ?? "-" },
      { title: "文章数", width: 96, render: (_, record) => (record as TaxonomyItem).postCount ?? 0 },
      {
        title: "操作",
        width: 160,
        render: (_, __, index) => (
          <Space>
            <Button icon={<EditOutlined />} onClick={() => onEdit(index)}>
              编辑
            </Button>
            <Button danger icon={<DeleteOutlined />} onClick={() => onDelete(index)}>
              删除
            </Button>
          </Space>
        )
      }
    ];
  }

  if (spec.kind === "links") {
    return [
      { title: "排序", width: 90, render: (_, record) => (record as FriendLinkItem).Index ?? 0 },
      { title: "标题", render: (_, record) => (record as FriendLinkItem).Title ?? "-" },
      { title: "地址", render: (_, record) => (record as FriendLinkItem).Link ?? "-" },
      { title: "LOGO", render: (_, record) => (record as FriendLinkItem).Logo ?? "-" },
      { title: "说明", render: (_, record) => (record as FriendLinkItem).Description ?? "-" },
      {
        title: "操作",
        width: 160,
        render: (_, __, index) => (
          <Space>
            <Button icon={<EditOutlined />} onClick={() => onEdit(index)}>
              编辑
            </Button>
            <Button danger icon={<DeleteOutlined />} onClick={() => onDelete(index)}>
              删除
            </Button>
          </Space>
        )
      }
    ];
  }

  if (spec.kind === "timeline") {
    return [
      { title: "时间", width: 180, render: (_, record) => formatDateTime((record as TimelineItem).Time) },
      { title: "标题", render: (_, record) => (record as TimelineItem).Title ?? "-" },
      { title: "内容", render: (_, record) => (record as TimelineItem).Content ?? "-" },
      {
        title: "操作",
        width: 160,
        render: (_, __, index) => (
          <Space>
            <Button icon={<EditOutlined />} onClick={() => onEdit(index)}>
              编辑
            </Button>
            <Button danger icon={<DeleteOutlined />} onClick={() => onDelete(index)}>
              删除
            </Button>
          </Space>
        )
      }
    ];
  }

  return [
    { title: "排序", width: 90, render: (_, record) => (record as SearchBlockedKeywordGroup).Sort ?? 0 },
    { title: "名称", render: (_, record) => (record as SearchBlockedKeywordGroup).Name ?? "-" },
    { title: "说明", render: (_, record) => (record as SearchBlockedKeywordGroup).Memo ?? "-" },
    { title: "关键词", render: (_, record) => ((record as SearchBlockedKeywordGroup).Keywords ?? []).join(", ") || "-" },
    {
      title: "操作",
      width: 160,
      render: (_, __, index) => (
        <Space>
          <Button icon={<EditOutlined />} onClick={() => onEdit(index)}>
            编辑
          </Button>
          <Button danger icon={<DeleteOutlined />} onClick={() => onDelete(index)}>
            删除
          </Button>
        </Space>
      )
    }
  ];
}

function flatFormFields(spec: JsonEditorSpec) {
  if (spec.kind === "taxonomy") {
    return (
      <>
        <Form.Item name="sort" label="排序" rules={[{ required: true, message: "请输入排序" }]}>
          <InputNumber className="wide" />
        </Form.Item>
        <Form.Item name="name" label="名称" rules={[{ required: true, message: "请输入名称" }]}>
          <Input />
        </Form.Item>
        <Form.Item name="slug" label="Slug" rules={[{ required: true, message: "请输入 Slug" }]}>
          <Input />
        </Form.Item>
        <Form.Item name="memo" label="说明">
          <Input.TextArea rows={3} />
        </Form.Item>
      </>
    );
  }

  if (spec.kind === "links") {
    return (
      <>
        <Form.Item name="index" label="排序" rules={[{ required: true, message: "请输入排序" }]}>
          <InputNumber className="wide" />
        </Form.Item>
        <Form.Item name="title" label="标题" rules={[{ required: true, message: "请输入标题" }]}>
          <Input />
        </Form.Item>
        <Form.Item name="link" label="链接" rules={[{ required: true, message: "请输入链接" }]}>
          <Input />
        </Form.Item>
        <Form.Item name="logo" label="图标地址">
          <Input />
        </Form.Item>
        <Form.Item name="description" label="说明">
          <Input.TextArea rows={3} />
        </Form.Item>
      </>
    );
  }

  if (spec.kind === "timeline") {
    return (
      <>
        <Form.Item name="time" label="时间">
          <Input placeholder="2026-05-13T08:00:00Z" />
        </Form.Item>
        <Form.Item name="title" label="标题" rules={[{ required: true, message: "请输入标题" }]}>
          <Input />
        </Form.Item>
        <Form.Item name="content" label="内容">
          <Input.TextArea rows={4} />
        </Form.Item>
      </>
    );
  }

  return (
    <>
      <Form.Item name="sort" label="排序" rules={[{ required: true, message: "请输入排序" }]}>
        <InputNumber className="wide" />
      </Form.Item>
      <Form.Item name="name" label="名称" rules={[{ required: true, message: "请输入名称" }]}>
        <Input />
      </Form.Item>
      <Form.Item name="memo" label="说明">
        <Input.TextArea rows={3} />
      </Form.Item>
      <Form.Item name="keywordsText" label="关键词">
        <Input.TextArea rows={4} placeholder="每行一个，或用逗号分隔" />
      </Form.Item>
    </>
  );
}

function parseEditableTreeJson(json: string | null): TreeJsonNode[] {
  if (!json?.trim()) {
    return [];
  }

  try {
    const parsed = JSON.parse(json);
    return Array.isArray(parsed) ? (parsed as TreeJsonNode[]) : [];
  } catch {
    return [];
  }
}

function buildEditableTree(nodes: TreeJsonNode[], allowHidden: boolean, prefix = ""): EditableTreeNode[] {
  return nodes.map((node, index) => {
    const key = `${prefix}${prefix ? "." : ""}${index}`;
    const raw = node as Record<string, unknown>;
    const children = (raw.children ?? raw.Children) as TreeJsonNode[] | undefined;
    return {
      key,
      name: trimText(raw.name ?? raw.Name),
      memo: trimText(raw.memo ?? raw.Memo),
      slug: trimText(raw.slug ?? raw.Slug),
      repository: trimText(raw.repository ?? raw.Repository),
      hidden: allowHidden ? Boolean(raw.hidden ?? raw.Hidden) : undefined,
      children: children?.length ? buildEditableTree(children, allowHidden, key) : undefined
    };
  });
}

function stripEditableTree(nodes: EditableTreeNode[], allowHidden: boolean): TreeJsonNode[] {
  return nodes.map((node) => ({
    name: trimText(node.name),
    memo: trimText(node.memo),
    slug: trimText(node.slug),
    repository: trimText(node.repository),
    hidden: allowHidden ? Boolean(node.hidden) : undefined,
    children: node.children?.length ? stripEditableTree(node.children, allowHidden) : undefined
  }));
}

function collectEditableTreeKeys(nodes: EditableTreeNode[], prefix = ""): string[] {
  const keys: string[] = [];
  nodes.forEach((node, index) => {
    const key = `${prefix}${prefix ? "." : ""}${index}`;
    keys.push(key);
    if (node.children?.length) {
      keys.push(...collectEditableTreeKeys(node.children, key));
    }
  });
  return keys;
}

function findEditableTreeNode(nodes: EditableTreeNode[], key: string): EditableTreeNode | null {
  for (const node of nodes) {
    if (node.key === key) {
      return node;
    }

    if (node.children?.length) {
      const found = findEditableTreeNode(node.children, key);
      if (found) {
        return found;
      }
    }
  }

  return null;
}

function updateEditableTreeNode(nodes: EditableTreeNode[], key: string, nextNode: EditableTreeNode): EditableTreeNode[] {
  return nodes.map((node) => {
    if (node.key === key) {
      return { ...nextNode, key, children: node.children };
    }

    return node.children?.length ? { ...node, children: updateEditableTreeNode(node.children, key, nextNode) } : node;
  });
}

function insertEditableTreeNode(nodes: EditableTreeNode[], parentKey: string, nextNode: EditableTreeNode): EditableTreeNode[] {
  return nodes.map((node) => {
    if (node.key === parentKey) {
      return { ...node, children: [...(node.children ?? []), nextNode] };
    }

    return node.children?.length ? { ...node, children: insertEditableTreeNode(node.children, parentKey, nextNode) } : node;
  });
}

function removeEditableTreeNode(nodes: EditableTreeNode[], key: string): EditableTreeNode[] {
  return nodes
    .filter((node) => node.key !== key)
    .map((node) => (node.children?.length ? { ...node, children: removeEditableTreeNode(node.children, key) } : node));
}

function treeColumns(
  spec: JsonEditorSpec,
  onCreateChild: (key: string) => void,
  onEdit: (key: string) => void,
  onDelete: (key: string) => void
): ColumnsType<EditableTreeNode> {
  return [
    {
      title: "名称",
      render: (_, record) => (
        <Space size={8}>
          <strong>{record.name ?? record.slug ?? "未命名节点"}</strong>
          {spec.allowHidden ? (record.hidden ? <Tag color="default">隐藏</Tag> : <Tag color="green">显示</Tag>) : null}
        </Space>
      )
    },
    {
      title: "Slug",
      width: 180,
      render: (_, record) => record.slug ?? "-"
    },
    {
      title: "来源/仓库",
      width: 220,
      render: (_, record) => record.repository ?? "-"
    },
    {
      title: "说明",
      render: (_, record) => record.memo ?? "-"
    },
    {
      title: "操作",
      width: 210,
      render: (_, record) => (
        <Space>
          <Button icon={<PlusOutlined />} onClick={() => onCreateChild(record.key)}>
            子项
          </Button>
          <Button icon={<EditOutlined />} onClick={() => onEdit(record.key)}>
            编辑
          </Button>
          <Button danger icon={<DeleteOutlined />} onClick={() => onDelete(record.key)}>
            删除
          </Button>
        </Space>
      )
    }
  ];
}

function treeFormValues(node: EditableTreeNode, allowHidden: boolean): Record<string, string | boolean> {
  return {
    name: node.name ?? "",
    memo: node.memo ?? "",
    slug: node.slug ?? "",
    repository: node.repository ?? "",
    hidden: allowHidden ? Boolean(node.hidden) : false
  };
}

function formValuesToTreeNode(values: Record<string, unknown>, allowHidden: boolean): EditableTreeNode {
  return {
    key: "",
    name: trimText(values.name),
    memo: trimText(values.memo),
    slug: trimText(values.slug),
    repository: trimText(values.repository),
    hidden: allowHidden ? Boolean(values.hidden) : undefined
  };
}

function emptyTreeRow(spec: JsonEditorSpec): EditableTreeNode {
  return {
    key: "",
    name: "",
    memo: "",
    slug: "",
    repository: "",
    hidden: spec.allowHidden ? false : undefined
  };
}

function AssetBrowser() {
  const { message } = AntApp.useApp();
  const [path, setPath] = useState("");
  const [data, setData] = useState<AssetDirectoryData | null>(null);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [selected, setSelected] = useState<AssetEntry | null>(null);
  const [selectedKeys, setSelectedKeys] = useState<Key[]>([]);
  const [preview, setPreview] = useState<GitFilePreviewResult | null>(null);
  const [previewLoading, setPreviewLoading] = useState(false);

  const loadDirectory = async (nextPath = path) => {
    setLoading(true);
    try {
      const directory = await api.assets(nextPath);
      setData(directory);
      setPath(nextPath);
      setSelected(null);
      setSelectedKeys([]);
      setPreview(null);
    } catch (error) {
      message.error(error instanceof Error ? error.message : "璧勬簮加载失败");
      setData(null);
    } finally {
      setLoading(false);
    }
  };

  const loadPreview = async (entry: AssetEntry) => {
    setSelected(entry);
    if (entry.isDirectory) {
      await loadDirectory(entry.path);
      return;
    }

    setPreviewLoading(true);
    try {
      setPreview(await api.gitFile(entry.path));
    } catch (error) {
      message.error(error instanceof Error ? error.message : "文件预览失败");
      setPreview(null);
    } finally {
      setPreviewLoading(false);
    }
  };

  useEffect(() => {
    loadDirectory("");
  }, []);

  const columns: ColumnsType<AssetEntry> = [
    {
      title: "名称",
      dataIndex: "name",
      render: (value, record) => (
        <Button type="link" style={{ padding: 0 }} onClick={() => loadPreview(record)}>
          <Space size={6}>
            {record.isDirectory ? <FolderOpenOutlined /> : <FileTextOutlined />}
            <span>{value}</span>
          </Space>
        </Button>
      )
    },
    {
      title: "类型",
      width: 96,
      render: (_, record) => <Tag color={record.isDirectory ? "blue" : "default"}>{record.isDirectory ? "目录" : "文件"}</Tag>
    },
    {
      title: "大小",
      width: 100,
      render: (_, record) => (record.isDirectory ? "-" : formatBytes(record.size))
    },
    {
      title: "更新时间",
      width: 180,
      render: (_, record) => (record.lastModified ? new Date(record.lastModified).toLocaleString() : "-")
    }
  ];

  const selectedFiles = useMemo(
    () => (data?.entries ?? []).filter((entry) => selectedKeys.includes(entry.path) && !entry.isDirectory),
    [data?.entries, selectedKeys]
  );

  return (
    <Row gutter={16}>
      <Col span={10}>
        <Card
          title="文件浏览"
          extra={
            <Space wrap>
              <Button disabled={!path} onClick={() => loadDirectory(parentPath(path))}>
                上一级
              </Button>
              <Button icon={<ReloadOutlined />} onClick={() => loadDirectory(path)}>
                刷新
              </Button>
              <Button
                danger
                disabled={selectedFiles.length === 0}
                onClick={() =>
                  Modal.confirm({
                    title: "批量删除文件",
                    content: `确定删除已选择的 ${selectedFiles.length} 个文件吗？此操作不可恢复。`,
                    okButtonProps: { danger: true },
                    onOk: async () => {
                      setUploading(true);
                      try {
                        for (const file of selectedFiles) {
                          const result = await api.deleteAsset(file.path);
                          if (!result.success) {
                            message.warning(result.message);
                          }
                        }
                        message.success("删除完成");
                        setSelectedKeys([]);
                        await loadDirectory(path);
                      } catch (error) {
                        message.error(error instanceof Error ? error.message : "删除失败");
                      } finally {
                        setUploading(false);
                      }
                    }
                  })
                }
              >
                批量删除
              </Button>
              <label className="upload-trigger">
                <input
                  type="file"
                  multiple
                  onChange={async (event) => {
                    const files = Array.from(event.target.files ?? []);
                    if (files.length === 0) {
                      return;
                    }

                    setUploading(true);
                    try {
                      for (const file of files) {
                        const result = await api.uploadAsset(path, file.name, file);
                        if (!result.success) {
                          message.warning(result.message);
                        }
                      }
                      message.success("上传完成");
                      await loadDirectory(path);
                    } catch (error) {
                      message.error(error instanceof Error ? error.message : "上传失败");
                    } finally {
                      setUploading(false);
                      event.currentTarget.value = "";
                    }
                  }}
                />
                <Button type="primary" loading={uploading} icon={<PlusOutlined />}>
                  上传
                </Button>
              </label>
            </Space>
          }
        >
          <Typography.Paragraph type="secondary">
            当前路径：{path || "/"}，点击目录进入，点击文件后在右侧预览。
          </Typography.Paragraph>
          <Table
            rowKey={(record) => record.path}
            loading={loading}
            columns={columns}
            dataSource={data?.entries ?? []}
            pagination={false}
            size="small"
            rowSelection={{
              selectedRowKeys: selectedKeys,
              getCheckboxProps: (record) => ({ disabled: record.isDirectory }),
              onChange: (keys) => setSelectedKeys(keys)
            }}
            onRow={(record) => ({ onClick: () => loadPreview(record) })}
          />
        </Card>
      </Col>
      <Col span={14}>
        <Card title="文件预览" className="preview-card">
          {previewLoading ? <Spin /> : <AssetPreviewPane entry={selected} preview={preview} />}
        </Card>
      </Col>
    </Row>
  );
}

function AssetPreviewPane({
  entry,
  preview
}: {
  entry: AssetEntry | null;
  preview: GitFilePreviewResult | null;
}) {
  if (!entry) {
    return <Empty description="请选择文件" />;
  }

  if (entry.isDirectory) {
    return <Empty description="目录没有预览内容" />;
  }

  if (!preview) {
    return <Empty description="暂无预览结果" />;
  }

  if (preview.kind === "image" && preview.dataUrl) {
    return (
      <Space direction="vertical" style={{ width: "100%" }} size={12}>
        <Typography.Text type="secondary">{preview.path}</Typography.Text>
        <Image src={preview.dataUrl} alt={preview.name} className="preview-image" />
      </Space>
    );
  }

  if (preview.kind === "text" && preview.textContent !== undefined) {
    return (
      <Space direction="vertical" style={{ width: "100%" }} size={12}>
        <Typography.Text type="secondary">
          {preview.path} {preview.mimeType ? `· ${preview.mimeType}` : ""}
        </Typography.Text>
        <pre className="terminal terminal--light preview-code">{preview.textContent}</pre>
      </Space>
    );
  }

  if (preview.kind === "binary") {
    return <Alert type="info" showIcon message="二进制文件" description={`大小：${formatBytes(preview.size ?? null)}`} />;
  }

  return <Empty description="文件不存在或无法预览" />;
}

function Repository() {
  const { message } = AntApp.useApp();
  const [status, setStatus] = useState<GitRepositoryStatusData | null>(null);
  const [log, setLog] = useState<GitCommandResult | null>(null);
  const [commitMessage, setCommitMessage] = useState("");
  const [selected, setSelected] = useState<GitChangeEntry | null>(null);
  const [preview, setPreview] = useState<GitFilePreviewResult | null>(null);
  const [previewLoading, setPreviewLoading] = useState(false);

  const load = async () => {
    try {
      const [statusData, logData] = await Promise.all([api.gitStatusDetails(), api.gitLog()]);
      setStatus(statusData);
      setLog(logData);
      const first = statusData.changes[0] ?? null;
      setSelected(first);
      if (first) {
        await loadPreview(first.path);
      } else {
        setPreview(null);
      }
    } catch (error) {
      message.error(error instanceof Error ? error.message : "版本控制加载失败");
    }
  };

  const loadPreview = async (path: string) => {
    setPreviewLoading(true);
    try {
      setPreview(await api.gitFile(path));
    } catch (error) {
      message.error(error instanceof Error ? error.message : "文件预览失败");
      setPreview(null);
    } finally {
      setPreviewLoading(false);
    }
  };

  const run = async (action: () => Promise<GitCommandResult>) => {
    try {
      const result = await action();
      if (result.success) {
        message.success("操作完成");
      } else {
        message.warning(result.error || "命令失败");
      }
      await load();
    } catch (error) {
      message.error(error instanceof Error ? error.message : "命令失败");
    }
  };

  useEffect(() => {
    load();
  }, []);

  const columns: ColumnsType<GitChangeEntry> = [
    {
      title: "状态",
      width: 96,
      render: (_, record) => <Tag color={statusColor(record.status)}>{record.status}</Tag>
    },
    {
      title: "文件",
      dataIndex: "path",
      render: (value, record) => (
        <Button type="link" style={{ padding: 0 }} onClick={() => { setSelected(record); loadPreview(record.path); }}>
          {value}
        </Button>
      )
    },
    {
      title: "原路径",
      width: 220,
      render: (_, record) => record.originalPath ?? "-"
    }
  ];

  return (
    <Tabs
      items={[
        {
          key: "status",
          label: "状态",
          children: (
            <Row gutter={16}>
              <Col span={10}>
                <Card
                  title="仓库状态"
                  extra={
                    <Space>
                      <Button icon={<ReloadOutlined />} onClick={load}>
                        刷新
                      </Button>
                      <Button icon={<CloudDownloadOutlined />} onClick={() => run(api.gitFetch)}>
                        抓取
                      </Button>
                      <Button icon={<SyncOutlined />} onClick={() => run(api.gitPull)}>
                        拉取
                      </Button>
                    </Space>
                  }
                >
                  <div className="summary-grid summary-grid--repo">
                    <div>
                      <Typography.Text type="secondary">分支</Typography.Text>
                      <div>{status?.branch || "-"}</div>
                    </div>
                    <div>
                      <Typography.Text type="secondary">变更数</Typography.Text>
                      <div>{status?.changeCount ?? 0}</div>
                    </div>
                  </div>
                  <Table
                    rowKey={(record) => `${record.status}-${record.path}`}
                    size="small"
                    columns={columns}
                    dataSource={status?.changes ?? []}
                    pagination={false}
                  />
                </Card>
              </Col>
              <Col span={14}>
                <Card title="变更文件预览" className="preview-card">
                  {previewLoading ? <Spin /> : <AssetPreviewPane entry={selected ? { name: selected.path, path: selected.path, isDirectory: false, size: null, lastModified: null } : null} preview={preview} />}
                </Card>
              </Col>
            </Row>
          )
        },
        {
          key: "commit",
          label: "提交",
          children: (
            <Card title="提交变更">
              <Space.Compact className="wide">
                <Input value={commitMessage} onChange={(event) => setCommitMessage(event.target.value)} placeholder="提交说明" />
                <Button type="primary" onClick={() => run(() => api.gitCommit(commitMessage))}>
                  提交
                </Button>
              </Space.Compact>
              <Typography.Paragraph type="secondary" className="section-note">
                提交前会自动执行 `git add -A`。
              </Typography.Paragraph>
            </Card>
          )
        },
        {
          key: "log",
          label: "日志",
          children: (
            <Row gutter={16}>
              <Col span={12}>
                <Card title="提交日志">
                  <pre className="terminal terminal--light">{log?.output || log?.error || "暂无日志"}</pre>
                </Card>
              </Col>
              <Col span={12}>
                <Card title="命令结果">
                  <pre className="terminal terminal--light">
                    {status ? `${status.branch || "unknown"}\n${status.changes.map((item) => `${item.status} ${item.path}`).join("\n")}` : "暂无状态"}
                  </pre>
                </Card>
              </Col>
            </Row>
          )
        }
      ]}
    />
  );
}

function RichPreview({ html }: { html: string }) {
  return <div className="rich-preview" dangerouslySetInnerHTML={{ __html: html }} />;
}

function renderMarkdownPreview(markdown: string) {
  const source = markdown ?? "";
  if (!source.trim()) {
    return "<p>暂无内容</p>";
  }

  const lines = source.replace(/\r\n/g, "\n").split("\n");
  const blocks: string[] = [];
  let index = 0;

  while (index < lines.length) {
    const trimmed = lines[index].trim();

    if (!trimmed) {
      index += 1;
      continue;
    }

    const fence = trimmed.match(/^```([\w-]+)?\s*$/);
    if (fence) {
      const language = fence[1] ? ` language-${escapeHtml(fence[1])}` : "";
      index += 1;
      const codeLines: string[] = [];
      while (index < lines.length && !/^```/.test(lines[index].trim())) {
        codeLines.push(lines[index]);
        index += 1;
      }
      if (index < lines.length) {
        index += 1;
      }
      blocks.push(`<pre><code class="${language.trim()}">${escapeHtml(codeLines.join("\n"))}</code></pre>`);
      continue;
    }

    const heading = trimmed.match(/^(#{1,6})\s+(.*)$/);
    if (heading) {
      const level = heading[1].length;
      blocks.push(`<h${level}>${renderInlineMarkdown(heading[2])}</h${level}>`);
      index += 1;
      continue;
    }

    if (/^>\s?/.test(trimmed)) {
      const quoteLines: string[] = [];
      while (index < lines.length && /^>\s?/.test(lines[index].trim())) {
        quoteLines.push(lines[index].trim().replace(/^>\s?/, ""));
        index += 1;
      }
      blocks.push(`<blockquote><p>${renderInlineMarkdown(quoteLines.join(" "))}</p></blockquote>`);
      continue;
    }

    if (/^(-|\*|\+)\s+/.test(trimmed)) {
      const items: string[] = [];
      while (index < lines.length && /^(-|\*|\+)\s+/.test(lines[index].trim())) {
        items.push(lines[index].trim().replace(/^(-|\*|\+)\s+/, ""));
        index += 1;
      }
      blocks.push(`<ul>${items.map((item) => `<li>${renderInlineMarkdown(item)}</li>`).join("")}</ul>`);
      continue;
    }

    if (/^\d+\.\s+/.test(trimmed)) {
      const items: string[] = [];
      while (index < lines.length && /^\d+\.\s+/.test(lines[index].trim())) {
        items.push(lines[index].trim().replace(/^\d+\.\s+/, ""));
        index += 1;
      }
      blocks.push(`<ol>${items.map((item) => `<li>${renderInlineMarkdown(item)}</li>`).join("")}</ol>`);
      continue;
    }

    const paragraphLines: string[] = [trimmed];
    index += 1;
    while (index < lines.length && lines[index].trim() && !isMarkdownBlockStart(lines[index].trim())) {
      paragraphLines.push(lines[index].trim());
      index += 1;
    }
    blocks.push(`<p>${renderInlineMarkdown(paragraphLines.join(" "))}</p>`);
  }

  return blocks.join("");
}

function isMarkdownBlockStart(line: string) {
  return (
    /^```/.test(line) ||
    /^(#{1,6})\s+/.test(line) ||
    /^>\s?/.test(line) ||
    /^(-|\*|\+)\s+/.test(line) ||
    /^\d+\.\s+/.test(line)
  );
}

function renderInlineMarkdown(text: string) {
  const escaped = escapeHtml(text);
  return escaped
    .replace(/`([^`]+)`/g, "<code>$1</code>")
    .replace(/!\[([^\]]*)\]\(([^)]+)\)/g, '<img src="$2" alt="$1" />')
    .replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" target="_blank" rel="noreferrer">$1</a>')
    .replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>")
    .replace(/\*([^*]+)\*/g, "<em>$1</em>")
    .replace(/__([^_]+)__/g, "<strong>$1</strong>")
    .replace(/_([^_]+)_/g, "<em>$1</em>")
    .replace(/\n/g, "<br />");
}

function escapeHtml(value: string) {
  return value
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

function toFormValues(post: BlogPost): PostFormValues {
  return {
    title: post.title,
    slug: post.slug,
    description: post.description,
    date: post.date ?? undefined,
    lastmod: post.lastmod ?? undefined,
    cover: post.cover,
    author: post.author,
    draft: post.draft,
    banner: post.banner,
    categoriesText: post.categories?.join(", ") ?? "",
    albumsText: post.albums?.join(", ") ?? "",
    tagsText: post.tags?.join(", ") ?? "",
    content: post.content ?? ""
  };
}

function toPost(values: PostFormValues): BlogPost {
  return {
    title: values.title?.trim(),
    slug: values.slug?.trim(),
    description: values.description?.trim(),
    date: values.date ? new Date(values.date).toISOString() : undefined,
    lastmod: values.lastmod ? new Date(values.lastmod).toISOString() : undefined,
    cover: values.cover?.trim(),
    author: values.author?.trim(),
    draft: Boolean(values.draft),
    banner: Boolean(values.banner),
    categories: splitList(values.categoriesText),
    albums: splitList(values.albumsText),
    tags: splitList(values.tagsText),
    content: values.content ?? ""
  };
}

function splitList(value?: string) {
  return value
    ?.split(",")
    .map((item) => item.trim())
    .filter(Boolean);
}

function parentPath(path: string) {
  if (!path) {
    return "";
  }

  const parts = path.split("/").filter(Boolean);
  parts.pop();
  return parts.join("/");
}

function formatBytes(size: number | null) {
  if (size === null || size === undefined) {
    return "-";
  }

  if (size < 1024) {
    return `${size} B`;
  }

  const units = ["KB", "MB", "GB", "TB"];
  let value = size / 1024;
  let index = 0;
  while (value >= 1024 && index < units.length - 1) {
    value /= 1024;
    index += 1;
  }

  return `${value.toFixed(value >= 10 ? 0 : 1)} ${units[index]}`;
}

function formatDate(value?: string | Date | null) {
  if (!value) {
    return "-";
  }

  const date = typeof value === "string" ? new Date(value) : value;
  if (Number.isNaN(date.getTime())) {
    return "-";
  }

  return date.toLocaleString();
}

function formatDateTime(value?: string | Date | null) {
  return formatDate(value);
}

function formatDateInput(value?: string | Date | null) {
  if (!value) {
    return "";
  }

  const date = typeof value === "string" ? new Date(value) : value;
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return date.toISOString();
}

function parseDateInput(value?: string | null) {
  if (!value?.trim()) {
    return undefined;
  }

  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? undefined : date.toISOString();
}

function trimText(value: unknown) {
  return typeof value === "string" ? value.trim() : value === null || value === undefined ? "" : String(value).trim();
}

function splitLines(value: unknown) {
  const text = trimText(value);
  if (!text) {
    return [];
  }

  return text
    .split(/[\n,]/)
    .map((item) => item.trim())
    .filter(Boolean);
}

function statusColor(status: string) {
  const first = status.trim().charAt(0);
  if (first === "A") {
    return "green";
  }
  if (first === "D") {
    return "red";
  }
  if (first === "R") {
    return "geekblue";
  }
  if (first === "C") {
    return "purple";
  }
  if (first === "U") {
    return "orange";
  }
  return "default";
}




