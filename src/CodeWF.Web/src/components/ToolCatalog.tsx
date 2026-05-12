"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { Search } from "lucide-react";
import { withLocale } from "@/i18n";
import type { Locale, ToolNode } from "@/types";

type ToolItem = {
  name: string;
  slug: string;
  memo?: string;
  repository?: string;
};

export function ToolCatalog({ locale, tools, title }: { locale: Locale; tools: ToolNode[]; title: string }) {
  const [keyword, setKeyword] = useState("");
  const items = useMemo(() => flattenTools(tools), [tools]);
  const filtered = useMemo(() => {
    const q = keyword.trim().toLowerCase();
    if (!q) {
      return items;
    }

    return items.filter((item) => [item.name, item.memo, item.slug].filter(Boolean).some((value) => value!.toLowerCase().includes(q)));
  }, [items, keyword]);

  return (
    <section className="tool-catalog">
      <div className="tool-catalog__toolbar">
        <div>
          <h1>{title}</h1>
          <p>{locale === "zh-CN" ? `${filtered.length} 个工具` : `${filtered.length} tools`}</p>
        </div>
        <label className="tool-search" aria-label={locale === "zh-CN" ? "搜索工具" : "Search tools"}>
          <Search size={16} aria-hidden="true" />
          <input value={keyword} onChange={(event) => setKeyword(event.target.value)} placeholder={locale === "zh-CN" ? "搜索标题或描述" : "Search title or description"} />
        </label>
      </div>
      <div className="tool-grid tool-grid--compact">
        {filtered.map((tool) => (
          <Link href={withLocale(locale, `/tool/${encodeURIComponent(tool.slug)}`)} className="tool-card" key={tool.slug}>
            <span className="card-kicker">{locale === "zh-CN" ? "工具" : "Tool"}</span>
            <strong>{tool.name}</strong>
            {tool.memo ? <p>{tool.memo}</p> : null}
            {tool.repository ? <small>{tool.repository}</small> : null}
          </Link>
        ))}
      </div>
      {filtered.length === 0 ? <div className="empty-state empty-state--compact">{locale === "zh-CN" ? "暂无匹配工具" : "No matching tools"}</div> : null}
    </section>
  );
}

function flattenTools(nodes: ToolNode[]): ToolItem[] {
  const result: ToolItem[] = [];

  const visit = (items: ToolNode[]) => {
    for (const node of items) {
      if (node.children?.length) {
        visit(node.children);
      } else if (node.slug) {
        result.push({
          name: node.name ?? node.slug,
          slug: node.slug,
          memo: node.memo,
          repository: node.repository
        });
      }
    }
  };

  visit(nodes);
  return result;
}
