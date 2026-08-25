import { notFound } from "next/navigation";
import Link from "next/link";
import { api } from "@/api";
import { ToolWorkbench } from "@/components/ToolWorkbench";
import { withLocale } from "@/i18n";
import type { ToolNode } from "@/types";

type Props = {
  params: Promise<{ locale: "zh-CN" | "en" | "ja" | "zh-TW"; slug: string }>;
};

export async function generateMetadata({ params }: Props) {
  const { locale, slug } = await params;
  const [tool, tree] = await Promise.all([api.tool(locale, slug), api.tools(locale)]);
  return {
    title: tool?.name ?? slug,
    description: tool?.memo
  };
}

export default async function ToolDetailPage({ params }: Props) {
  const { locale, slug } = await params;
  const [tool, tree] = await Promise.all([api.tool(locale, slug), api.tools(locale)]);
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
          <a className="repo-link" href={tool.repository} target="_blank" rel="noreferrer">
            {locale === "zh-CN" ? "仓库链接" : "Repository"}
          </a>
        ) : null}
      </div>
      <ToolWorkbench tool={tool} locale={locale} />
      <section className="tool-related">
        <div className="section-title">相关工具</div>
        <div className="tool-related-grid">
          {flattenTools(tree).filter((item) => item.slug !== tool.slug).slice(0, 5).map((item) => (
            <Link className="tool-related-card" href={withLocale(locale, `/tool/${item.slug}`)} key={item.slug}>
              <span className="tool-related-card__icon" aria-hidden="true">#</span>
              <span>{item.name}</span>
            </Link>
          ))}
        </div>
      </section>
    </main>
  );
}

function flattenTools(nodes: ToolNode[]): ToolNode[] {
  return nodes.flatMap((node) => node.children?.length ? flattenTools(node.children) : node.slug ? [node] : []);
}
