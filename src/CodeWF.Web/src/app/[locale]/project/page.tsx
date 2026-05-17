import Link from "next/link";
import type { Metadata } from "next";
import { api } from "@/api";
import { dictionary, withLocale } from "@/i18n";
import type { DocNode, Locale } from "@/types";
import type { LocalePageProps } from "../layout";
import type { ReactNode } from "react";

type Props = LocalePageProps;

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = dictionary(locale);
  return {
    title: t.projectCatalog,
    description: locale === "zh-CN" ? "浏览项目、文档和开源仓库索引。" : "Browse project docs and open-source repository index."
  };
}

export default async function ProjectPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const docs = await api.docs(locale);
  const t = dictionary(locale);
  const count = countDocs(docs);

  return (
    <main className="page-wrap">
      <div className="section-head">
        <div>
          <h1>{t.projectCatalog}</h1>
          <p>{locale === "zh-CN" ? `${count} 篇项目文档` : `${count} project docs across the repository tree`}</p>
        </div>
      </div>
      <div className="project-tree">{docs.map((group) => renderNode(group, locale, 0))}</div>
    </main>
  );
}

function renderNode(group: DocNode, locale: Locale, depth: number): ReactNode {
  const children = group.children ?? [];
  const isLeaf = children.length === 0 && Boolean(group.slug);
  const repoLabel = locale === "zh-CN" ? "仓库链接" : "Repository";
  const itemLabel = locale === "zh-CN" ? (depth === 0 ? "项目" : "子项目") : depth === 0 ? "Project" : "Subproject";

  if (isLeaf) {
    return (
      <article className="doc-list-item" key={group.slug}>
        <div className="doc-list-item__head">
          <span className="card-kicker">{itemLabel}</span>
          <h2>
            <Link href={withLocale(locale, `/project/${group.slug}`)}>{group.name}</Link>
          </h2>
        </div>
        <p>{group.memo}</p>
        {group.repository ? (
          <a className="repo-link" href={group.repository} target="_blank" rel="noreferrer">
            {repoLabel}
          </a>
        ) : null}
      </article>
    );
  }

  return (
    <section className="doc-list-group" key={group.slug ?? group.name}>
      <div className="doc-list-group__head">
        <h2>{group.name}</h2>
        {group.memo ? <p>{group.memo}</p> : null}
      </div>
      <div className="doc-grid">{children.map((child) => renderNode(child, locale, depth + 1))}</div>
    </section>
  );
}

function countDocs(nodes: DocNode[]): number {
  let count = 0;
  for (const node of nodes) {
    if (node.children?.length) {
      count += countDocs(node.children);
    } else if (node.slug) {
      count += 1;
    }
  }
  return count;
}
