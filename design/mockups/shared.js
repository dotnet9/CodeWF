/* ============================================================
   CodeWF 原型 · 全站交互（shared.js）
   1. Ctrl+K / 点击导航搜索框 / 按 "/" → 唤起命令面板
      （索引：页面 / 文章 / 工具 / 专题；↑↓ 选择、Enter 跳转、Esc 关闭）
   2. 导航栏滚动加深投影
   落地时：INDEX 替换为 /api/search 的异步检索。
   ============================================================ */
(function () {
  "use strict";

  /* ── 原型内置索引（真实数据节选）── */
  var INDEX = [
    /* 快速入口（空查询时展示） */
    { g: "页面", t: "首页", s: "~/ · home", h: "index.html", quick: true },
    { g: "页面", t: "文章列表", s: "~/blog", h: "blog.html", quick: true },
    { g: "页面", t: "专题系列", s: "~/album", h: "album.html", quick: true },
    { g: "页面", t: "文档中心", s: "~/doc", h: "doc.html", quick: true },
    { g: "页面", t: "在线工具", s: "~/tool · 91 tools", h: "tools.html", quick: true },
    { g: "页面", t: "建站时间线", s: "~/timeline", h: "timeline.html", quick: true },
    { g: "页面", t: "友情链接", s: "~/links", h: "friends.html", quick: true },
    { g: "页面", t: "关于本站", s: "~/about", h: "about.html", quick: true },
    { g: "页面", t: "全局搜索", s: "~/search", h: "search.html", quick: true },

    /* 文章 */
    { g: "文章", t: "CodeWF.Markdown：一个基于 Avalonia 12 的 Markdown 渲染控件", s: "2026-05-20 · avalonia", h: "post.html" },
    { g: "文章", t: "CodeWF.AvaloniaControls 新增 Guide 引导控件：从 AtomUI Tour 到 Vex 落地", s: "2026-05-23 · avalonia", h: "post.html" },
    { g: "文章", t: "我宣布：这个网站，终于被我用 AI 重构成了", s: "2026-05-01 · ai · razor", h: "post.html" },
    { g: "文章", t: "Vex 1.1.0：免费开源的 .NET + Avalonia 跨平台 Markdown 编辑器", s: "2026-05-24 · desktop", h: "post.html" },
    { g: "文章", t: "枝见 Zhijian：一个用 Avalonia 做的 Markdown 脑图编辑器", s: "2026-05-17 · desktop", h: "post.html" },
    { g: "文章", t: "CodeWF Toolbox：一个用 Avalonia + Prism 做出来的开发者工具箱", s: "2026-05-16 · desktop", h: "post.html" },
    { g: "文章", t: "AI 重构 Razor Pages 网站完成", s: "2026-04-16 · aspnetcore", h: "post.html" },
    { g: "文章", t: ".NET 跨平台本地库引入实战", s: "2026-04-17 · native", h: "post.html" },
    { g: "文章", t: "写给所有 .NET 开发者的 2025 年度总结", s: "2026-01-05 · summary", h: "post.html" },
    { g: "文章", t: "Avalonia 剪贴板和 DataGrid 的问题", s: "2026-01-11 · avalonia", h: "post.html" },
    { g: "文章", t: "2025 年度语言：C#", s: "2026-01-08 · csharp", h: "post.html" },

    /* 工具 */
    { g: "工具", t: "时间戳转换", s: "timestamp", h: "tool-detail.html" },
    { g: "工具", t: "标题转 URL 别名 Slugify", s: "slugify-string", h: "tool-detail.html" },
    { g: "工具", t: "文本哈希计算", s: "hash-text · md5/sha", h: "tool-detail.html" },
    { g: "工具", t: "二维码生成器", s: "qrcode-generator", h: "tool-detail.html" },
    { g: "工具", t: "UUID / ULID 生成器", s: "uuid-generator", h: "tool-detail.html" },
    { g: "工具", t: "Bcrypt 哈希", s: "bcrypt", h: "tool-detail.html" },
    { g: "工具", t: "JWT 解析器", s: "jwt-parser", h: "tool-detail.html" },
    { g: "工具", t: "JSON 格式化 / 压缩", s: "json-prettify", h: "tool-detail.html" },
    { g: "工具", t: "正则表达式测试器", s: "regex-tester", h: "tool-detail.html" },
    { g: "工具", t: "YAML ⇄ JSON ⇄ TOML 互转", s: "yaml-to-json", h: "tool-detail.html" },
    { g: "工具", t: "IPv4 子网计算器", s: "ipv4-subnet-calculator", h: "tool-detail.html" },
    { g: "工具", t: "Docker Run 转 Compose", s: "docker-run-to-compose", h: "tool-detail.html" },
    { g: "工具", t: "Crontab 表达式生成器", s: "crontab-generator", h: "tool-detail.html" },
    { g: "工具", t: "Markdown 转 HTML", s: "markdown-to-html", h: "tool-detail.html" },
    { g: "工具", t: "Icon 图标转换器", s: "icon-converter", h: "tool-detail.html" },

    /* 专题 */
    { g: "专题", t: "一起学 Blazor 系列", s: "31 篇 · 已完结", h: "album-detail.html" },
    { g: "专题", t: "Blazor 组件库", s: "18 篇", h: "album.html" },
    { g: "专题", t: "Avalonia UI 开源项目", s: "21 篇", h: "album.html" },
    { g: "专题", t: "WPF MVVM 框架 Prism 系列", s: "26 篇", h: "album.html" },
    { g: "专题", t: "C# AOT", s: "9 篇", h: "album.html" },
    { g: "专题", t: "从护士到 C# 开发者", s: "8 篇 · 连载中", h: "album.html" }
  ];

  var GLYPH = { "页面": "»", "文章": "#", "工具": "$", "专题": "◈" };
  var panel, input, listEl, rows = [], active = -1;

  function esc(s) {
    return String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;")
      .replace(/>/g, "&gt;").replace(/"/g, "&quot;");
  }
  /* 命中片段高亮 */
  function mark(text, q) {
    var t = esc(text);
    if (!q) return t;
    var i = t.toLowerCase().indexOf(q.toLowerCase());
    if (i < 0) return t;
    return t.slice(0, i) + "<mark>" + t.slice(i, i + q.length) + "</mark>" + t.slice(i + q.length);
  }

  function build() {
    panel = document.createElement("div");
    panel.className = "cmdk";
    panel.innerHTML =
      '<div class="mask"></div>' +
      '<div class="dlg" role="dialog" aria-label="全局搜索">' +
      '  <div class="ibar">⌕<input type="text" placeholder="搜索页面 / 文章 / 工具 / 专题…" spellcheck="false"><kbd>ESC</kbd></div>' +
      '  <div class="list"></div>' +
      '  <div class="fbar"><span>↑↓ 导航</span><span>↵ 打开</span><span>esc 关闭</span><span style="margin-left:auto">' + INDEX.length + ' entries indexed</span></div>' +
      '</div>';
    document.body.appendChild(panel);
    listEl = panel.querySelector(".list");
    input = panel.querySelector("input");

    panel.querySelector(".mask").addEventListener("click", close);
    input.addEventListener("input", function () { render(input.value); });
    /* 阻止面板内点击冒泡触发全局快捷键判断 */
    panel.addEventListener("mousedown", function (e) { if (e.target.tagName !== "INPUT") e.preventDefault(); });
  }

  function render(q) {
    q = (q || "").trim();
    rows = []; active = -1;
    var hits, i;
    if (!q) {
      hits = INDEX.filter(function (x) { return x.quick; });
    } else {
      var ql = q.toLowerCase();
      hits = INDEX.filter(function (x) {
        return (x.t + " " + x.s).toLowerCase().indexOf(ql) >= 0;
      }).slice(0, 14);
    }

    if (!hits.length) {
      listEl.innerHTML = '<div class="empty">no results for "' + esc(q) + '"<br><small>试试 avalonia / json / blazor / hash …</small></div>';
      return;
    }

    var html = "", lastGroup = null;
    for (i = 0; i < hits.length; i++) {
      var x = hits[i];
      if (x.g !== lastGroup) {
        html += '<div class="grp">' + esc(x.g).toUpperCase() + "</div>";
        lastGroup = x.g;
      }
      html += '<a class="it" href="' + esc(x.h) + '">' +
        '<span class="ic">' + (GLYPH[x.g] || "»") + "</span>" +
        '<span class="t">' + mark(x.t, q) + "</span>" +
        '<span class="s">' + esc(x.s) + "</span></a>";
    }
    listEl.innerHTML = html;

    var els = listEl.querySelectorAll(".it");
    for (i = 0; i < els.length; i++) {
      (function (el, idx) {
        el.addEventListener("mouseenter", function () { setActive(idx); });
      })(els[i], i);
    }
    rows = hits;
    setActive(0);
  }

  function setActive(idx) {
    active = idx;
    var els = listEl.querySelectorAll(".it");
    for (var i = 0; i < els.length; i++) els[i].classList.toggle("act", i === idx);
    if (els[idx]) {
      var top = els[idx].offsetTop, h = listEl.clientHeight;
      if (top < listEl.scrollTop || top > listEl.scrollTop + h - 44) {
        listEl.scrollTop = top - h / 2 + 22;
      }
    }
  }

  function open() {
    if (!panel) build();
    panel.classList.add("open");
    document.documentElement.style.overflow = "hidden";
    input.value = "";
    render("");
    setTimeout(function () { input.focus(); }, 30);
  }
  function close() {
    if (!panel) return;
    panel.classList.remove("open");
    document.documentElement.style.overflow = "";
  }
  function toggle() { panel && panel.classList.contains("open") ? close() : open(); }

  /* ── 全局按键 ── */
  document.addEventListener("keydown", function (e) {
    var isOpen = panel && panel.classList.contains("open");
    if ((e.ctrlKey || e.metaKey) && (e.key === "k" || e.key === "K")) {
      e.preventDefault(); toggle(); return;
    }
    if (e.key === "Escape") { close(); return; }
    if (e.key === "/" && !isOpen && !/INPUT|TEXTAREA|SELECT/.test(document.activeElement.tagName)) {
      e.preventDefault(); open(); return;
    }
    if (isOpen) {
      if (e.key === "ArrowDown") { e.preventDefault(); setActive((active + 1) % rows.length); }
      else if (e.key === "ArrowUp") { e.preventDefault(); setActive((active - 1 + rows.length) % rows.length); }
      else if (e.key === "Enter") {
        e.preventDefault();
        if (rows[active]) location.href = rows[active].h;
      }
    }
  });

  /* 导航栏搜索框可点击唤起 */
  Array.prototype.forEach.call(document.querySelectorAll(".nav-kbd, .searchbox"), function (el) {
    el.style.cursor = "pointer";
    el.addEventListener("click", function () { open(); });
  });

  /* ── 导航滚动态：加投影，视觉分层 ── */
  var nav = document.querySelector(".navbar");
  function onScroll() {
    if (nav) nav.classList.toggle("scrolled", (window.scrollY || document.documentElement.scrollTop || 0) > 8);
  }
  window.addEventListener("scroll", onScroll, { passive: true });
  onScroll();
})();
