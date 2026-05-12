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
    description:
      locale === "zh-CN"
        ? "浏览站内工具目录，左侧按分组导航，右侧查看工具详情。"
        : "Browse the tool catalog with grouped navigation and detailed listings."
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
