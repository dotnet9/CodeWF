import Link from "next/link";
import { dictionary, withLocale } from "@/i18n";
import type { Locale } from "@/types";

export function Pagination({
  locale,
  pageIndex,
  pageSize,
  total,
  basePath,
  query
}: {
  locale: Locale;
  pageIndex: number;
  pageSize: number;
  total: number;
  basePath: string;
  query?: Record<string, string | number | undefined>;
}) {
  const t = dictionary(locale);
  const pageCount = Math.max(1, Math.ceil(total / pageSize));
  if (pageCount <= 1) {
    return null;
  }

  const href = (page: number) => {
    const params = new URLSearchParams();
    Object.entries(query ?? {}).forEach(([key, value]) => {
      if (value !== undefined && value !== "") {
        params.set(key, String(value));
      }
    });
    if (page > 1) {
      params.set("pageIndex", String(page));
    }
    const suffix = params.toString();
    return `${withLocale(locale, basePath)}${suffix ? `?${suffix}` : ""}`;
  };

  return (
    <nav className="pagination" aria-label="Pagination">
      <Link aria-disabled={pageIndex <= 1} href={href(Math.max(1, pageIndex - 1))}>
        {t.previous}
      </Link>
      <span>
        {t.page} {pageIndex} / {pageCount}
      </span>
      <Link aria-disabled={pageIndex >= pageCount} href={href(Math.min(pageCount, pageIndex + 1))}>
        {t.next}
      </Link>
    </nav>
  );
}
