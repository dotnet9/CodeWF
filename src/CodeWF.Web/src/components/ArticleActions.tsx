"use client";

import { useState } from "react";
import { Flag, Link2, MessageCircle, Star, ThumbsUp } from "lucide-react";

export function ArticleActions() {
  const [liked, setLiked] = useState(false);
  const [starred, setStarred] = useState(false);
  const [copied, setCopied] = useState(false);

  async function copyLink() {
    try {
      await navigator.clipboard.writeText(window.location.href);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1600);
    } catch {
      setCopied(false);
    }
  }

  return (
    <div className="article-actions" aria-label="文章操作">
      <button type="button" className={liked ? "is-active" : ""} onClick={() => setLiked((value) => !value)}>
        <ThumbsUp size={14} aria-hidden="true" /> {liked ? "已赞" : "赞"}
      </button>
      <a href="#comments"><MessageCircle size={14} aria-hidden="true" /> 评论</a>
      <button type="button" onClick={copyLink}>
        <Link2 size={14} aria-hidden="true" /> {copied ? "已复制" : "复制链接"}
      </button>
      <button type="button" className={starred ? "is-active" : ""} onClick={() => setStarred((value) => !value)}>
        <Star size={14} aria-hidden="true" /> {starred ? "已收藏" : "收藏"}
      </button>
      <a href="#report"><Flag size={14} aria-hidden="true" /> 举报</a>
    </div>
  );
}
