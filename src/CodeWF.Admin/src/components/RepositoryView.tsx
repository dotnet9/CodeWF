import { useEffect, useState } from "react";
import { App as AntApp, Alert, Button, Card, Col, Empty, Image, Input, Row, Space, Spin, Table, Tabs, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { CloudDownloadOutlined, ReloadOutlined, SyncOutlined } from "@ant-design/icons";
import {
  api,
  type AssetEntry,
  type GitChangeEntry,
  type GitCommandResult,
  type GitFilePreviewResult,
  type GitRepositoryStatusData
} from "../api";

export function RepositoryView({ canWrite }: { canWrite: boolean }) {
  const { message } = AntApp.useApp();
  const [status, setStatus] = useState<GitRepositoryStatusData | null>(null);
  const [log, setLog] = useState<GitCommandResult | null>(null);
  const [commitMessage, setCommitMessage] = useState("");
  const [selected, setSelected] = useState<GitChangeEntry | null>(null);
  const [preview, setPreview] = useState<GitFilePreviewResult | null>(null);
  const [previewLoading, setPreviewLoading] = useState(false);

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

  const run = async (action: () => Promise<GitCommandResult>) => {
    try {
      const result = await action();
      if (result.success) {
        message.success("操作完成");
      } else {
        message.warning(result.error || "命令执行失败");
      }
      await load();
    } catch (error) {
      message.error(error instanceof Error ? error.message : "命令执行失败");
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
        <Button
          type="link"
          style={{ padding: 0 }}
          onClick={() => {
            setSelected(record);
            loadPreview(record.path);
          }}
        >
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
                      <Button icon={<CloudDownloadOutlined />} onClick={() => run(api.gitFetch)} disabled={!canWrite}>
                        抓取
                      </Button>
                      <Button icon={<SyncOutlined />} onClick={() => run(api.gitPull)} disabled={!canWrite}>
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
                  {previewLoading ? (
                    <Spin />
                  ) : (
                    <RepositoryPreviewPane
                      entry={selected ? { name: selected.path, path: selected.path, isDirectory: false, size: null, lastModified: null } : null}
                      preview={preview}
                    />
                  )}
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
                <Button type="primary" onClick={() => run(() => api.gitCommit(commitMessage))} disabled={!canWrite}>
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

function RepositoryPreviewPane({
  entry,
  preview
}: {
  entry: AssetEntry | null;
  preview: GitFilePreviewResult | null;
}) {
  if (!entry) {
    return <Empty description="请选择文件" />;
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

function statusColor(status: string) {
  const first = status.trim().charAt(0);
  if (first === "A") {
    return "green";
  }
  if (first === "D") {
    return "red";
  }
  if (first === "M") {
    return "blue";
  }
  if (first === "R") {
    return "purple";
  }
  return "default";
}
