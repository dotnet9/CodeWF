import Image from "next/image";
import Link from "next/link";
import { Braces, Clock3, Hash, QrCode } from "lucide-react";
import { api, resolveAssetUrl } from "@/api";
import { HomeHero } from "@/components/HomeHero";
import { formatDate, withLocale } from "@/i18n";
import type { BlogPostBrief, DocNode, Locale, SiteInfo, ToolNode } from "@/types";
import type { LocalePageProps } from "./layout";

const tickerItems = [
  ["#.NET 10", "持续更新"],
  ["#C#", "语言与工程实践"],
  ["#Avalonia", "跨平台桌面"],
  ["#Blazor", "全栈 Web UI"],
  ["#AI 辅助开发", "实战记录"],
  ["#WPF", "企业级桌面"],
  ["#Open Source", "项目与组件"],
  ["#CodeWF", "前后端分离"]
] as const;

export default async function HomePage({ params }: LocalePageProps) {
  const { locale } = await params;
  const [home, docs] = await Promise.all([api.home(locale), api.docs(locale)]);
  const posts = uniquePosts([...home.bannerPosts, ...home.recentPosts]);
  const latestPosts = home.recentPosts.slice(0, 3);
  const rankedPosts = posts.slice(0, 6);
  const featuredPost = posts[0];
  const streamPosts = posts.slice(1, 4);
  const tools = collectToolLeaves(home.tools).slice(0, 4);
  const project = collectDocLeaves(docs)[0];
  const latestDate = latestPosts[0]?.lastmod ?? latestPosts[0]?.date;
  const yearsOnline = Math.max(1, new Date().getFullYear() - home.site.startYear);

  return (
    <main className="prototype-home">
      <div className="prototype-container">
        <HomeHero
          locale={locale}
          updatedAt={formatDate(latestDate, locale)}
          metrics={[
            { value: home.counts.posts ?? 0, suffix: "+", label: "ARTICLES" },
            { value: home.counts.tools ?? 0, label: "TOOLS" },
            { value: home.albums.length, suffix: "+", label: "ALBUMS" },
            { value: yearsOnline, suffix: "yrs", label: `SINCE ${home.site.startYear}` }
          ]}
        />
      </div>

      <div className="prototype-ticker" aria-label="热门技术方向">
        <div className="prototype-ticker__track">
          {[...tickerItems, ...tickerItems].map(([topic, detail], index) => (
            <span key={`${topic}-${index}`}><b>{topic}</b> {detail}</span>
          ))}
        </div>
      </div>

      <div className="prototype-container">
        <section className="prototype-bento" aria-label="站点速览">
          <div className="prototype-card prototype-bento__latest">
            <PrototypeCardTitle title="最新文章" href={withLocale(locale, "/post")} />
            {latestPosts.map((post, index) => (
              <LatestPostLine post={post} locale={locale} site={home.site} index={index + 1} key={postKey(post)} />
            ))}
          </div>

          <div className="prototype-card prototype-bento__ranking">
            <h2 className="prototype-card-title"><span className="prototype-dot prototype-dot--amber" />精选热榜</h2>
            <ol className="prototype-ranking">
              {rankedPosts.map((post, index) => (
                <li key={postKey(post)}>
                  <span>{index + 1}</span>
                  <Link href={withLocale(locale, post.url ?? "/post")}>{post.title ?? post.slug}</Link>
                </li>
              ))}
            </ol>
          </div>

          <div className="prototype-card prototype-bento__tools">
            <h2 className="prototype-card-title"><span className="prototype-dot" />常用工具</h2>
            <div className="prototype-tool-strip">
              {tools.map((tool, index) => (
                <Link href={withLocale(locale, `/tool/${encodeURIComponent(tool.slug ?? "")}`)} key={tool.slug}>
                  <span>{toolIcon(index)}</span>{tool.name}
                </Link>
              ))}
              <Link className="prototype-tool-strip__all" href={withLocale(locale, "/tool")}>
                ./all-tools --count {home.counts.tools ?? 0} →
              </Link>
            </div>
          </div>
        </section>

        <div className="prototype-content-layout">
          <section className="prototype-article-list">
            <div className="prototype-section-title">
              精选文章
              <Link href={withLocale(locale, "/post")}>archive →</Link>
            </div>
            {featuredPost ? <FeaturePost post={featuredPost} locale={locale} site={home.site} featured /> : null}
            {streamPosts.map((post) => (
              <FeaturePost post={post} locale={locale} site={home.site} key={postKey(post)} />
            ))}
            {posts.length === 0 ? <div className="empty-state">近期更新会在这里展示。</div> : null}
          </section>

          <aside className="prototype-home-aside">
            <div className="prototype-card prototype-category-card">
              <h2 className="prototype-card-title"><span className="prototype-dot" />分类索引</h2>
              {home.categories.slice(0, 7).map((category) => (
                <Link href={withLocale(locale, `/cat/${encodeURIComponent(category.slug ?? category.name ?? "")}`)} key={category.slug ?? category.name}>
                  {category.name}<span>{category.postCount}</span>
                </Link>
              ))}
            </div>

            {project ? (
              <div className="prototype-card prototype-project-card">
                <h2 className="prototype-card-title"><span className="prototype-dot" />开源项目</h2>
                <strong>{project.name}</strong>
                <p>{project.memo ?? "查看项目说明、源码与使用方式。"}</p>
                <Link className="prototype-button" href={withLocale(locale, `/project/${encodeURIComponent(project.slug ?? "")}`)}>
                  查看项目 →
                </Link>
              </div>
            ) : null}
          </aside>
        </div>
      </div>
    </main>
  );
}

