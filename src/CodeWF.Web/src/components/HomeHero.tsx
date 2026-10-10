"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { ArrowRight, Wrench } from "lucide-react";
import { withLocale } from "@/i18n";
import { toolsSiteUrlFromLocation, toolsSiteUrlFromSite } from "@/lib/use-doc-site";
import type { Locale, SiteInfo } from "@/types";

type Metric = { value: number; suffix?: string; label: string };
const words = ["代码工坊", "灵感仓库", "实用工具箱"];

export function HomeHero({ locale, site, updatedAt, metrics }: { locale: Locale; site?: SiteInfo; updatedAt: string; metrics: Metric[] }) {
  const [toolsSiteUrl, setToolsSiteUrl] = useState(() => toolsSiteUrlFromSite(site));
  const [wordIndex, setWordIndex] = useState(0);
  const [characterCount, setCharacterCount] = useState(words[0].length);
  const [deleting, setDeleting] = useState(false);
  const text = words[wordIndex].slice(0, characterCount);

  useEffect(() => {
    setToolsSiteUrl(toolsSiteUrlFromLocation() ?? toolsSiteUrlFromSite(site));
  }, [site]);

  useEffect(() => {
    const word = words[wordIndex];
    const delay = !deleting && characterCount === word.length ? 1800 : deleting && characterCount === 0 ? 260 : deleting ? 65 : 95;
    const timer = window.setTimeout(() => {
      if (!deleting && characterCount === word.length) {
        setDeleting(true);
      } else if (deleting && characterCount === 0) {
        setWordIndex((current) => (current + 1) % words.length);
        setDeleting(false);
      } else {
        setCharacterCount((current) => current + (deleting ? -1 : 1));
      }
    }, delay);
    return () => window.clearTimeout(timer);
  }, [characterCount, deleting, wordIndex]);

  return (
    <section className="prototype-hero">
      <div className="prototype-hero__copy">
        <span className="prototype-status"><i />SYSTEM ONLINE · 更新于 {updatedAt || "今天"}</span>
        <h1>.NET 开发者的<br /><span>{text}</span><b aria-hidden="true">▌</b></h1>
        <p>深度技术文章与系统化专题，覆盖 WPF、Avalonia、Blazor 与 AI 开发实践；常用在线工具已独立为工具箱站。</p>
        <div className="prototype-hero__actions">
          <Link className="prototype-button" href={withLocale(locale, "/post")}>开始阅读 <ArrowRight size={16} /></Link>
          <a className="prototype-button prototype-button--ghost" href={toolsSiteUrl} target="_blank" rel="noreferrer"><Wrench size={16} /> 打开工具箱</a>
        </div>
        <div className="prototype-metrics">
          {metrics.map((metric) => (
            <div key={metric.label}><strong>{metric.value}<i>{metric.suffix}</i></strong><span>{metric.label}</span></div>
          ))}
        </div>
      </div>

      <div className="prototype-terminal" aria-label="CodeWF 站点状态">
        <div className="prototype-terminal__bar">
          <i className="red" /><i className="yellow" /><i className="green" /><span>codewf — 站点情报 · live</span>
        </div>
        <div className="prototype-terminal__body">
          <p><span className="comment"># 检查站点状态</span></p>
          <p><span className="prompt">$</span> curl codewf.dev/api/health</p>
          <p>{`{`} <span className="key">"status"</span>: <span className="string">"online"</span> {`}`}</p>
          <br />
          <p><span className="prompt">$</span> codewf stats --live</p>
          <p><span className="key">articles</span> <span className="value">{metrics[0]?.value ?? 0}</span></p>
          <p><span className="key">albums</span> <span className="value">{metrics[1]?.value ?? 0}</span></p>
          <br />
          <p><span className="prompt">$</span> dotnet --info</p>
          <p><span className="string">Ready to build.</span> <span className="prototype-terminal__cursor" /></p>
        </div>
      </div>
    </section>
  );
}
