import Link from "next/link";
import type { Metadata } from "next";
import { api } from "@/api";
import { dictionary, withLocale } from "@/i18n";
import type { ToolNode } from "@/types";
import type { LocalePageProps } from "../layout";

type Props = LocalePageProps;

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = dictionary(locale);
  return {
    title: t.toolCatalog,
    description: locale === "zh-CN" ? "浏览站内在线工具目录。" : "Browse the online tools catalog."
  };
}

export default async function ToolsPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const tools = await api.tools(locale);
  const t = dictionary(locale);
  const count = countTools(tools);

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{t.toolCatalog}</h1>
          <p>{locale === "zh-CN" ? `${count} 个工具` : `${count} items across all tool groups`}</p>
        </div>
      </div>
      <div className="tool-tree">
        {tools.map((group) => renderToolNode(group, locale, 0))}
      </div>
    </main>
  );
}

function renderToolNode(node: ToolNode, locale: "zh-CN" | "en" | "ja" | "zh-TW", depth: number): React.ReactNode {
  const children = node.children ?? [];
  const isLeaf = children.length === 0 && Boolean(node.slug);

  if (isLeaf) {
    return (
      <article className="tool-group" key={node.slug ?? node.name}>
        <div className="tool-group__head">
          <span className="card-kicker">{depth === 0 ? "工具" : "子工具"}</span>
          <h2>
            <Link href={withLocale(locale, `/tool/${node.slug}`)}>{node.name}</Link>
          </h2>
        </div>
        {node.memo ? <p>{node.memo}</p> : null}
        {node.repository ? (
          <a className="text-link" href={node.repository} target="_blank" rel="noreferrer">
            仓库链接
          </a>
        ) : null}
      </article>
    );
  }

  return (
    <section className="tool-group tool-group--nested" key={node.slug ?? node.name}>
      <div className="tool-group__head">
        <h2>{node.name}</h2>
        {node.memo ? <p>{node.memo}</p> : null}
      </div>
      <div className="tool-tree__children">{children.map((child) => renderToolNode(child, locale, depth + 1))}</div>
    </section>
  );
}

function countTools(nodes: ToolNode[]): number {
  let count = 0;
  for (const node of nodes) {
    if (node.children?.length) {
      count += countTools(node.children);
    } else if (node.slug) {
      count += 1;
    }
  }
  return count;
}