function PrototypeCardTitle({ title, href }: { title: string; href: string }) {
  return (
    <h2 className="prototype-card-title">
      <span className="prototype-dot" />{title}<Link href={href}>查看全部 →</Link>
    </h2>
  );
}

function LatestPostLine({ post, locale, site, index }: { post: BlogPostBrief; locale: Locale; site: SiteInfo; index: number }) {
  const cover = resolveAssetUrl(site, post.cover);
  return (
    <article className="prototype-latest-line">
      <span className="prototype-latest-line__index">{String(index).padStart(2, "0")}</span>
      <div>
        <h3><Link href={withLocale(locale, post.url ?? "/post")}>{post.title ?? post.slug}</Link></h3>
        <div className="prototype-meta">
          <time dateTime={post.date}>{formatDate(post.lastmod ?? post.date, locale)}</time>
          <span>{post.categories?.slice(0, 2).join(" / ")}</span>
        </div>
      </div>
      {cover ? (
        <Link className="prototype-latest-line__cover" href={withLocale(locale, post.url ?? "/post")} aria-label={post.title}>
          <Image src={cover} alt="" fill sizes="92px" />
        </Link>
      ) : null}
    </article>
  );
}

function FeaturePost({ post, locale, site, featured = false }: { post: BlogPostBrief; locale: Locale; site: SiteInfo; featured?: boolean }) {
  const cover = resolveAssetUrl(site, post.cover);
  const href = withLocale(locale, post.url ?? "/post");
  return (
    <article className={`prototype-card prototype-stream-post${featured ? " prototype-stream-post--featured" : ""}`}>
      <Link className={`prototype-stream-post__cover${cover ? "" : " prototype-stream-post__cover--empty"}`} href={href} aria-label={post.title}>
        {cover ? <Image src={cover} alt="" fill sizes={featured ? "300px" : "180px"} /> : <span>{post.categories?.[0] ?? "CodeWF"}</span>}
      </Link>
      <div className="prototype-stream-post__body">
        <div className="prototype-tags">
          {(post.categories ?? []).slice(0, 3).map((category) => (
            <Link href={withLocale(locale, `/cat/${encodeURIComponent(category)}`)} key={category}>{category}</Link>
          ))}
        </div>
        <h2><Link href={href}>{post.title ?? post.slug}</Link></h2>
        {post.description ? <p>{post.description}</p> : null}
        <div className="prototype-meta">
          {post.author ? <span>{post.author}</span> : null}
          <time dateTime={post.date}>{formatDate(post.lastmod ?? post.date, locale)}</time>
        </div>
      </div>
    </article>
  );
}

function toolIcon(index: number) {
  const icons = [Clock3, Hash, Braces, QrCode];
  const Icon = icons[index % icons.length];
  return <Icon size={16} aria-hidden="true" />;
}

function uniquePosts(posts: BlogPostBrief[]) {
  const seen = new Set<string>();
  return posts.filter((post) => {
    const key = postKey(post);
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}

function postKey(post: BlogPostBrief) {
  return post.slug ?? post.url ?? `${post.date}-${post.title}`;
}

function collectToolLeaves(nodes: ToolNode[]) {
  const results: ToolNode[] = [];
  const visit = (items: ToolNode[]) => {
    for (const node of items) {
      if (node.hidden) continue;
      if (node.children?.length) visit(node.children);
      else if (node.slug) results.push(node);
    }
  };
  visit(nodes);
  return results;
}

function collectDocLeaves(nodes: DocNode[]) {
  const results: DocNode[] = [];
  const visit = (items: DocNode[]) => {
    for (const node of items) {
      if (node.children?.length) visit(node.children);
      else if (node.slug) results.push(node);
    }
  };
  visit(nodes);
  return results;
}
