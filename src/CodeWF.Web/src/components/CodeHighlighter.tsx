"use client";

import { useEffect } from "react";
import hljs from "highlight.js/lib/common";

export function CodeHighlighter() {
  useEffect(() => {
    hljs.highlightAll();
  }, []);

  return null;
}
