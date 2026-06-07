import "./globals.css";
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: {
    default: "CodeWF",
    template: "%s | CodeWF"
  },
  description: "CodeWF articles, projects, and online tools",
  icons: {
    icon: "/favicon.ico",
    shortcut: "/favicon.ico",
    apple: "/favicon.ico"
  }
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="zh-CN">
      <body>{children}</body>
    </html>
  );
}
