import type { MetadataRoute } from "next";
import { api } from "@/api";

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const site = await api.site();
  const posts = await api.posts(site.defaultCulture, { pageIndex: 1, pageSize: 100 });
  const base = site.domain.replace(/\/$/, "");
  return [
    { url: base, lastModified: new Date() },
    { url: `${base}/post`, lastModified: new Date() },
    { url: `${base}/tool`, lastModified: new Date() },
    { url: `${base}/project`, lastModified: new Date() },
    ...posts.data.map((post) => ({
      url: `${base}${post.url ?? ""}`,
      lastModified: post.lastmod ? new Date(post.lastmod) : undefined
    }))
  ];
}
