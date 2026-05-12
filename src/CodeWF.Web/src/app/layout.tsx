import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "CodeWF",
  description: "CodeWF articles, projects, and online tools"
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="zh-CN">
      <body>{children}</body>
    </html>
  );
}
