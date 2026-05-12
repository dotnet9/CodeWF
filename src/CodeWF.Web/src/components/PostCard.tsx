import Image from "next/image";
import Link from "next/link";
import { CalendarDays } from "lucide-react";
import { formatDate, withLocale } from "@/i18n";
import { resolveAssetUrl } from "@/api";
import type { BlogPostBrief, Locale, SiteInfo } from "@/types";

export function PostCard({ post, locale, site }: { post: BlogPostBrief; locale: Locale; site: SiteInfo }) {
  const href = withLocale(locale, post.url ?? "/post");
  const cover = resolveAssetUrl(site, post.cover);

  return (
    <article className="post-card">
      {cover ? (
        <Link href={href} className="post-cover" aria-label={post.title}>
          <Image src={cover} alt="" fill sizes="(max-width: 720px) 100vw, 360px" />
        </Link>
      ) : null}
      <div className="post-card-body">
        <div className="post-meta">
          <CalendarDays size={15} />
          <time dateTime={post.date}>{formatDate(post.date, locale)}</time>
        </div>
        <h3>
          <Link href={href} className="post-card__title">
            {post.title ?? post.slug}
          </Link>
        </h3>
        <p className="post-card__summary">{post.description}</p>
        <div className="tag-row">
          {(post.categories ?? []).slice(0, 3).map((item) => (
            <Link href={withLocale(locale, `/cat/${encodeURIComponent(item)}`)} key={item}>
              {item}
            </Link>
          ))}
        </div>
      </div>
    </article>
  );
}
