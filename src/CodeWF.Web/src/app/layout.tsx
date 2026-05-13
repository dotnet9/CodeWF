import type { Metadata } from "next";
import "./globals.css";

const API_ORIGIN = (process.env.NEXT_PUBLIC_API_BASE_URL ?? process.env.API_BASE_URL ?? "http://localhost:5100/api")
  .replace(/\/$/, "")
  .replace(/\/api$/, "");

export const metadata: Metadata = {
  title: {
    default: "CodeWF",
    template: "%s | CodeWF"
  },
  description: "CodeWF articles, projects, and online tools",
  icons: {
    icon: `${API_ORIGIN}/site/favicon/logo.ico?v=20260513`,
    shortcut: `${API_ORIGIN}/site/favicon/logo.ico?v=20260513`,
    apple: `${API_ORIGIN}/site/favicon/logo.ico?v=20260513`
  }
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="zh-CN">
      <body>{children}</body>
    </html>
  );
}
