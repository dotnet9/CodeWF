import { NextResponse, type NextRequest } from "next/server";
import { defaultLocale, isKnownLocale, isLocale } from "./src/i18n";

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  if (
    pathname.startsWith("/_next")
    || pathname.startsWith("/api")
    || pathname === "/favicon.ico"
    || pathname === "/robots.txt"
    || pathname === "/sitemap.xml"
    || pathname === "/sitemap"
    || pathname === "/rss.xml"
    || pathname === "/rss"
    || /\.[a-z0-9]+$/i.test(pathname)
  ) {
    return NextResponse.next();
  }

  const segment = pathname.split("/")[1];
  if (isLocale(segment)) {
    return NextResponse.next();
  }

  const url = request.nextUrl.clone();
  if (isKnownLocale(segment)) {
    const rest = pathname.split("/").slice(2).join("/");
    url.pathname = `/${defaultLocale}${rest ? `/${rest}` : ""}`;
    return NextResponse.redirect(url);
  }

  url.pathname = `/${defaultLocale}${pathname === "/" ? "" : pathname}`;
  return NextResponse.redirect(url);
}

export const config = {
  matcher: ["/((?!_next/static|_next/image).*)"]
};
