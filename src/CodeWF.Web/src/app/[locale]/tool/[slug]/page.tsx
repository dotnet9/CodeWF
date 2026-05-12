import { notFound } from "next/navigation";
import { api } from "@/api";
import { ToolWorkbench } from "@/components/ToolWorkbench";

type Props = {
  params: Promise<{ locale: "zh-CN" | "en" | "ja" | "zh-TW"; slug: string }>;
};

export async function generateMetadata({ params }: Props) {
  const { locale, slug } = await params;
  const tool = await api.tool(locale, slug);
  return {
    title: tool?.name ?? slug,
    description: tool?.memo
  };
}

export default async function ToolDetailPage({ params }: Props) {
  const { locale, slug } = await params;
  const tool = await api.tool(locale, slug);
  if (!tool) {
    notFound();
  }

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{tool.name}</h1>
          <p>{tool.memo}</p>
        </div>
        {tool.repository ? (
          <a className="text-link" href={tool.repository} target="_blank" rel="noreferrer">
            仓库链接
          </a>
        ) : null}
      </div>
      <ToolWorkbench tool={tool} locale={locale} />
    </main>
  );
}
