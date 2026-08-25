import Link from "next/link";
import { api } from "@/api";
import { CodeHighlighter } from "@/components/CodeHighlighter";
import { HtmlContent } from "@/components/HtmlContent";
import { withLocale } from "@/i18n";
import type { DocNode, Locale } from "@/types";
import type { LocalePageProps } from "../layout";
import type { ReactNode } from "react";

export default async function DocPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const [docs, intro] = await Promise.all([api.docs(locale), api.doc(locale, "readme")]);
  return (
    <main className="page-wrap doc-page">
      <div className="section-head">
        <div>
          <h1>文档中心</h1>
          <p>~/docs · guides &amp; manuals</p>
        </div>
      </div>
      <div className="doc-page-layout">
        <aside className="doc-toc-panel">
          <h2>TABLE OF CONTENTS</h2>
          {docs.map((node) => renderTocNode(node, locale, 0))}
        </aside>
        <article className="doc-article">
          <div className="doc-article__path">site/doc/getting-started/intro.md</div>
          {intro?.htmlContent ? (
            <HtmlContent html={intro.htmlContent} />
          ) : (
            <>
              <h1>项目简介</h1>
              <p>CodeWF 是一个前后端分离的博客与在线工具网站，内容仓库存放文章、项目说明和多语言资源。</p>
              <h2>本地运行</h2>
              <p>安装依赖后启动 API、前台和后台三个应用。</p>
              <pre>npm install{"\n"}dotnet restore CodeWF.slnx{"\n"}{"\n"}npm run dev:api{"\n"}npm run dev:frontend{"\n"}npm run dev:admin</pre>
              <h2>内容仓库</h2>
              <p>文章正文存放于 <code>YYYY/MM/slug.md</code>，元数据使用同名 sidecar 文件维护。</p>
              <h2>多语言</h2>
              <p>缺少对应语言文件时，页面回退到默认中文内容。</p>
            </>
          )}
        </article>
      </div>
      <CodeHighlighter />
    </main>
  );
}

function renderTocNode(node: DocNode, locale: Locale, depth: number): ReactNode {
  const children = node.children ?? [];
  if (children.length === 0) {
    return node.slug ? (
      <Link className={node.slug === "readme" ? "doc-toc-link is-current" : "doc-toc-link"} href={withLocale(locale, `/project/${node.slug}`)} key={node.slug}>
        {node.name}
      </Link>
    ) : null;
  }

  return (
    <div className="doc-toc-group" key={node.slug ?? node.name}>
      <div className="doc-toc-group__title">/ {node.name}</div>
      {children.map((child) => renderTocNode(child, locale, depth + 1))}
    </div>
  );
}
