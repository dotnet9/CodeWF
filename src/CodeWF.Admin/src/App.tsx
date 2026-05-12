import { useEffect, useMemo, useState } from "react";
import {
  App as AntApp,
  Button,
  Card,
  Drawer,
  Form,
  Input,
  Layout,
  Menu,
  Modal,
  Space,
  Statistic,
  Switch,
  Table,
  Tabs,
  Tag,
  Tree,
  Typography
} from "antd";
import type { ColumnsType } from "antd/es/table";
import {
  AppstoreOutlined,
  BranchesOutlined,
  DashboardOutlined,
  EditOutlined,
  FileTextOutlined,
  PlusOutlined,
  ReloadOutlined,
  SaveOutlined
} from "@ant-design/icons";
import { api, type BlogPost, type BlogPostBrief, type GitCommandResult, type ToolNode } from "./api";

const { Header, Content, Sider } = Layout;

type ViewKey = "dashboard" | "posts" | "repository" | "tools";

export default function App() {
  const [view, setView] = useState<ViewKey>("dashboard");
  const [collapsed, setCollapsed] = useState(false);

  return (
    <AntApp>
      <Layout className="admin-shell">
        <Sider collapsible collapsed={collapsed} onCollapse={setCollapsed} theme="light" width={240}>
          <div className="admin-brand">CodeWF</div>
          <Menu
            mode="inline"
            selectedKeys={[view]}
            onClick={(event) => setView(event.key as ViewKey)}
            items={[
              { key: "dashboard", icon: <DashboardOutlined />, label: "概览" },
              { key: "posts", icon: <FileTextOutlined />, label: "文章管理" },
              { key: "repository", icon: <BranchesOutlined />, label: "资源仓库" },
              { key: "tools", icon: <AppstoreOutlined />, label: "工具目录" }
            ]}
          />
        </Sider>
        <Layout>
          <Header className="admin-header">
            <Typography.Title level={4}>后台管理</Typography.Title>
            <TokenInput />
          </Header>
          <Content className="admin-content">
            {view === "dashboard" ? <Dashboard /> : null}
            {view === "posts" ? <Posts /> : null}
            {view === "repository" ? <Repository /> : null}
            {view === "tools" ? <Tools /> : null}
          </Content>
        </Layout>
      </Layout>
    </AntApp>
  );
}

function TokenInput() {
  const [value, setValue] = useState(api.getToken());
  return (
    <Space.Compact>
      <Input.Password placeholder="Admin API Key" value={value} onChange={(event) => setValue(event.target.value)} />
      <Button icon={<SaveOutlined />} onClick={() => api.setToken(value)}>
        保存
      </Button>
    </Space.Compact>
  );
}

function Dashboard() {
  const [counts, setCounts] = useState<Record<string, number>>({});

  useEffect(() => {
    api.home().then((data) => setCounts(data.counts)).catch(() => setCounts({}));
  }, []);

  return (
    <div className="dashboard-grid">
      {[
        ["文章", counts.posts ?? 0],
        ["工具", counts.tools ?? 0],
        ["项目", counts.docs ?? 0],
        ["分类", counts.categories ?? 0]
      ].map(([label, value]) => (
        <Card key={label}>
          <Statistic title={label} value={value} />
        </Card>
      ))}
    </div>
  );
}

