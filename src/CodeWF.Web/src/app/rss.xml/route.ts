const apiBase = (process.env.API_BASE_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5002/api").replace(/\/api$/, "");

export async function GET() {
  const response = await fetch(`${apiBase}/rss.xml`, { next: { revalidate: 120 } });
  return new Response(await response.text(), {
    headers: { "content-type": "application/rss+xml; charset=utf-8" }
  });
}
