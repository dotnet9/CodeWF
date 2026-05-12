"use client";

import Link from "next/link";
import { useEffect, useRef, useState } from "react";
import { Search } from "lucide-react";
import { withLocale } from "@/i18n";
import type { Locale, SearchResultItem } from "@/types";

const apiBase = (process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5100/api").replace(/\/$/, "");

export function GlobalSearch({
  locale,
  action,
  placeholder,
  label
}: {
  locale: Locale;
  action: string;
  placeholder: string;
  label: string;
}) {
  const [query, setQuery] = useState("");
  const [items, setItems] = useState<SearchResultItem[]>([]);
  const [open, setOpen] = useState(false);
  const panelRef = useRef<HTMLFormElement>(null);

  useEffect(() => {
    const timer = window.setTimeout(async () => {
      const keyword = query.trim();
      if (keyword.length < 2) {
        setItems([]);
        return;
      }

      try {
        const response = await fetch(`${apiBase}/search/suggest?culture=${encodeURIComponent(locale)}&q=${encodeURIComponent(keyword)}&take=5`, {
          cache: "no-store"
        });
        if (!response.ok) {
          setItems([]);
          return;
        }

        const data = (await response.json()) as { data?: SearchResultItem[] };
        setItems(data.data ?? []);
      } catch {
        setItems([]);
      }
    }, 220);

    return () => window.clearTimeout(timer);
  }, [locale, query]);

  useEffect(() => {
    const onClickOutside = (event: MouseEvent) => {
      if (!panelRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    };

    window.addEventListener("click", onClickOutside);
    return () => window.removeEventListener("click", onClickOutside);
  }, []);

  return (
    <form
      className="site-search-form site-search-form--interactive"
      action={action}
      role="search"
      ref={panelRef}
      onFocus={() => setOpen(true)}
      onSubmit={() => setOpen(false)}
      onKeyDown={(event) => {
        if (event.key === "Escape") {
          setOpen(false);
        }
      }}
    >
      <Search size={16} aria-hidden="true" />
      <input
        name="q"
        type="search"
        value={query}
        onChange={(event) => {
          setQuery(event.target.value);
          setOpen(true);
        }}
        placeholder={placeholder}
        aria-label={label}
        autoComplete="off"
      />
      <button type="submit">{label}</button>
      {open && items.length > 0 ? (
        <div className="search-suggest-panel">
          {items.map((item) => (
            <Link href={withLocale(locale, item.url)} key={`${item.kind}-${item.url}`} onClick={() => setOpen(false)}>
              <span>{item.kind}</span>
              <strong>{item.title}</strong>
              {item.matchedSnippet ? <small>{item.matchedSnippet}</small> : item.summary ? <small>{item.summary}</small> : null}
            </Link>
          ))}
        </div>
      ) : null}
    </form>
  );
}
