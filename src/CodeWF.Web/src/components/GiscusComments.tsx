"use client";

import { useEffect, useRef } from "react";

/** giscus 配置：dotnet9/CodeWF 仓库 Discussions（General 分类，访客可发起） */
const GISCUS = {
  repo: "dotnet9/CodeWF",
  repoId: "R_kgDOLAiTcA",
  category: "General",
  categoryId: "DIC_kwDOLAiTcM4DG9PS",
};

const LANG_BY_LOCALE: Record<string, string> = {
  "zh-CN": "zh-CN",
  "zh-TW": "zh-TW",
  en: "en",
  ja: "ja",
};

/**
 * 全站评论区（giscus，基于 GitHub Discussions）。
 * mapping=pathname：每个页面 URL 对应独立讨论串，自动跟随页面。
 * 仓库需安装 giscus App：https://github.com/apps/giscus
 */
export default function GiscusComments({ locale }: { locale: string }) {
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;
    container.innerHTML = "";

    const script = document.createElement("script");
    script.src = "https://giscus.app/client.js";
    script.async = true;
    script.crossOrigin = "anonymous";
    const attrs: Record<string, string> = {
      "data-repo": GISCUS.repo,
      "data-repo-id": GISCUS.repoId,
      "data-category": GISCUS.category,
      "data-category-id": GISCUS.categoryId,
      "data-mapping": "pathname",
      "data-strict": "0",
      "data-reactions-enabled": "1",
      "data-emit-metadata": "0",
      "data-input-position": "top",
      "data-theme": "light",
      "data-lang": LANG_BY_LOCALE[locale] ?? "zh-CN",
      "data-loading": "lazy",
    };
    Object.entries(attrs).forEach(([k, v]) => script.setAttribute(k, v));
    container.appendChild(script);

    return () => {
      container.innerHTML = "";
    };
  }, [locale]);

  return (
    <section className="site-comments" aria-label="评论">
      <div className="site-comments__inner">
        <h2 className="site-comments__title">评论</h2>
        <div ref={containerRef} className="site-comments__widget" />
        <p className="site-comments__hint">
          评论基于 GitHub Discussions，登录 GitHub 后即可发表。
        </p>
      </div>
    </section>
  );
}
