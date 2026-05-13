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
  Tree,
  Tooltip,
  Typography
} from "antd";
import { ConfigProvider } from "antd";
import enUS from "antd/locale/en_US";
import zhCN from "antd/locale/zh_CN";
import type { ColumnsType } from "antd/es/table";
import {
  AppstoreOutlined,
  BookOutlined,
  BranchesOutlined,
  CloudDownloadOutlined,
  DashboardOutlined,
  EditOutlined,
  FileTextOutlined,
  FolderOpenOutlined,
  LogoutOutlined,
  PlusOutlined,
  ReloadOutlined,
  SaveOutlined,
  SyncOutlined
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
  type SiteSettings,
  type SiteSettingsRequest,
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
    sitePreview: "Site preview",
    defaultCulture: "Default locale",
    siteTitle: "Site title",
    assetsDir: "Assets directory",
    record: "备案号",
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

type ViewKey = "overview" | "posts" | "pages" | "resources" | "site" | "repository";
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

type EditorSpec = {
  name: string;
  label: string;
  description: string;
  pathHint: string;
};

const MARKDOWN_PAGES: EditorSpec[] = [
  { name: "about", label: "关于", description: "站点简介和作者信息。", pathHint: "site/about.md" },
  { name: "donation", label: "赞赏", description: "赞赏页面内容。", pathHint: "site/pays/Donation.md" },
  { name: "privacy", label: "隐私", description: "隐私政策页面。", pathHint: "site/Privacy.md" }
];

const JSON_RESOURCES: EditorSpec[] = [
  { name: "friend-links", label: "友情链接", description: "页脚友情链接配置。", pathHint: "site/friend-links.json" },
  { name: "timelines", label: "时间线", description: "站点时间线条目。", pathHint: "site/timelines.json" },
  { name: "tools", label: "工具目录", description: "工具分类树。", pathHint: "site/tools/tools.json" },
  { name: "navigation", label: "文档导航", description: "项目和文档树。", pathHint: "site/doc/navigation.json" },
  { name: "blocked-search-keywords", label: "屏蔽词", description: "屏蔽搜索关键词分组。", pathHint: "site/blocked-search-keywords.json" },
  { name: "lang", label: "词典", description: "站点本地化词条。", pathHint: "site/lang.json" },
  { name: "categories", label: "分类", description: "分类定义。", pathHint: "site/categories.json" },
  { name: "albums", label: "专题", description: "专题定义。", pathHint: "site/albums.json" }
];

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
    <ConfigProvider locale={antdLocale} theme={{ token: { colorPrimary: "#0f766e", borderRadius: 8 } }}>
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
  const text = ADMIN_TEXT[culture];

  return (
    <Layout className="admin-shell">
      <Sider collapsible collapsed={collapsed} onCollapse={setCollapsed} theme="light" width={252} className="admin-sider">
        <div className="admin-brand">
          <img className="brand-icon" src={SITE_LOGO_URL} alt="" />
          <span>{collapsed ? "CW" : text.workspaceTitle}</span>
        </div>
        <Menu
          mode="inline"
          selectedKeys={[view]}
          onClick={(event) => setView(event.key as ViewKey)}
          items={[
            { key: "overview", icon: <DashboardOutlined />, label: text.overview },
            { key: "posts", icon: <FileTextOutlined />, label: text.posts },
            { key: "pages", icon: <BookOutlined />, label: text.pages },
            { key: "resources", icon: <AppstoreOutlined />, label: text.resources },
            { key: "site", icon: <DashboardOutlined />, label: text.site },
            { key: "repository", icon: <BranchesOutlined />, label: text.repository }
          ]}
        />
      </Sider>
      <Layout className="admin-main">
        <Header className="admin-header">
          <div className="admin-header__title">
            <Typography.Title level={4}>{text.workspaceTitle}</Typography.Title>
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
            {view === "overview" ? <Overview culture={culture} onJump={setView} /> : null}
            {view === "posts" ? <Posts /> : null}
            {view === "pages" ? <MarkdownPages culture={culture} /> : null}
            {view === "resources" ? <ResourceRepo culture={culture} /> : null}
            {view === "site" ? <SiteSettingsEditor /> : null}
            {view === "repository" ? <Repository /> : null}
          </div>
        </Content>
      </Layout>
    </Layout>
  );
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
              <Button onClick={() => onJump("pages")}>{text.sitePages}</Button>
              <Button onClick={() => onJump("resources")}>{text.resourceRepo}</Button>
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

function MarkdownPages({ culture }: { culture: string }) {
  return (
    <Tabs
      items={MARKDOWN_PAGES.map((item) => ({
        key: item.name,
        label: item.label,
        children: <MarkdownEditor culture={culture} spec={item} />
      }))}
    />
  );
}

function ResourceRepo({ culture }: { culture: string }) {
  return (
    <Tabs
      items={[
        {
          key: "json",
          label: "JSON 资源",
          children: <JsonEditors culture={culture} />
        },
        {
          key: "tools",
          label: "工具树",
          children: <ToolTreePreview culture={culture} />
        },
        {
          key: "assets",
          label: "资源仓库",
          children: <AssetBrowser />
        }
      ]}
    />
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
      {loading ? (
        <Spin />
      ) : (
        <Form form={form} layout="vertical">
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
                <Select options={CULTURES.map((item) => ({ label: item.label, value: item.value }))} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="supportedCultures" label="支持语言">
                <Select mode="multiple" options={CULTURES.map((item) => ({ label: item.label, value: item.value }))} />
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
      )}
    </Card>
  );
}

function MarkdownEditor({ culture, spec }: { culture: string; spec: EditorSpec }) {
  const { message } = AntApp.useApp();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [resource, setResource] = useState<EditableMarkdownResource | null>(null);
  const [draft, setDraft] = useState("");
  const editorRef = useRef<any>(null);
  const previewRef = useRef<HTMLDivElement | null>(null);

  const load = async () => {
    setLoading(true);
    try {
      const data = await api.markdownResource(spec.name, culture);
      setResource(data);
      setDraft(data.markdown ?? "");
    } catch (error) {
      message.error(error instanceof Error ? error.message : "资源加载失败");
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
                <RichPreview html={resource?.htmlContent ?? "<p>暂无内容</p>"} />
              </div>
            )}
          </Card>
        </Col>
      </Row>
    </Card>
  );
}

