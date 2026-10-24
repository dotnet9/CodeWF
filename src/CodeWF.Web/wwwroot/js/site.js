// CodeWF 站点脚本：代码高亮、目录提取、阅读进度、图片灯箱、链接复制。
// 设计约束：静态 SSR 为主——本脚本在每次导航后按需初始化，兼容静态渲染的整页加载。
(function () {
    "use strict";

    function $(selector, root) { return (root || document).querySelector(selector); }

    // ---------- 代码高亮 ----------
    function highlightCode() {
        if (window.hljs) {
            document.querySelectorAll(".rich-content pre code:not(.hljs)").forEach(function (block) {
                try { window.hljs.highlightElement(block); } catch (e) { /* 高亮失败保持原文 */ }
            });
        }
    }

    // ---------- 目录提取（h2-h4 带 id 的标题，与原 ContentToc 口径一致） ----------
    function buildToc() {
        var host = $("#toc-body");
        var content = $("#rich-content");
        if (!host || !content) return;
        var headings = content.querySelectorAll("h2[id], h3[id], h4[id]");
        if (headings.length === 0) { host.parentElement.style.display = "none"; return; }
        var html = "";
        headings.forEach(function (h) {
            var level = h.tagName === "H2" ? "" : (h.tagName === "H3" ? " is-l3" : " is-l4");
            html += '<a class="toc-link' + level + '" href="#' + h.id + '">' + h.textContent + "</a>";
        });
        host.innerHTML = html;
        host.querySelectorAll("a").forEach(function (a) {
            a.addEventListener("click", function (e) {
                e.preventDefault();
                var target = document.getElementById(decodeURIComponent(a.getAttribute("href").slice(1)));
                if (target) target.scrollIntoView({ behavior: "smooth", block: "start" });
            });
        });
    }

    // ---------- 阅读进度条 ----------
    function setupProgress() {
        var bar = $("#reading-progress");
        if (!bar) return;
        function update() {
            var doc = document.documentElement;
            var max = doc.scrollHeight - doc.clientHeight;
            bar.style.width = (max > 0 ? Math.min(100, (doc.scrollTop / max) * 100) : 0) + "%";
        }
        document.removeEventListener("scroll", window.__codewfProgress);
        window.__codewfProgress = update;
        document.addEventListener("scroll", update, { passive: true });
        update();
    }

    // ---------- 图片灯箱 ----------
    var lightbox = null;
    function setupLightbox() {
        var content = $("#rich-content");
        if (!content) return;
        var images = Array.from(content.querySelectorAll("img"));
        images.forEach(function (img, index) {
            img.style.cursor = "zoom-in";
            img.addEventListener("click", function () { openLightbox(images, index); });
        });
    }

    function openLightbox(images, index) {
        closeLightbox();
        lightbox = document.createElement("div");
        lightbox.className = "lightbox";
        lightbox.innerHTML =
            '<button class="lightbox__close" aria-label="关闭">×</button>' +
            '<button class="lightbox__nav lightbox__prev" aria-label="上一张">‹</button>' +
            '<img class="lightbox__img" alt="" />' +
            '<button class="lightbox__nav lightbox__next" aria-label="下一张">›</button>' +
            '<a class="lightbox__origin" target="_blank" rel="noreferrer">查看原图</a>';
        document.body.appendChild(lightbox);

        var img = lightbox.querySelector(".lightbox__img");
        var origin = lightbox.querySelector(".lightbox__origin");
        var current = index;
        function show() {
            img.src = images[current].src;
            origin.href = images[current].src;
        }
        function step(delta) { current = (current + delta + images.length) % images.length; show(); }

        lightbox.querySelector(".lightbox__close").addEventListener("click", closeLightbox);
        lightbox.querySelector(".lightbox__prev").addEventListener("click", function () { step(-1); });
        lightbox.querySelector(".lightbox__next").addEventListener("click", function () { step(1); });
        lightbox.addEventListener("click", function (e) { if (e.target === lightbox) closeLightbox(); });
        document.addEventListener("keydown", onKey);
        function onKey(e) {
            if (e.key === "Escape") closeLightbox();
            if (e.key === "ArrowLeft") step(-1);
            if (e.key === "ArrowRight") step(1);
        }
        lightbox.__onKey = onKey;
        show();
    }

    function closeLightbox() {
        if (!lightbox) return;
        if (lightbox.__onKey) document.removeEventListener("keydown", lightbox.__onKey);
        lightbox.remove();
        lightbox = null;
    }

    // ---------- 复制链接 ----------
    window.site = {
        init: function () { highlightCode(); buildToc(); setupProgress(); setupLightbox(); },
        copyLink: function () {
            navigator.clipboard && navigator.clipboard.writeText(window.location.href);
        }
    };

    function init() { window.site.init(); }
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
    // Blazor 增强导航：DOM 替换后重新初始化（高亮/灯箱/目录/进度条）
    document.addEventListener("blazor:enhancedload", init);
})();
