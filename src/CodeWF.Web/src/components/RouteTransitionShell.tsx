"use client";

import type { ReactNode } from "react";
import { useEffect, useMemo, useRef, useState } from "react";
import { usePathname } from "next/navigation";

type Props = {
  children: ReactNode;
};

export function RouteTransitionShell({ children }: Props) {
  const pathname = usePathname();
  const [navigating, setNavigating] = useState(false);
  const pendingTimer = useRef<number | null>(null);

  useEffect(() => {
    if (pendingTimer.current !== null) {
      window.clearTimeout(pendingTimer.current);
      pendingTimer.current = null;
    }
    setNavigating(false);
  }, [pathname]);

  useEffect(() => {
    const onPointerDown = (event: MouseEvent) => {
      if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
        return;
      }

      const target = event.target instanceof Element ? event.target.closest("a[href]") : null;
      if (!(target instanceof HTMLAnchorElement)) {
        return;
      }

      if (target.target && target.target !== "_self") {
        return;
      }

      const href = target.getAttribute("href");
      if (!href || href.startsWith("mailto:") || href.startsWith("tel:") || href.startsWith("javascript:")) {
        return;
      }

      try {
        const url = new URL(href, window.location.href);
        if (url.origin !== window.location.origin) {
          return;
        }
        if (url.pathname === window.location.pathname && url.hash === window.location.hash) {
          return;
        }
        if (pendingTimer.current !== null) {
          window.clearTimeout(pendingTimer.current);
        }
        pendingTimer.current = window.setTimeout(() => {
          setNavigating(true);
          pendingTimer.current = null;
        }, 180);
      } catch {
        return;
      }
    };

    window.addEventListener("click", onPointerDown, true);
    return () => {
      if (pendingTimer.current !== null) {
        window.clearTimeout(pendingTimer.current);
      }
      window.removeEventListener("click", onPointerDown, true);
    };
  }, []);

  const frameKey = useMemo(() => pathname ?? "root", [pathname]);

  return (
    <div className={navigating ? "route-transition-shell is-navigating" : "route-transition-shell"}>
      <div className="route-transition-overlay" aria-hidden="true" />
      <div className="route-transition-frame" key={frameKey}>
        {children}
      </div>
    </div>
  );
}