function JsonEditors({ culture }: { culture: string }) {
  return (
    <div className="resource-stack">
      {JSON_RESOURCES.map((item) => (
        <JsonEditor key={item.name} culture={culture} spec={item} />
      ))}
    </div>
  );
}

function JsonEditor({ culture, spec }: { culture: string; spec: EditorSpec }) {
  const { message } = AntApp.useApp();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [resource, setResource] = useState<EditableJsonResource | null>(null);
  const [draft, setDraft] = useState("");
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await api.jsonResource(spec.name, culture);
      setResource(data);
      setDraft(data.json ?? "");
    } catch (err) {
      setError(err instanceof Error ? err.message : "JSON 加载失败");
      setResource(null);
      setDraft("");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [culture, spec.name]);

  const save = async () => {
    setSaving(true);
    try {
      const result = await api.saveJsonResource(spec.name, draft, culture);
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

  const preview = useMemo(() => {
    if (!draft.trim()) {
      return "[]";
    }

    try {
      return JSON.stringify(JSON.parse(draft), null, 2);
    } catch (err) {
      return err instanceof Error ? err.message : "无效 JSON";
    }
  }, [draft]);

  return (
    <Card
      title={spec.label}
      extra={
        <Space>
          <Tag>{spec.pathHint}</Tag>
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
      {error ? <Alert type="error" showIcon message="加载错误" description={error} style={{ marginBottom: 16 }} /> : null}
      <Row gutter={16}>
        <Col span={12}>
          <Input.TextArea value={draft} onChange={(event) => setDraft(event.target.value)} className="editor-textarea" autoSize={false} />
        </Col>
        <Col span={12}>
          <Card size="small" title="解析预览" className="preview-card">
            {loading ? <Spin /> : <pre className="terminal terminal--light">{preview}</pre>}
          </Card>
        </Col>
      </Row>
      {resource ? <Typography.Text type="secondary">资源文件：{resource.path}</Typography.Text> : null}
    </Card>
  );
}

function ToolTreePreview({ culture }: { culture: string }) {
  const { message } = AntApp.useApp();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [resource, setResource] = useState<EditableJsonResource | null>(null);
  const [tree, setTree] = useState<EditableToolNode[]>([]);
  const [checkedKeys, setCheckedKeys] = useState<string[]>([]);
  const [expandedKeys, setExpandedKeys] = useState<string[]>([]);
  const [preview, setPreview] = useState("");

  const load = async () => {
    setLoading(true);
    try {
      const data = await api.jsonResource("tools", culture);
      const parsed = normalizeToolTree(parseToolTree(data.json));
      setResource(data);
      setTree(buildEditableToolTree(parsed));
      setCheckedKeys(collectVisibleKeys(parsed));
      setExpandedKeys(collectTreeKeys(parsed));
      setPreview(formatToolTree(parsed));
    } catch (error) {
      message.error(error instanceof Error ? error.message : "工具树加载失败");
      setResource(null);
      setTree([]);
      setCheckedKeys([]);
      setExpandedKeys([]);
      setPreview("");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [culture]);

  const save = async () => {
    setSaving(true);
    try {
      const payload = formatToolTree(stripEditableToolTree(tree));
      const result = await api.saveJsonResource("tools", payload, culture);
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

  const onCheck = (keys: unknown) => {
    const next = Array.isArray(keys) ? keys : (keys as { checked?: unknown[] }).checked ?? [];
    const visibleKeys = next.map((value) => String(value));
    const nextTree = applyToolVisibility(tree, new Set(visibleKeys));
    setTree(nextTree);
    setCheckedKeys(collectVisibleKeys(nextTree));
    setPreview(formatToolTree(stripEditableToolTree(nextTree)));
  };

  const onExpand = (keys: unknown) => {
    const next = Array.isArray(keys) ? keys : [];
    setExpandedKeys(next.map((value) => String(value)));
  };

  return (
    <Card
      title="工具树"
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
      <Typography.Paragraph type="secondary">
        勾选表示前台可见，取消勾选后会保存为 `hidden: true`，前台工具目录和搜索都会忽略它。
      </Typography.Paragraph>
      {loading ? (
        <Spin />
      ) : (
        <Row gutter={16}>
          <Col span={12}>
            <Tree
              checkable
              selectable={false}
              blockNode
              checkedKeys={checkedKeys}
              expandedKeys={expandedKeys}
              onCheck={onCheck}
              onExpand={onExpand}
              treeData={tree.map(toTreeData)}
            />
          </Col>
          <Col span={12}>
            <Card size="small" title="JSON 预览" className="preview-card">
              <pre className="terminal terminal--light preview-code">{preview || resource?.json || "[]"}</pre>
            </Card>
          </Col>
        </Row>
      )}
    </Card>
  );
}

type EditableToolNode = ToolNode & {
  key: string;
  children?: EditableToolNode[];
};

type ToolTreeData = {
  key: string;
  title: ReactNode;
  children?: ToolTreeData[];
};

function parseToolTree(json: string | null): ToolNode[] {
  if (!json?.trim()) {
    return [];
  }

  try {
    const parsed = JSON.parse(json);
    return Array.isArray(parsed) ? (parsed as ToolNode[]) : [];
  } catch {
    return [];
  }
}

function buildEditableToolTree(nodes: ToolNode[], prefix = ""): EditableToolNode[] {
  return nodes.map((node, index) => {
    const key = `${prefix}${prefix ? "." : ""}${index}`;
    return {
      ...node,
      key,
      children: node.children?.length ? buildEditableToolTree(node.children, key) : undefined
    };
  });
}

function stripEditableToolTree(nodes: EditableToolNode[]): ToolNode[] {
  return nodes.map((node) => ({
    name: node.name,
    memo: node.memo,
    slug: node.slug,
    repository: node.repository,
    hidden: node.hidden,
    children: node.children?.length ? stripEditableToolTree(node.children) : undefined
  }));
}

function applyToolVisibility(nodes: EditableToolNode[], visibleKeys: Set<string>): EditableToolNode[] {
  return nodes.map((node) => {
    const children = node.children?.length ? applyToolVisibility(node.children, visibleKeys) : undefined;
    const hasVisibleChild = children?.some((child) => !child.hidden) ?? false;
    const selfVisible = visibleKeys.has(node.key);

    return {
      ...node,
      hidden: !(selfVisible || hasVisibleChild),
      children
    };
  });
}

function collectVisibleKeys(nodes: ToolNode[], prefix = ""): string[] {
  const keys: string[] = [];
  nodes.forEach((node, index) => {
    const key = `${prefix}${prefix ? "." : ""}${index}`;
    if (!node.hidden) {
      keys.push(key);
    }
    if (node.children?.length) {
      keys.push(...collectVisibleKeys(node.children, key));
    }
  });
  return keys;
}

function collectTreeKeys(nodes: ToolNode[], prefix = ""): string[] {
  const keys: string[] = [];
  nodes.forEach((node, index) => {
    const key = `${prefix}${prefix ? "." : ""}${index}`;
    keys.push(key);
    if (node.children?.length) {
      keys.push(...collectTreeKeys(node.children, key));
    }
  });
  return keys;
}

function toTreeData(node: EditableToolNode): ToolTreeData {
  return {
    key: node.key,
    title: (
      <Space size={8}>
        <span>{node.name ?? node.slug ?? "未命名节点"}</span>
        {node.hidden ? <Tag color="default">隐藏</Tag> : <Tag color="green">显示</Tag>}
        {!node.name && node.slug ? <Tag color="blue">Slug</Tag> : null}
        {!node.name && !node.slug && node.repository ? <Typography.Text type="secondary">来源：{node.repository}</Typography.Text> : null}
        {node.repository && node.name ? <Typography.Text type="secondary">{node.repository}</Typography.Text> : null}
      </Space>
    ),
    children: node.children?.map(toTreeData)
  };
}

function formatToolTree(nodes: ToolNode[]) {
  return JSON.stringify(nodes, null, 2);
}

function normalizeToolTree(nodes: ToolNode[]): ToolNode[] {
  return nodes.map((node) => {
    const children = node.children?.length ? normalizeToolTree(node.children) : undefined;
    const hasVisibleChild = children?.some((child) => !child.hidden) ?? false;
    return {
      ...node,
      hidden: node.hidden && !hasVisibleChild ? true : false,
      children
    };
  });
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
      message.error(error instanceof Error ? error.message : "资源加载失败");
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
                  <pre className="terminal terminal--light">{log?.output || log?.error || "暂无日志。"}</pre>
                </Card>
              </Col>
              <Col span={12}>
                <Card title="命令结果">
                  <pre className="terminal terminal--light">
                    {status ? `${status.branch || "unknown"}\n${status.changes.map((item) => `${item.status} ${item.path}`).join("\n")}` : "暂无状态。"}
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
