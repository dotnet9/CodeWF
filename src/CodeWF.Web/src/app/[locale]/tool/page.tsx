import type { Metadata } from "next";
import { api } from "@/api";
import { ToolCatalog } from "@/components/ToolCatalog";
import { dictionary } from "@/i18n";
import type { LocalePageProps } from "../layout";

type Props = LocalePageProps;

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = dictionary(locale);
  return {
    title: t.toolCatalog,
    description: locale === "zh-CN" ? "紧凑浏览站内工具目录，并支持标题和描述搜索。" : "Browse the tools catalog with compact cards and search."
  };
}

export default async function ToolsPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const tools = await api.tools(locale);
  const t = dictionary(locale);

  return (
    <main className="page-wrap">
      <ToolCatalog locale={locale} tools={tools} title={t.toolCatalog} />
    </main>
  );
}
