import { useState, type ReactNode } from "react";
import { Alert, Button, Card, Input, Layout, Select, Space, Typography } from "antd";
import type { AdminCredentials } from "../api";

type CultureOption<TCulture extends string> = {
  label: string;
  value: TCulture;
};

export type AuthText = {
  loginTitle: string;
  loginSubtitle: string;
  usernamePlaceholder: string;
  passwordPlaceholder: string;
  loginButton: string;
  defaultAccount: string;
  failedAuth: string;
};

export function CenteredShell<TCulture extends string>({
  children,
  culture,
  cultures,
  onCultureChange
}: {
  children: ReactNode;
  culture: TCulture;
  cultures: readonly CultureOption<TCulture>[];
  onCultureChange: (value: TCulture) => void;
}) {
  return (
    <Layout className="login-shell">
      <Card className="login-card">
        <div className="login-shell__locale">
          <Select
            value={culture}
            onChange={onCultureChange}
            className="culture-select"
            options={cultures.map((item) => ({ label: item.label, value: item.value }))}
          />
        </div>
        {children}
      </Card>
    </Layout>
  );
}

export function LoginPanel({
  pending,
  error,
  defaultValue,
  logoUrl,
  text,
  onSubmit
}: {
  pending: boolean;
  error: string | null;
  defaultValue: AdminCredentials;
  logoUrl: string;
  text: AuthText;
  onSubmit: (value: AdminCredentials) => Promise<void>;
}) {
  const [value, setValue] = useState<AdminCredentials>(defaultValue);

  return (
    <div className="login-panel">
      <div className="login-brand">
        <img className="brand-icon" src={logoUrl} alt="" />
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
