import { useState, type ReactNode } from "react";
import { Button, Layout, Menu, type MenuProps, Select, Space, Typography } from "antd";
import { LogoutOutlined } from "@ant-design/icons";

const { Header, Content, Sider } = Layout;

type CultureOption<TCulture extends string> = {
  label: string;
  value: TCulture;
};

export function WorkspaceShell<TCulture extends string, TView extends string>({
  activeTitle,
  brandTitle,
  brandSubtitle,
  children,
  culture,
  cultures,
  logoUrl,
  logoutLabel,
  menuItems,
  openKeys,
  selectedView,
  onCultureChange,
  onLogout,
  onOpenKeysChange,
  onViewChange
}: {
  activeTitle: string;
  brandTitle: string;
  brandSubtitle: string;
  children: ReactNode;
  culture: TCulture;
  cultures: readonly CultureOption<TCulture>[];
  logoUrl: string;
  logoutLabel: string;
  menuItems: MenuProps["items"];
  openKeys: string[];
  selectedView: TView;
  onCultureChange: (value: TCulture) => void;
  onLogout: () => void;
  onOpenKeysChange: (value: string[]) => void;
  onViewChange: (value: TView) => void;
}) {
  const [collapsed, setCollapsed] = useState(false);

  return (
    <Layout className="admin-shell">
      <Sider collapsible collapsed={collapsed} onCollapse={setCollapsed} theme="light" width={280} className="admin-sider">
        <div className="admin-brand">
          <img className="brand-icon" src={logoUrl} alt="" />
          {collapsed ? (
            <span className="admin-brand__abbr">CW</span>
          ) : (
            <div className="admin-brand__copy">
              <strong>{brandTitle}</strong>
              <span>{brandSubtitle}</span>
            </div>
          )}
        </div>
        <Menu
          className="admin-menu"
          mode="inline"
          selectedKeys={[selectedView]}
          openKeys={collapsed ? [] : openKeys}
          onOpenChange={onOpenKeysChange}
          onClick={(event) => onViewChange(event.key as TView)}
          items={menuItems}
        />
      </Sider>
      <Layout className="admin-main">
        <Header className="admin-header">
          <div className="admin-header__title">
            <Typography.Title level={4}>{activeTitle}</Typography.Title>
            <Typography.Text type="secondary">{brandSubtitle}</Typography.Text>
          </div>
          <Space wrap>
            <Select
              value={culture}
              onChange={onCultureChange}
              className="culture-select"
              options={cultures.map((item) => ({ label: item.label, value: item.value }))}
            />
            <Button icon={<LogoutOutlined />} onClick={onLogout}>
              {logoutLabel}
            </Button>
          </Space>
        </Header>
        <Content className="admin-content">
          <div className="admin-content-scroll" key={selectedView}>
            {children}
          </div>
        </Content>
      </Layout>
    </Layout>
  );
}
