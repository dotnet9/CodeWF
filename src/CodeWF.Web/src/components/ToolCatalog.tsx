"use client";

import Link from "next/link";
import { useEffect, useMemo, useState, type ReactNode } from "react";
import { Search } from "lucide-react";
import { withLocale } from "@/i18n";
import type { Locale, ToolNode } from "@/types";

type CatalogGroup = {
  key: string;
  title: string;
  memo?: string;
  tools: ToolNode[];
  count: number;
};

export function ToolCatalog({ locale, tools, title }: { locale: Locale; tools: ToolNode[]; title: string }) {
  const [keyword, setKeyword] = useState("");
  const filteredTree = useMemo(() => filterTree(tools, keyword.trim()), [tools, keyword]);
  const groups = useMemo(() => buildGroups(filteredTree), [filteredTree]);
  const total = useMemo(() => groups.reduce((sum, group) => sum + group.count, 0), [groups]);
  const [activeGroup, setActiveGroup] = useState(groups[0]?.key ?? "");

  useEffect(() => {
    setActiveGroup(groups[0]?.key ?? "");
  }, [groups]);

  const scrollToGroup = (key: string) => {
    setActiveGroup(key);
    document.getElementById(`tool-group-${key}`)?.scrollIntoView({ behavior: "smooth", block: "start" });
  };

  return (
    <section className="tool-catalog">
      <div className="tool-catalog__toolbar">
        <div>
          <h1>{title}</h1>
          <p>{locale === "zh-CN" ? `共 ${total} 个工具，打开就能处理常见开发小任务` : `${total} tools total`}</p>
        </div>
        <label className="tool-search" aria-label={locale === "zh-CN" ? "搜索工具" : "Search tools"}>
          <Search size={16} aria-hidden="true" />
          <input
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
            placeholder={locale === "zh-CN" ? "搜索标题、简介或仓库地址" : "Search title, memo, or repository"}
          />
        </label>
      </div>

      <div className="tool-catalog__layout">
        <aside className="tool-catalog__sidebar">
          <div className="tool-catalog__sidebar-title">{locale === "zh-CN" ? "分组" : "Groups"}</div>
          <div className="tool-catalog__group-list">
            {groups.map((group) => (
              <button
                type="button"
                key={group.key}
                className={group.key === activeGroup ? "tool-catalog__group-link is-active" : "tool-catalog__group-link"}
                onClick={() => scrollToGroup(group.key)}
              >
                <strong>{group.title}</strong>
                <span>{group.count}</span>
              </button>
            ))}
          </div>
        </aside>

        <div className="tool-catalog__content">
          {groups.map((group) => (
            <section className="tool-detail-section" id={`tool-group-${group.key}`} key={group.key}>
              <div className="tool-detail-section__head">
                <div>
                  <h2>{group.title}</h2>
                  {group.memo ? <p>{group.memo}</p> : null}
                </div>
                <span>{group.count}</span>
              </div>
              <div className="tool-detail-grid">
                {group.tools.map((node) => renderNode(node, locale))}
              </div>
            </section>
          ))}

          {groups.length === 0 ? (
            <div className="empty-state empty-state--compact">
              {locale === "zh-CN" ? "暂无匹配工具" : "No matching tools"}
            </div>
          ) : null}
        </div>
      </div>
    </section>
  );
}

function renderNode(node: ToolNode, locale: Locale): ReactNode {
  if (node.hidden) {
    return null;
  }

  const children = node.children ?? [];
  const hasChildren = children.length > 0;

  if (!hasChildren && node.slug) {
    return (
      <Link href={withLocale(locale, `/tool/${encodeURIComponent(node.slug)}`)} className="tool-card" key={node.slug}>
        <span className="card-kicker">{locale === "zh-CN" ? "工具" : "Tool"}</span>
        <strong>{node.name ?? node.slug}</strong>
        {node.memo ? <p>{node.memo}</p> : null}
        {node.repository ? <small>{node.repository}</small> : null}
      </Link>
    );
  }

  return (
    <article className="tool-node" key={node.slug ?? node.name}>
      <div className="tool-node__head">
        <div>
          <strong>{node.name ?? node.slug}</strong>
          {node.memo ? <p>{node.memo}</p> : null}
        </div>
        <span>{children.length}</span>
      </div>
      <div className="tool-detail-grid tool-detail-grid--nested">
        {children.map((child) => renderNode(child, locale))}
      </div>
    </article>
  );
}

function buildGroups(nodes: ToolNode[]): CatalogGroup[] {
  return nodes.map((node, index) => {
    const groupTools = node.children?.length ? node.children : node.slug ? [node] : [];
    return {
      key: node.slug ?? `${node.name ?? "group"}-${index}`,
      title: node.name ?? node.slug ?? "Group",
      memo: node.memo,
      tools: groupTools,
      count: countLeaves(groupTools)
    };
  });
}

function filterTree(nodes: ToolNode[], keyword: string): ToolNode[] {
  if (!keyword) {
    return nodes.filter((node) => !node.hidden);
  }

  const q = keyword.toLowerCase();
  const matches = (node: ToolNode) =>
    [node.name, node.memo, node.slug, node.repository].filter(Boolean).some((value) => value!.toLowerCase().includes(q));

  const visit = (items: ToolNode[]): ToolNode[] =>
    items
      .map((node) => {
        if (node.hidden) {
          return null;
        }

        const children = node.children?.length ? visit(node.children) : [];
        if (children.length > 0) {
          return { ...node, children };
        }

        return matches(node) ? { ...node, children: undefined } : null;
      })
      .filter((node): node is ToolNode => Boolean(node));

  return visit(nodes);
}

function countLeaves(nodes: ToolNode[]): number {
  let total = 0;

  const visit = (items: ToolNode[]) => {
    for (const node of items) {
      if (node.hidden) {
        continue;
      }

      if (node.children?.length) {
        visit(node.children);
      } else if (node.slug) {
        total += 1;
      }
    }
  };

  visit(nodes);
  return total;
}