function Posts() {
  const { message } = AntApp.useApp();
  const [loading, setLoading] = useState(false);
  const [keyword, setKeyword] = useState("");
  const [posts, setPosts] = useState<BlogPostBrief[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [editing, setEditing] = useState<BlogPost | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [form] = Form.useForm<BlogPost>();

  const load = async (pageIndex = page) => {
    setLoading(true);
    try {
      const result = await api.posts(pageIndex, keyword);
      setPosts(result.data);
      setTotal(result.total);
      setPage(result.pageIndex);
    } catch (error) {
      message.error(error instanceof Error ? error.message : "加载失败");
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
        <Space direction="vertical" size={2}>
          <strong>{value}</strong>
          <Typography.Text type="secondary">{record.slug}</Typography.Text>
        </Space>
      )
    },
    {
      title: "日期",
      dataIndex: "date",
      width: 130,
      render: (value) => value?.slice(0, 10)
    },
    {
      title: "状态",
      width: 110,
      render: (_, record) => (
        <Space>
          {record.draft ? <Tag color="orange">草稿</Tag> : <Tag color="green">发布</Tag>}
          {record.banner ? <Tag color="cyan">Banner</Tag> : null}
        </Space>
      )
    },
    {
      title: "操作",
      width: 160,
      render: (_, record) => (
        <Space>
          <Button icon={<EditOutlined />} onClick={() => openEdit(record.slug)}>
            编辑
          </Button>
          <Button danger onClick={() => remove(record.slug)}>
            删除
          </Button>
        </Space>
      )
    }
  ];

  async function openEdit(slug?: string) {
    if (!slug) {
      return;
    }
    const post = await api.post(slug);
    setEditing(post);
    form.setFieldsValue({ ...post, content: post.content ?? "" });
    setDrawerOpen(true);
  }

  function openCreate() {
    const today = new Date().toISOString();
    const next: BlogPost = { title: "", slug: "", description: "", date: today, lastmod: today, draft: true, banner: false, content: "" };
    setEditing(null);
    form.setFieldsValue(next);
    setDrawerOpen(true);
  }

  async function save() {
    const values = await form.validateFields();
    const payload: BlogPost = normalizePost(values);
    if (editing?.slug) {
      await api.updatePost(editing.slug, payload);
    } else {
      await api.createPost(payload);
    }
    message.success("已保存");
    setDrawerOpen(false);
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
          <Input.Search placeholder="标题、slug、标签" value={keyword} onChange={(event) => setKeyword(event.target.value)} onSearch={() => load(1)} />
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
        pagination={{ current: page, total, pageSize: 20, onChange: load }}
      />
      <Drawer title={editing ? "编辑文章" : "新建文章"} open={drawerOpen} width={880} onClose={() => setDrawerOpen(false)} extra={<Button type="primary" onClick={save}>保存</Button>}>
        <Form form={form} layout="vertical">
          <Form.Item name="title" label="标题" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="slug" label="Slug">
            <Input />
          </Form.Item>
          <Form.Item name="description" label="摘要">
            <Input.TextArea rows={3} />
          </Form.Item>
          <div className="form-grid">
            <Form.Item name="date" label="发布日期">
              <Input />
            </Form.Item>
            <Form.Item name="lastmod" label="更新时间">
              <Input />
            </Form.Item>
            <Form.Item name="author" label="作者">
              <Input />
            </Form.Item>
          </div>
          <Form.Item name="cover" label="封面">
            <Input />
          </Form.Item>
          <div className="form-grid">
            <Form.Item name="draft" label="草稿" valuePropName="checked">
              <Switch />
            </Form.Item>
            <Form.Item name="banner" label="Banner" valuePropName="checked">
              <Switch />
            </Form.Item>
          </div>
          <Form.Item name="categories" label="分类">
            <Input placeholder="逗号分隔" />
          </Form.Item>
          <Form.Item name="albums" label="专题">
            <Input placeholder="逗号分隔" />
          </Form.Item>
          <Form.Item name="tags" label="标签">
            <Input placeholder="逗号分隔" />
          </Form.Item>
          <Form.Item name="content" label="Markdown 正文">
            <Input.TextArea rows={18} className="code-area" />
          </Form.Item>
        </Form>
      </Drawer>
    </Card>
  );
}

function Repository() {
  const { message } = AntApp.useApp();
  const [status, setStatus] = useState<GitCommandResult | null>(null);
  const [log, setLog] = useState<GitCommandResult | null>(null);
  const [commitMessage, setCommitMessage] = useState("");

  async function run(action: () => Promise<GitCommandResult>) {
    try {
      const result = await action();
      setStatus(result);
      if (result.success) {
        message.success("操作完成");
      } else {
        message.warning(result.error || "命令失败");
      }
    } catch (error) {
      message.error(error instanceof Error ? error.message : "操作失败");
    }
  }

  useEffect(() => {
    api.gitStatus().then(setStatus).catch(() => undefined);
    api.gitLog().then(setLog).catch(() => undefined);
  }, []);

  return (
    <Tabs
      items={[
        {
          key: "status",
          label: "状态",
          children: (
            <Card
              extra={
                <Space>
                  <Button onClick={() => run(api.gitStatus)}>刷新</Button>
                  <Button onClick={() => run(api.gitFetch)}>Fetch</Button>
                  <Button onClick={() => run(api.gitPull)}>Pull</Button>
                </Space>
              }
            >
              <pre className="terminal">{status?.output || status?.error || "No status."}</pre>
            </Card>
          )
        },
        {
          key: "commit",
          label: "提交",
          children: (
            <Card>
              <Space.Compact className="wide">
                <Input value={commitMessage} onChange={(event) => setCommitMessage(event.target.value)} placeholder="资源仓库提交说明" />
                <Button type="primary" onClick={() => run(() => api.gitCommit(commitMessage))}>
                  Commit
                </Button>
              </Space.Compact>
            </Card>
          )
        },
        {
          key: "log",
          label: "日志",
          children: <Card><pre className="terminal">{log?.output || log?.error || "No log."}</pre></Card>
        }
      ]}
    />
  );
}

function Tools() {
  const [tools, setTools] = useState<ToolNode[]>([]);

  useEffect(() => {
    api.tools().then(setTools).catch(() => setTools([]));
  }, []);

  const treeData = useMemo(() => toTreeData(tools), [tools]);
  return (
    <Card title="工具目录">
      <Tree treeData={treeData} defaultExpandAll />
    </Card>
  );
}

function normalizePost(values: BlogPost): BlogPost {
  return {
    ...values,
    date: values.date ? new Date(values.date).toISOString() : undefined,
    lastmod: values.lastmod ? new Date(values.lastmod).toISOString() : undefined,
    categories: splitList(values.categories),
    albums: splitList(values.albums),
    tags: splitList(values.tags)
  };
}

function splitList(value: string[] | string | undefined) {
  if (Array.isArray(value)) {
    return value;
  }
  return value?.split(",").map((item) => item.trim()).filter(Boolean) ?? [];
}

function toTreeData(nodes: ToolNode[]) {
  return nodes.map((node) => ({
    title: node.name ?? node.slug,
    key: node.slug ?? node.name ?? crypto.randomUUID(),
    children: node.children?.map((child) => ({
      title: `${child.name} (${child.slug})`,
      key: child.slug ?? child.name ?? crypto.randomUUID()
    }))
  }));
}
