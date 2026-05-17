import Image from "next/image";
import Link from "next/link";
import { CalendarDays } from "lucide-react";
import { formatDate, withLocale } from "@/i18n";
import { resolveAssetUrl } from "@/api";
import type { BlogPostBrief, Locale, SiteInfo } from "@/types";

export function PostCard({ post, locale, site }: { post: BlogPostBrief; locale: Locale; site: SiteInfo }) {
  const href = withLocale(locale, post.url ?? "/post");
  const cover = resolveAssetUrl(site, post.cover);
  const fallbackLabel = post.categories?.[0] ?? site.appTitle;

  return (
    <article className="post-card">
      <Link href={href} className={cover ? "post-cover" : "post-cover post-cover--placeholder"} aria-label={post.title}>
        {cover ? (
          <Image className="post-cover__image" src={cover} alt="" fill sizes="(max-width: 720px) 100vw, 360px" />
        ) : (
          <span className="post-cover__placeholder">
            <span>{fallbackLabel}</span>
          </span>
        )}
        <span className="post-cover__shine" aria-hidden="true" />
        <span className="post-cover__glow" aria-hidden="true" />
      </Link>
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
