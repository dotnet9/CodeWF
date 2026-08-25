"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { ArrowRight, Wrench } from "lucide-react";
import { withLocale } from "@/i18n";
import type { Locale } from "@/types";

type Metric = { value: number; suffix?: string; label: string };
const words = ["代码工坊", "灵感仓库", "实用工具箱"];

export function HomeHero({ locale, updatedAt, metrics }: { locale: Locale; updatedAt: string; metrics: Metric[] }) {
  const [wordIndex, setWordIndex] = useState(0);
  const [characterCount, setCharacterCount] = useState(words[0].length);
  const [deleting, setDeleting] = useState(false);
  const text = words[wordIndex].slice(0, characterCount);

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
        <p>深度技术文章、浏览器本地运行的在线工具和系统化专题，覆盖 WPF、Avalonia、Blazor 与 AI 开发实践。</p>
        <div className="prototype-hero__actions">
          <Link className="prototype-button" href={withLocale(locale, "/post")}>开始阅读 <ArrowRight size={16} /></Link>
          <Link className="prototype-button prototype-button--ghost" href={withLocale(locale, "/tool")}><Wrench size={16} /> 打开工具箱</Link>
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
          <p><span className="key">tools</span> <span className="value">{metrics[1]?.value ?? 0}</span></p>
          <p><span className="key">albums</span> <span className="value">{metrics[2]?.value ?? 0}</span></p>
          <br />
          <p><span className="prompt">$</span> dotnet --info</p>
          <p><span className="string">Ready to build.</span> <span className="prototype-terminal__cursor" /></p>
        </div>
      </div>
    </section>
  );
}
