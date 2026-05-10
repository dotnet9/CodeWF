document.addEventListener("DOMContentLoaded", () => {
    initializeI18nText();
    initializeHeaderOffset();
    initializeDesktopDropdowns();
    initializeLanguageSwitch();
    initializeLocalizedNavigationLoading();
    resumePendingLanguagePreparation();
    initializeSearchSuggestions();
    initializeReadingToc();
    initializeReadingExperience();
});

const LANGUAGE_PENDING_STORAGE_KEY = "codewf.pendingLanguagePreparation";
const LANGUAGE_PENDING_MAX_AGE_MS = 35 * 60 * 1000;
const LANGUAGE_CANCEL_WAIT_MS = 20 * 1000;

function codewfTranslate(value) {
    const text = String(value ?? "");
    const resource = window.CodeWFI18n;
    if (!resource) {
        return text;
    }

    return resource.textMap?.[text] ?? resource.strings?.[text] ?? text;
}

function codewfFormat(key, fallback, ...args) {
    const resource = window.CodeWFI18n;
    const template = String(resource?.strings?.[key] ?? fallback ?? key ?? "");
    return args.reduce(
        (text, value, index) => text.replaceAll(`{${index}}`, String(value ?? "")),
        template);
}

function initializeI18nText() {
    const resource = window.CodeWFI18n;
    if (!resource || resource.language === "zh-cn") {
        return;
    }

    const translateText = (value) => {
        const text = String(value ?? "");
        const trimmed = text.trim();
        if (!trimmed) {
            return text;
        }

        if (resource.textMap?.[trimmed]) {
            return text.replace(trimmed, resource.textMap[trimmed]);
        }

        for (const item of resource.patterns || []) {
            try {
                const regex = new RegExp(item.pattern);
                if (regex.test(trimmed)) {
                    return text.replace(trimmed, trimmed.replace(regex, item.replacement));
                }
            } catch {
                continue;
            }
        }

        return text;
    };

    const shouldSkipNode = (node) => {
        const parent = node.parentElement;
        return !parent
            || parent.closest("script, style, code, pre, textarea, input, select, [contenteditable='true'], .article-content, [data-no-i18n]");
    };

    const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
    const textNodes = [];
    while (walker.nextNode()) {
        const node = walker.currentNode;
        if (!shouldSkipNode(node)) {
            textNodes.push(node);
        }
    }

    textNodes.forEach((node) => {
        node.nodeValue = translateText(node.nodeValue);
    });

    document.querySelectorAll("[placeholder], [aria-label], [title], [alt]").forEach((element) => {
        ["placeholder", "aria-label", "title", "alt"].forEach((attributeName) => {
            const value = element.getAttribute(attributeName);
            if (value) {
                element.setAttribute(attributeName, translateText(value));
            }
        });
    });
}

function initializeHeaderOffset() {
    const root = document.documentElement;
    const navbar = document.querySelector(".site-navbar");

    if (!root || !navbar) {
        return;
    }

    const compactHeaderMedia = window.matchMedia("(max-width: 991.98px)");

    const updateOffset = () => {
        const height = Math.ceil(navbar.getBoundingClientRect().height);
        const minOffset = compactHeaderMedia.matches ? 68 : 72;
        root.style.setProperty("--site-header-offset", `${Math.max(height + 12, minOffset)}px`);
    };

    updateOffset();
    window.addEventListener("resize", updateOffset);
    compactHeaderMedia.addEventListener("change", updateOffset);

    navbar.addEventListener("shown.bs.collapse", updateOffset);
    navbar.addEventListener("hidden.bs.collapse", updateOffset);
    navbar.addEventListener("shown.bs.dropdown", updateOffset);
    navbar.addEventListener("hidden.bs.dropdown", updateOffset);
}

function initializeDesktopDropdowns() {
    const desktopMedia = window.matchMedia("(min-width: 1200px)");
    const dropdowns = Array.from(document.querySelectorAll(".site-navbar .dropdown"));

    if (!dropdowns.length || !window.bootstrap?.Dropdown) {
        return;
    }

    const clearHideTimer = (dropdown) => {
        window.clearTimeout(dropdown.__hideTimer);
    };

    const hideAllDropdowns = () => {
        dropdowns.forEach((dropdown) => {
            const toggle = dropdown.querySelector(".dropdown-toggle");
            if (!toggle) {
                return;
            }

            clearHideTimer(dropdown);
            dropdown.classList.remove("is-hover-open");
            window.bootstrap.Dropdown.getOrCreateInstance(toggle).hide();
        });
    };

    const closeOtherDropdowns = (currentDropdown) => {
        dropdowns.forEach((dropdown) => {
            if (dropdown === currentDropdown) {
                return;
            }

            const toggle = dropdown.querySelector(".dropdown-toggle");
            if (!toggle) {
                return;
            }

            clearHideTimer(dropdown);
            dropdown.classList.remove("is-hover-open");
            window.bootstrap.Dropdown.getOrCreateInstance(toggle).hide();
        });
    };

    dropdowns.forEach((dropdown) => {
        const toggle = dropdown.querySelector(".dropdown-toggle");
        const menu = dropdown.querySelector(".dropdown-menu");

        if (!toggle || !menu) {
            return;
        }

        const dropdownInstance = window.bootstrap.Dropdown.getOrCreateInstance(toggle);

        const showMenu = () => {
            if (!desktopMedia.matches) {
                return;
            }

            closeOtherDropdowns(dropdown);
            clearHideTimer(dropdown);
            dropdown.classList.add("is-hover-open");
            dropdownInstance.show();
        };

        const hideMenu = (event) => {
            if (!desktopMedia.matches) {
                return;
            }

            const nextTarget = event?.relatedTarget;
            if (nextTarget instanceof Node && dropdown.contains(nextTarget)) {
                return;
            }

            clearHideTimer(dropdown);
            dropdown.__hideTimer = window.setTimeout(() => {
                if (dropdown.matches(":hover") || dropdown.contains(document.activeElement)) {
                    return;
                }

                dropdown.classList.remove("is-hover-open");
                dropdownInstance.hide();
            }, 220);
        };

        const hideMenuOnFocusOut = (event) => {
            const nextTarget = event.relatedTarget;
            if (nextTarget instanceof Node && dropdown.contains(nextTarget)) {
                return;
            }

            hideMenu();
        };

        dropdown.addEventListener("pointerenter", showMenu);
        dropdown.addEventListener("pointerleave", hideMenu);
        toggle.addEventListener("focus", showMenu);
        menu.addEventListener("focusin", showMenu);
        dropdown.addEventListener("focusout", hideMenuOnFocusOut);
        dropdown.addEventListener("shown.bs.dropdown", () => dropdown.classList.add("is-hover-open"));
        dropdown.addEventListener("hidden.bs.dropdown", () => dropdown.classList.remove("is-hover-open"));
    });

    desktopMedia.addEventListener("change", hideAllDropdowns);
}

function initializeLanguageSwitch() {
    document.querySelectorAll("[data-language-switch]").forEach((select) => {
        select.addEventListener("change", () => {
            if (select.value) {
                navigateWithLanguagePreparation(select.value, getTargetLanguageFromUrl(select.value));
            }
        });
    });
}

function initializeLocalizedNavigationLoading() {
    const resource = window.CodeWFI18n;
    const currentLanguage = normalizeLanguageCode(resource?.language);
    if (!currentLanguage) {
        return;
    }

    document.addEventListener("click", (event) => {
        if (event.defaultPrevented
            || event.button !== 0
            || event.metaKey
            || event.ctrlKey
            || event.shiftKey
            || event.altKey) {
            return;
        }

        const link = event.target?.closest?.("a[href]");
        const targetLanguage = link ? getLocalizedNavigationTargetLanguage(link, currentLanguage) : "";
        if (!targetLanguage) {
            return;
        }

        event.preventDefault();
        navigateWithLanguagePreparation(link.href, targetLanguage);
    });
}

function getLocalizedNavigationTargetLanguage(link, currentLanguage) {
    if (link.hasAttribute("download")) {
        return "";
    }

    const target = (link.getAttribute("target") || "").trim().toLowerCase();
    if (target && target !== "_self") {
        return "";
    }

    const rawHref = (link.getAttribute("href") || "").trim();
    if (!rawHref
        || rawHref.startsWith("#")
        || /^(?:mailto|tel|javascript|data|blob):/i.test(rawHref)) {
        return "";
    }

    let url;
    try {
        url = new URL(rawHref, window.location.href);
    } catch {
        return "";
    }

    if (url.origin !== window.location.origin) {
        return "";
    }

    if (url.pathname === window.location.pathname
        && url.search === window.location.search
        && url.hash
        && url.hash !== window.location.hash) {
        return "";
    }

    if (isStaticAssetPath(url.pathname)) {
        return "";
    }

    const targetLanguage = getPathLanguage(url.pathname) || currentLanguage;
    return targetLanguage !== "zh-cn" ? targetLanguage : "";
}

async function navigateWithLanguagePreparation(url, language) {
    if (window.__codewfLanguageNavigationActive) {
        return;
    }

    let targetUrl;
    try {
        targetUrl = new URL(url, window.location.href);
    } catch {
        window.location.href = url;
        return;
    }

    const targetLanguage = normalizeLanguageCode(language)
        || getPathLanguage(targetUrl.pathname)
        || normalizeLanguageCode(window.CodeWFI18n?.language)
        || "zh-cn";
    targetUrl = localizeUrlForLanguage(targetUrl, targetLanguage);
    const defaultUrl = localizeUrlForLanguage(targetUrl, "zh-cn");
    const fallbackLanguageName = getFallbackLanguageName(targetLanguage);
    window.__codewfLanguageNavigationActive = true;
    hideLanguageCancelAction();
    updateLanguageLoadingOverlay(targetLanguage, "checking", fallbackLanguageName);

    if (!window.fetch) {
        await delayLanguageNavigation(180);
        window.location.href = targetUrl.href;
        return;
    }

    let cancelTimerId = 0;
    try {
        const status = await fetchLanguageResourceStatus(targetLanguage);
        const languageName = getLanguageStatusName(status, targetLanguage);
        if (targetLanguage === "zh-cn") {
            updateLanguageLoadingOverlay(targetLanguage, "ready", languageName);
            await delayLanguageNavigation(180);
            window.location.href = targetUrl.href;
            return;
        }

        if (status?.hasLanguageResource || status?.isDefaultLanguage) {
            updateLanguageLoadingOverlay(targetLanguage, "existing", languageName);
            await delayLanguageNavigation(220);
            updateLanguageLoadingOverlay(targetLanguage, "preparing", languageName);
        } else {
            updateLanguageLoadingOverlay(targetLanguage, "missing", languageName);
            await delayLanguageNavigation(320);
            updateLanguageLoadingOverlay(targetLanguage, "translating", languageName);
        }

        const job = await queueLanguageTargetPreparation(targetLanguage, targetUrl.href);
        const pending = {
            jobId: job.jobId,
            language: targetLanguage,
            languageName,
            targetUrl: targetUrl.href,
            createdAt: Date.now()
        };
        savePendingLanguagePreparation(pending);
        cancelTimerId = window.setTimeout(
            () => showLanguageCancelAction(targetLanguage, languageName, defaultUrl.href),
            LANGUAGE_CANCEL_WAIT_MS);

        const prepareResult = await waitForLanguagePreparationJob(job.jobId);
        window.clearTimeout(cancelTimerId);
        hideLanguageCancelAction();
        if (prepareResult?.isCompleted) {
            const preparedLanguageName = getLanguageStatusName(prepareResult, targetLanguage);
            updateLanguageLoadingOverlay(targetLanguage, "complete", preparedLanguageName);
            clearPendingLanguagePreparation();
            await delayLanguageNavigation(520);
            window.location.href = targetUrl.href;
            return;
        }

        clearPendingLanguagePreparation();
        updateLanguageLoadingOverlay(targetLanguage, "failed", languageName);
        window.__codewfLanguageNavigationActive = false;
        return;
    } catch {
        window.clearTimeout(cancelTimerId);
        hideLanguageCancelAction();
        updateLanguageLoadingOverlay(targetLanguage, "failed", fallbackLanguageName);
        clearPendingLanguagePreparation();
        window.__codewfLanguageNavigationActive = false;
        return;
    }

    window.location.href = targetUrl.href;
}

async function fetchLanguageResourceStatus(language) {
    const url = new URL("/api/language/status", window.location.origin);
    url.searchParams.set("language", language);
    const response = await fetch(url, {
        headers: { "Accept": "application/json" },
        credentials: "same-origin"
    });

    if (!response.ok) {
        throw new Error(`Language status request failed: ${response.status}`);
    }

    return response.json();
}

async function queueLanguageTargetPreparation(language, targetUrl) {
    const target = getLanguagePreparationTarget(targetUrl);
    const response = await fetch("/api/language/prepare", {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        credentials: "same-origin",
        body: JSON.stringify({ language, url: target, background: true })
    });

    if (!response.ok) {
        throw new Error(`Language prepare request failed: ${response.status}`);
    }

    const payload = await response.json();
    if (payload?.success === false) {
        throw new Error(payload.message || "Language prepare request failed.");
    }

    return payload;
}

function getLanguagePreparationTarget(targetUrl) {
    try {
        const url = targetUrl instanceof URL
            ? targetUrl
            : new URL(targetUrl, window.location.href);
        return `${url.pathname}${url.search}${url.hash}`;
    } catch {
        return targetUrl;
    }
}

async function fetchLanguagePreparationJob(jobId) {
    const response = await fetch(`/api/language/prepare/${encodeURIComponent(jobId)}`, {
        headers: { "Accept": "application/json" },
        credentials: "same-origin"
    });

    if (!response.ok) {
        throw new Error(`Language prepare job request failed: ${response.status}`);
    }

    return response.json();
}

async function waitForLanguagePreparationJob(jobId, timeoutMs) {
    const startedAt = Date.now();
    let latest = null;
    while (!timeoutMs || Date.now() - startedAt < timeoutMs) {
        await delayLanguageNavigation(480);
        latest = await fetchLanguagePreparationJob(jobId);
        if (latest?.isCompleted || latest?.isFailed) {
            return latest;
        }
    }

    return latest;
}

function savePendingLanguagePreparation(pending) {
    try {
        window.sessionStorage?.setItem(LANGUAGE_PENDING_STORAGE_KEY, JSON.stringify(pending));
    } catch {
        return;
    }
}

function getPendingLanguagePreparation() {
    try {
        const value = window.sessionStorage?.getItem(LANGUAGE_PENDING_STORAGE_KEY);
        return value ? JSON.parse(value) : null;
    } catch {
        return null;
    }
}

function clearPendingLanguagePreparation() {
    try {
        window.sessionStorage?.removeItem(LANGUAGE_PENDING_STORAGE_KEY);
    } catch {
        return;
    }
}

function resumePendingLanguagePreparation() {
    const pending = getPendingLanguagePreparation();
    if (!pending?.jobId || !pending?.language || !pending?.targetUrl) {
        return;
    }

    if (Date.now() - Number(pending.createdAt || 0) > LANGUAGE_PENDING_MAX_AGE_MS) {
        clearPendingLanguagePreparation();
        return;
    }

    if (window.__codewfPendingLanguagePreparationActive) {
        return;
    }

    window.__codewfPendingLanguagePreparationActive = true;
    const languageName = pending.languageName || getFallbackLanguageName(pending.language);
    showLanguageTranslationToast(pending.language, getLanguageLoadingStateText(pending.language, "background", languageName), "running");
    pollPendingLanguagePreparation(pending);
}

async function pollPendingLanguagePreparation(pending) {
    const languageName = pending.languageName || getFallbackLanguageName(pending.language);
    try {
        while (Date.now() - Number(pending.createdAt || Date.now()) < LANGUAGE_PENDING_MAX_AGE_MS) {
            await delayLanguageNavigation(2000);
            const job = await fetchLanguagePreparationJob(pending.jobId);
            if (job?.isCompleted) {
                clearPendingLanguagePreparation();
                showLanguageTranslationToast(pending.language, getLanguageLoadingStateText(pending.language, "backgroundComplete", languageName), "complete");
                await delayLanguageNavigation(1200);
                window.location.href = pending.targetUrl;
                return;
            }

            if (job?.isFailed) {
                clearPendingLanguagePreparation();
                showLanguageTranslationToast(pending.language, getLanguageLoadingStateText(pending.language, "backgroundFailed", languageName), "failed");
                return;
            }
        }
    } catch {
        showLanguageTranslationToast(pending.language, getLanguageLoadingStateText(pending.language, "backgroundFailed", languageName), "failed");
        return;
    }

    clearPendingLanguagePreparation();
    showLanguageTranslationToast(pending.language, getLanguageLoadingStateText(pending.language, "backgroundFailed", languageName), "failed");
}

function delayLanguageNavigation(delayMs) {
    return new Promise((resolve) => window.setTimeout(resolve, delayMs));
}

function updateLanguageLoadingOverlay(language, state, languageName) {
    showLanguageLoadingOverlay(language, getLanguageLoadingStateText(language, state, languageName));
}

function showLanguageLoadingOverlay(language, message) {
    const targetLanguage = normalizeLanguageCode(language) || normalizeLanguageCode(window.CodeWFI18n?.language) || "zh-cn";
    let overlay = document.querySelector("[data-language-loading-overlay]");
    if (!overlay) {
        overlay = document.createElement("div");
        overlay.className = "language-loading-overlay";
        overlay.setAttribute("data-language-loading-overlay", "");
        overlay.setAttribute("role", "status");
        overlay.setAttribute("aria-live", "polite");

        const panel = document.createElement("div");
        panel.className = "language-loading-panel";

        const spinner = document.createElement("span");
        spinner.className = "language-loading-spinner";
        spinner.setAttribute("aria-hidden", "true");

        const text = document.createElement("span");
        text.setAttribute("data-language-loading-text", "");

        const button = document.createElement("button");
        button.type = "button";
        button.className = "language-loading-cancel";
        button.setAttribute("data-language-loading-cancel", "");
        button.hidden = true;

        panel.append(spinner, text, button);
        overlay.appendChild(panel);
        document.body.appendChild(overlay);
    }

    const text = overlay.querySelector("[data-language-loading-text]");
    if (text) {
        text.textContent = message || getLanguageLoadingText(targetLanguage);
    }

    overlay.hidden = false;
    document.body.classList.add("language-loading-active");
}

function showLanguageCancelAction(language, languageName, defaultUrl) {
    const button = document.querySelector("[data-language-loading-cancel]");
    if (!button || !defaultUrl) {
        return;
    }

    button.textContent = getLanguageLoadingStateText(language, "cancel", languageName);
    button.onclick = () => {
        clearPendingLanguagePreparation();
        window.__codewfLanguageNavigationActive = false;
        window.location.href = defaultUrl;
    };
    button.hidden = false;
}

function hideLanguageCancelAction() {
    const button = document.querySelector("[data-language-loading-cancel]");
    if (!button) {
        return;
    }

    button.hidden = true;
    button.onclick = null;
}

function showLanguageTranslationToast(language, message, state) {
    let toast = document.querySelector("[data-language-translation-toast]");
    if (!toast) {
        toast = document.createElement("div");
        toast.className = "language-translation-toast";
        toast.setAttribute("data-language-translation-toast", "");
        toast.setAttribute("role", "status");
        toast.setAttribute("aria-live", "polite");
        document.body.appendChild(toast);
    }

    toast.className = `language-translation-toast is-${state || "running"}`;
    toast.textContent = message || getLanguageLoadingText(language);
    toast.hidden = false;
}

function getLanguageLoadingText(language) {
    const normalized = normalizeLanguageCode(language);
    const currentLanguage = normalizeLanguageCode(window.CodeWFI18n?.language);
    if (!normalized || normalized === currentLanguage) {
        return codewfTranslate("正在准备语言内容，请稍候...");
    }

    const direct = languageLoadingTextByCode[normalized];
    if (direct) {
        return direct;
    }

    const parent = normalized.split("-")[0];
    return languageLoadingTextByCode[parent]
        || "Preparing language content, please wait...";
}

const languageLoadingTextByCode = {
    "zh": "正在准备语言内容，请稍候...",
    "zh-cn": "正在准备语言内容，请稍候...",
    "zh-hans": "正在准备语言内容，请稍候...",
    "zh-tw": "正在準備語言內容，請稍候...",
    "zh-hant": "正在準備語言內容，請稍候...",
    "en": "Preparing language content, please wait...",
    "ja": "言語コンテンツを準備しています。しばらくお待ちください...",
    "ko": "언어 콘텐츠를 준비하는 중입니다. 잠시만 기다려 주세요...",
    "fr": "Préparation du contenu linguistique, veuillez patienter...",
    "de": "Sprachinhalte werden vorbereitet, bitte warten...",
    "es": "Preparando el contenido del idioma, espera un momento...",
    "pt": "Preparando o conteúdo do idioma, aguarde...",
    "it": "Preparazione dei contenuti della lingua, attendere...",
    "ru": "Подготавливаем языковой контент, подождите...",
    "nl": "Taalinhoud wordt voorbereid, even geduld...",
    "pl": "Przygotowywanie treści językowych, proszę czekać...",
    "tr": "Dil içeriği hazırlanıyor, lütfen bekleyin...",
    "ar": "جارٍ إعداد محتوى اللغة، يرجى الانتظار...",
    "hi": "भाषा सामग्री तैयार की जा रही है, कृपया प्रतीक्षा करें...",
    "id": "Menyiapkan konten bahasa, harap tunggu...",
    "th": "กำลังเตรียมเนื้อหาภาษา โปรดรอสักครู่...",
    "vi": "Đang chuẩn bị nội dung ngôn ngữ, vui lòng chờ..."
};

const languageLoadingStateTemplatesByCode = {
    "zh": {
        checking: "正在检查{language}语言资源...",
        missing: "无{language}语言资源，准备翻译...",
        existing: "{language}语言资源已存在，正在准备页面内容...",
        translating: "正在翻译{language}语言，请稍等...",
        preparing: "正在准备并翻译{language}页面内容，请稍等...",
        fallback: "{language}翻译预计需要一些时间，请继续等待...",
        background: "{language}翻译正在后台进行，请继续等待...",
        cancel: "取消等待，先看默认中文",
        complete: "翻译完成，准备跳转，感谢等待...",
        ready: "准备完成，马上跳转...",
        backgroundComplete: "{language}翻译完成，准备切换过去...",
        backgroundFailed: "{language}翻译暂未完成，请稍后重试。",
        failed: "{language}语言内容准备失败，请稍后重试。"
    },
    "zh-cn": {
        checking: "正在检查{language}语言资源...",
        missing: "无{language}语言资源，准备翻译...",
        existing: "{language}语言资源已存在，正在准备页面内容...",
        translating: "正在翻译{language}语言，请稍等...",
        preparing: "正在准备并翻译{language}页面内容，请稍等...",
        fallback: "{language}翻译预计需要一些时间，请继续等待...",
        background: "{language}翻译正在后台进行，请继续等待...",
        cancel: "取消等待，先看默认中文",
        complete: "翻译完成，准备跳转，感谢等待...",
        ready: "准备完成，马上跳转...",
        backgroundComplete: "{language}翻译完成，准备切换过去...",
        backgroundFailed: "{language}翻译暂未完成，请稍后重试。",
        failed: "{language}语言内容准备失败，请稍后重试。"
    },
    "zh-tw": {
        checking: "正在檢查{language}語言資源...",
        missing: "沒有{language}語言資源，準備翻譯...",
        existing: "{language}語言資源已存在，正在準備頁面內容...",
        translating: "正在翻譯{language}語言，請稍候...",
        preparing: "正在準備並翻譯{language}頁面內容，請稍候...",
        fallback: "{language}翻譯預計需要一些時間，請繼續等待...",
        background: "{language}翻譯正在背景進行，請繼續等待...",
        cancel: "取消等待，先看預設中文",
        complete: "翻譯完成，準備跳轉，感謝等待...",
        ready: "準備完成，馬上跳轉...",
        backgroundComplete: "{language}翻譯完成，準備切換過去...",
        backgroundFailed: "{language}翻譯暫未完成，請稍後重試。",
        failed: "{language}語言內容準備失敗，請稍後重試。"
    },
    "zh-hant": {
        checking: "正在檢查{language}語言資源...",
        missing: "沒有{language}語言資源，準備翻譯...",
        existing: "{language}語言資源已存在，正在準備頁面內容...",
        translating: "正在翻譯{language}語言，請稍候...",
        preparing: "正在準備並翻譯{language}頁面內容，請稍候...",
        fallback: "{language}翻譯預計需要一些時間，請繼續等待...",
        background: "{language}翻譯正在背景進行，請繼續等待...",
        cancel: "取消等待，先看預設中文",
        complete: "翻譯完成，準備跳轉，感謝等待...",
        ready: "準備完成，馬上跳轉...",
        backgroundComplete: "{language}翻譯完成，準備切換過去...",
        backgroundFailed: "{language}翻譯暫未完成，請稍後重試。",
        failed: "{language}語言內容準備失敗，請稍後重試。"
    },
    "en": {
        checking: "Checking {language} language resources...",
        missing: "No {language} resources found. Preparing translation...",
        existing: "{language} resources found. Preparing page content...",
        translating: "Translating {language}. Please wait...",
        preparing: "Preparing and translating {language} page content. Please wait...",
        fallback: "{language} translation may take a while. Please keep waiting...",
        background: "{language} translation is still running. Please keep waiting...",
        cancel: "Stop waiting and show default Chinese",
        complete: "Translation complete. Redirecting. Thanks for waiting...",
        ready: "Content is ready. Redirecting...",
        backgroundComplete: "{language} translation is ready. Switching now...",
        backgroundFailed: "{language} translation is not ready yet. Please try again later.",
        failed: "Failed to prepare {language} content. Please try again later."
    },
    "ja": {
        checking: "{language}の言語リソースを確認しています...",
        missing: "{language}の言語リソースがありません。翻訳を準備しています...",
        existing: "{language}の言語リソースがあります。ページ内容を準備しています...",
        translating: "{language}に翻訳しています。しばらくお待ちください...",
        preparing: "{language}のページ内容を準備して翻訳しています。しばらくお待ちください...",
        fallback: "{language}の翻訳には時間がかかります。もうしばらくお待ちください...",
        background: "{language}の翻訳をバックグラウンドで続行しています。もうしばらくお待ちください...",
        cancel: "待機をやめて既定の中国語を表示",
        complete: "翻訳が完了しました。移動します。お待ちいただきありがとうございます...",
        ready: "準備が完了しました。移動します...",
        backgroundComplete: "{language}の翻訳が完了しました。切り替えます...",
        backgroundFailed: "{language}の翻訳はまだ完了していません。後でもう一度お試しください。",
        failed: "{language}の内容を準備できませんでした。後でもう一度お試しください。"
    },
    "ko": {
        checking: "{language} 언어 리소스를 확인하는 중입니다...",
        missing: "{language} 언어 리소스가 없습니다. 번역을 준비하는 중입니다...",
        existing: "{language} 언어 리소스가 있습니다. 페이지 콘텐츠를 준비하는 중입니다...",
        translating: "{language}로 번역하는 중입니다. 잠시만 기다려 주세요...",
        preparing: "{language} 페이지 콘텐츠를 준비하고 번역하는 중입니다. 잠시만 기다려 주세요...",
        complete: "번역이 완료되었습니다. 이동합니다. 기다려 주셔서 감사합니다...",
        ready: "준비가 완료되었습니다. 이동합니다...",
        failed: "{language} 콘텐츠 준비에 실패했습니다. 나중에 다시 시도해 주세요."
    },
    "fr": {
        checking: "Vérification des ressources {language}...",
        missing: "Aucune ressource {language} trouvée. Préparation de la traduction...",
        existing: "Ressources {language} trouvées. Préparation du contenu de la page...",
        translating: "Traduction en {language}, veuillez patienter...",
        preparing: "Préparation et traduction du contenu {language}, veuillez patienter...",
        complete: "Traduction terminée. Redirection en cours. Merci d'avoir patienté...",
        ready: "Contenu prêt. Redirection en cours...",
        failed: "Impossible de préparer le contenu {language}. Veuillez réessayer plus tard."
    },
    "de": {
        checking: "{language}-Sprachressourcen werden geprüft...",
        missing: "Keine {language}-Ressourcen gefunden. Übersetzung wird vorbereitet...",
        existing: "{language}-Ressourcen gefunden. Seiteninhalt wird vorbereitet...",
        translating: "Übersetzung nach {language}. Bitte warten...",
        preparing: "{language}-Seiteninhalt wird vorbereitet und übersetzt. Bitte warten...",
        complete: "Übersetzung abgeschlossen. Weiterleitung läuft. Danke fürs Warten...",
        ready: "Inhalt ist bereit. Weiterleitung läuft...",
        failed: "{language}-Inhalt konnte nicht vorbereitet werden. Bitte versuchen Sie es später erneut."
    },
    "es": {
        checking: "Comprobando recursos de {language}...",
        missing: "No se encontraron recursos de {language}. Preparando traducción...",
        existing: "Recursos de {language} encontrados. Preparando contenido de la página...",
        translating: "Traduciendo a {language}. Espera un momento...",
        preparing: "Preparando y traduciendo el contenido de {language}. Espera un momento...",
        complete: "Traducción completada. Redirigiendo. Gracias por esperar...",
        ready: "Contenido listo. Redirigiendo...",
        failed: "No se pudo preparar el contenido de {language}. Inténtalo de nuevo más tarde."
    },
    "pt": {
        checking: "Verificando recursos de {language}...",
        missing: "Nenhum recurso de {language} encontrado. Preparando tradução...",
        existing: "Recursos de {language} encontrados. Preparando conteúdo da página...",
        translating: "Traduzindo para {language}. Aguarde...",
        preparing: "Preparando e traduzindo o conteúdo de {language}. Aguarde...",
        complete: "Tradução concluída. Redirecionando. Obrigado por aguardar...",
        ready: "Conteúdo pronto. Redirecionando...",
        failed: "Falha ao preparar o conteúdo de {language}. Tente novamente mais tarde."
    },
    "it": {
        checking: "Controllo delle risorse {language}...",
        missing: "Nessuna risorsa {language} trovata. Preparazione della traduzione...",
        existing: "Risorse {language} trovate. Preparazione del contenuto della pagina...",
        translating: "Traduzione in {language}. Attendere...",
        preparing: "Preparazione e traduzione del contenuto {language}. Attendere...",
        complete: "Traduzione completata. Reindirizzamento in corso. Grazie per l'attesa...",
        ready: "Contenuto pronto. Reindirizzamento in corso...",
        failed: "Impossibile preparare il contenuto {language}. Riprova più tardi."
    },
    "ru": {
        checking: "Проверяем ресурсы {language}...",
        missing: "Ресурсы {language} не найдены. Подготавливаем перевод...",
        existing: "Ресурсы {language} найдены. Подготавливаем содержимое страницы...",
        translating: "Переводим на {language}. Пожалуйста, подождите...",
        preparing: "Подготавливаем и переводим содержимое {language}. Пожалуйста, подождите...",
        complete: "Перевод завершен. Переходим дальше. Спасибо за ожидание...",
        ready: "Содержимое готово. Переходим дальше...",
        failed: "Не удалось подготовить содержимое {language}. Повторите попытку позже."
    },
    "nl": {
        checking: "{language}-bronnen controleren...",
        missing: "Geen {language}-bronnen gevonden. Vertaling voorbereiden...",
        existing: "{language}-bronnen gevonden. Pagina-inhoud voorbereiden...",
        translating: "Vertalen naar {language}. Even geduld...",
        preparing: "{language}-pagina-inhoud voorbereiden en vertalen. Even geduld...",
        complete: "Vertaling voltooid. Doorsturen. Bedankt voor het wachten...",
        ready: "Inhoud is klaar. Doorsturen...",
        failed: "{language}-inhoud voorbereiden is mislukt. Probeer het later opnieuw."
    },
    "pl": {
        checking: "Sprawdzanie zasobów {language}...",
        missing: "Nie znaleziono zasobów {language}. Przygotowywanie tłumaczenia...",
        existing: "Zasoby {language} znalezione. Przygotowywanie treści strony...",
        translating: "Tłumaczenie na {language}. Proszę czekać...",
        preparing: "Przygotowywanie i tłumaczenie treści {language}. Proszę czekać...",
        complete: "Tłumaczenie zakończone. Przekierowanie. Dziękujemy za cierpliwość...",
        ready: "Treść gotowa. Przekierowanie...",
        failed: "Nie udało się przygotować treści {language}. Spróbuj ponownie później."
    },
    "tr": {
        checking: "{language} dil kaynakları kontrol ediliyor...",
        missing: "{language} kaynağı bulunamadı. Çeviri hazırlanıyor...",
        existing: "{language} kaynakları bulundu. Sayfa içeriği hazırlanıyor...",
        translating: "{language} diline çevriliyor. Lütfen bekleyin...",
        preparing: "{language} sayfa içeriği hazırlanıyor ve çevriliyor. Lütfen bekleyin...",
        complete: "Çeviri tamamlandı. Yönlendiriliyor. Beklediğiniz için teşekkürler...",
        ready: "İçerik hazır. Yönlendiriliyor...",
        failed: "{language} içeriği hazırlanamadı. Lütfen daha sonra tekrar deneyin."
    },
    "ar": {
        checking: "جارٍ فحص موارد {language}...",
        missing: "لم يتم العثور على موارد {language}. جارٍ تجهيز الترجمة...",
        existing: "تم العثور على موارد {language}. جارٍ تجهيز محتوى الصفحة...",
        translating: "جارٍ الترجمة إلى {language}. يرجى الانتظار...",
        preparing: "جارٍ تجهيز وترجمة محتوى {language}. يرجى الانتظار...",
        complete: "اكتملت الترجمة. جارٍ الانتقال. شكرًا لانتظارك...",
        ready: "المحتوى جاهز. جارٍ الانتقال...",
        failed: "تعذر تجهيز محتوى {language}. يرجى المحاولة مرة أخرى لاحقًا."
    },
    "hi": {
        checking: "{language} भाषा संसाधन जांचे जा रहे हैं...",
        missing: "{language} संसाधन नहीं मिले। अनुवाद तैयार किया जा रहा है...",
        existing: "{language} संसाधन मिल गए। पेज सामग्री तैयार की जा रही है...",
        translating: "{language} में अनुवाद हो रहा है। कृपया प्रतीक्षा करें...",
        preparing: "{language} पेज सामग्री तैयार और अनुवाद की जा रही है। कृपया प्रतीक्षा करें...",
        complete: "अनुवाद पूरा हुआ। रीडायरेक्ट किया जा रहा है। प्रतीक्षा के लिए धन्यवाद...",
        ready: "सामग्री तैयार है। रीडायरेक्ट किया जा रहा है...",
        failed: "{language} सामग्री तैयार नहीं हो सकी। कृपया बाद में फिर कोशिश करें."
    },
    "id": {
        checking: "Memeriksa resource {language}...",
        missing: "Resource {language} belum ada. Menyiapkan terjemahan...",
        existing: "Resource {language} ditemukan. Menyiapkan konten halaman...",
        translating: "Menerjemahkan ke {language}. Harap tunggu...",
        preparing: "Menyiapkan dan menerjemahkan konten {language}. Harap tunggu...",
        complete: "Terjemahan selesai. Mengalihkan. Terima kasih sudah menunggu...",
        ready: "Konten siap. Mengalihkan...",
        failed: "Gagal menyiapkan konten {language}. Coba lagi nanti."
    },
    "th": {
        checking: "กำลังตรวจสอบทรัพยากร {language}...",
        missing: "ไม่พบทรัพยากร {language} กำลังเตรียมการแปล...",
        existing: "พบทรัพยากร {language} แล้ว กำลังเตรียมเนื้อหาหน้า...",
        translating: "กำลังแปลเป็น {language} โปรดรอสักครู่...",
        preparing: "กำลังเตรียมและแปลเนื้อหา {language} โปรดรอสักครู่...",
        complete: "แปลเสร็จแล้ว กำลังเปลี่ยนหน้า ขอบคุณที่รอ...",
        ready: "เนื้อหาพร้อมแล้ว กำลังเปลี่ยนหน้า...",
        failed: "เตรียมเนื้อหา {language} ไม่สำเร็จ โปรดลองอีกครั้งภายหลัง."
    },
    "vi": {
        checking: "Đang kiểm tra tài nguyên {language}...",
        missing: "Chưa có tài nguyên {language}. Đang chuẩn bị dịch...",
        existing: "Đã có tài nguyên {language}. Đang chuẩn bị nội dung trang...",
        translating: "Đang dịch sang {language}. Vui lòng chờ...",
        preparing: "Đang chuẩn bị và dịch nội dung {language}. Vui lòng chờ...",
        complete: "Dịch xong. Đang chuyển trang. Cảm ơn bạn đã chờ...",
        ready: "Nội dung đã sẵn sàng. Đang chuyển trang...",
        failed: "Không thể chuẩn bị nội dung {language}. Vui lòng thử lại sau."
    }
};

const languageDisplayNameByCode = {
    "zh": "简体中文",
    "zh-cn": "简体中文",
    "zh-hans": "简体中文",
    "zh-tw": "繁體中文",
    "zh-hant": "繁體中文",
    "en": "English",
    "ja": "日本語",
    "ko": "한국어",
    "fr": "français",
    "de": "Deutsch",
    "es": "español",
    "pt": "português",
    "it": "italiano",
    "ru": "русский",
    "nl": "Nederlands",
    "pl": "polski",
    "tr": "Türkçe",
    "ar": "العربية",
    "hi": "हिन्दी",
    "id": "Indonesia",
    "th": "ไทย",
    "vi": "Tiếng Việt"
};

const fallbackSupportedLanguageCodes = new Set(["zh-cn", "zh-tw", "en", "ja"]);

function getLanguageLoadingStateText(language, state, languageName) {
    const normalized = normalizeLanguageCode(language);
    const templates = getLanguageLoadingStateTemplates(normalized);
    const template = templates[state]
        || languageLoadingStateTemplatesByCode.en[state]
        || templates.preparing
        || getLanguageLoadingText(normalized);
    return template.replace(/\{language\}/g, languageName || getFallbackLanguageName(normalized));
}

function getLanguageLoadingStateTemplates(language) {
    const normalized = normalizeLanguageCode(language);
    return languageLoadingStateTemplatesByCode[normalized]
        || languageLoadingStateTemplatesByCode[normalized.split("-")[0]]
        || languageLoadingStateTemplatesByCode.en;
}

function getLanguageStatusName(status, language) {
    return status?.languageName
        || status?.englishName
        || getFallbackLanguageName(language);
}

function getFallbackLanguageName(language) {
    const normalized = normalizeLanguageCode(language);
    return languageDisplayNameByCode[normalized]
        || languageDisplayNameByCode[normalized.split("-")[0]]
        || normalized.toUpperCase()
        || "language";
}

function getSupportedLanguageCodes() {
    if (!window.__codewfSupportedLanguageCodes) {
        const resourceCodes = Array.isArray(window.CodeWFI18n?.languages)
            ? window.CodeWFI18n.languages
            : [];
        const sourceCodes = resourceCodes.length > 0
            ? resourceCodes
            : [...fallbackSupportedLanguageCodes];
        window.__codewfSupportedLanguageCodes = new Set(
            sourceCodes
                .map(normalizeLanguageCode)
                .filter(Boolean));
    }

    return window.__codewfSupportedLanguageCodes;
}

function normalizeLanguageCode(value) {
    const normalized = String(value || "").trim().replace(/_/g, "-").toLowerCase();
    if (!normalized) {
        return "";
    }

    if (normalized === "zh-tw"
        || normalized === "zh-hk"
        || normalized === "zh-mo"
        || normalized.startsWith("zh-hant")) {
        return "zh-tw";
    }

    if (normalized === "zh"
        || normalized === "zh-cn"
        || normalized === "zh-sg"
        || normalized.startsWith("zh-hans")
        || normalized.startsWith("zh-")) {
        return "zh-cn";
    }

    if (normalized === "en" || normalized.startsWith("en-")) {
        return "en";
    }

    if (normalized === "ja" || normalized === "jp" || normalized.startsWith("ja-")) {
        return "ja";
    }

    return normalized;
}

function getTargetLanguageFromUrl(value) {
    try {
        return getPathLanguage(new URL(value, window.location.href).pathname);
    } catch {
        return "";
    }
}

function localizeUrlForLanguage(url, language) {
    const normalized = normalizeLanguageCode(language);
    if (!normalized) {
        return url;
    }

    const nextUrl = new URL(url.href);
    const pathLanguage = getPathLanguage(nextUrl.pathname);
    if (pathLanguage === normalized) {
        return nextUrl;
    }

    let path = nextUrl.pathname || "/";
    if (pathLanguage) {
        const segments = path.split("/").filter(Boolean);
        path = `/${segments.slice(1).join("/")}`;
        if (path === "/") {
            path = "/";
        }
    }

    nextUrl.pathname = path === "/"
        ? `/${normalized}`
        : `/${normalized}${path}`;
    return nextUrl;
}

function getPathLanguage(pathname) {
    const firstSegment = normalizeLanguageCode(pathname.split("/").filter(Boolean)[0]);
    if (!firstSegment) {
        return "";
    }

    return getSupportedLanguageCodes().has(firstSegment) ? firstSegment : "";
}

function isStaticAssetPath(pathname) {
    const value = pathname.toLowerCase();
    const firstSegment = getPathLanguage(value);
    const path = firstSegment ? value.slice(firstSegment.length + 1) || "/" : value;

    if (path === "/favicon.ico" || path === "/favicon.png" || path === "/robots.txt") {
        return true;
    }

    if (/^\/(?:api|css|js|lib|img|webfonts|uploadicons)(?:\/|$)/i.test(path)) {
        return true;
    }

    return /\.(?:avif|bmp|css|gif|ico|jpe?g|js|json|map|pdf|png|svg|txt|webp|woff2?|xml)(?:$|\?)/i.test(path);
}

function initializeSearchSuggestions() {
    const forms = Array.from(document.querySelectorAll("[data-search-suggestions-url]"));
    if (!forms.length || !window.fetch) {
        return;
    }

    forms.forEach((form, index) => {
        const input = form.querySelector("[data-search-suggest-input]");
        const panel = form.querySelector("[data-search-suggest-panel]");
        if (!input || !panel) {
            return;
        }

        if (!panel.id) {
            panel.id = `search-suggestions-${index + 1}`;
        }

        input.setAttribute("aria-controls", panel.id);

        let suggestions = [];
        let activeIndex = -1;
        let debounceTimer = 0;
        let requestController = null;

        const hidePanel = () => {
            panel.hidden = true;
            panel.innerHTML = "";
            input.removeAttribute("aria-activedescendant");
            activeIndex = -1;
        };

        const setActiveOption = (nextIndex) => {
            const options = Array.from(panel.querySelectorAll(".search-suggest-option"));
            if (!options.length) {
                activeIndex = -1;
                input.removeAttribute("aria-activedescendant");
                return;
            }

            activeIndex = (nextIndex + options.length) % options.length;
            options.forEach((option, optionIndex) => {
                const isActive = optionIndex === activeIndex;
                option.classList.toggle("is-active", isActive);
                option.setAttribute("aria-selected", isActive ? "true" : "false");
            });

            input.setAttribute("aria-activedescendant", options[activeIndex].id);
        };

        const submitSuggestion = (suggestion) => {
            if (!suggestion?.query) {
                return;
            }

            input.value = suggestion.query;
            hidePanel();
            form.requestSubmit ? form.requestSubmit() : form.submit();
        };

        const renderSuggestions = () => {
            panel.innerHTML = "";
            activeIndex = -1;
            input.removeAttribute("aria-activedescendant");

            if (!suggestions.length) {
                hidePanel();
                return;
            }

            const list = document.createElement("div");
            list.className = "search-suggest-list";
            list.setAttribute("role", "listbox");

            suggestions.forEach((suggestion, suggestionIndex) => {
                const option = document.createElement("button");
                option.type = "button";
                option.className = "search-suggest-option";
                option.id = `${panel.id}-option-${suggestionIndex}`;
                option.setAttribute("role", "option");
                option.setAttribute("aria-selected", "false");

                const text = document.createElement("span");
                text.className = "search-suggest-option__text";
                text.textContent = suggestion.query;

                const meta = document.createElement("span");
                meta.className = "search-suggest-option__meta";
                meta.textContent = suggestion.count > 0
                    ? `${suggestion.label} ${suggestion.count}`
                    : suggestion.label;
                meta.textContent = codewfTranslate(meta.textContent);

                option.append(text, meta);
                option.addEventListener("click", () => submitSuggestion(suggestion));
                list.appendChild(option);
            });

            panel.appendChild(list);
            panel.hidden = false;
        };

        const loadSuggestions = async () => {
            const query = input.value.trim();
            if (query.length > 60) {
                hidePanel();
                return;
            }

            if (requestController) {
                requestController.abort();
            }

            requestController = new AbortController();
            const url = new URL(form.dataset.searchSuggestionsUrl, window.location.origin);
            url.searchParams.set("q", query);

            try {
                const response = await fetch(url, {
                    headers: { "Accept": "application/json" },
                    signal: requestController.signal
                });

                if (!response.ok) {
                    hidePanel();
                    return;
                }

                const payload = await response.json();
                suggestions = (payload.suggestions || []).map((item) => ({
                    query: item.query ?? item.Query,
                label: codewfTranslate(item.label ?? item.Label ?? "建议"),
                    count: item.count ?? item.Count ?? 0
                })).filter((item) => item.query);
                renderSuggestions();
            } catch (error) {
                if (error?.name !== "AbortError") {
                    hidePanel();
                }
            }
        };

        const scheduleLoad = () => {
            window.clearTimeout(debounceTimer);
            debounceTimer = window.setTimeout(loadSuggestions, 160);
        };

        input.addEventListener("focus", scheduleLoad);
        input.addEventListener("input", scheduleLoad);
        input.addEventListener("keydown", (event) => {
            if (panel.hidden) {
                return;
            }

            if (event.key === "ArrowDown") {
                event.preventDefault();
                setActiveOption(activeIndex + 1);
                return;
            }

            if (event.key === "ArrowUp") {
                event.preventDefault();
                setActiveOption(activeIndex - 1);
                return;
            }

            if (event.key === "Enter" && activeIndex >= 0) {
                event.preventDefault();
                submitSuggestion(suggestions[activeIndex]);
                return;
            }

            if (event.key === "Escape") {
                hidePanel();
            }
        });

        document.addEventListener("click", (event) => {
            if (!form.contains(event.target)) {
                hidePanel();
            }
        });
    });
}

function initializeReadingToc() {
    const readingBody = document.querySelector("[data-reading-body]");
    const tocRoot = document.querySelector("[data-reading-toc]");

    if (!readingBody || !tocRoot) {
        return;
    }

    const tocContainer = tocRoot.querySelector(".toc-nav");
    if (!tocContainer) {
        return;
    }

    const headings = Array.from(readingBody.querySelectorAll("h2, h3, h4"));
    if (!headings.length) {
        if (tocRoot.dataset.tocHideEmpty === "true") {
            tocRoot.setAttribute("hidden", "hidden");
            return;
        }

        tocContainer.innerHTML = `<p class="toc-empty">${codewfTranslate("当前页面没有可生成的目录。")}</p>`;
        return;
    }

    const fragment = document.createDocumentFragment();
    const tocLinks = [];

    headings.forEach((heading, index) => {
        if (!heading.id) {
            const headingText = (heading.textContent || `section-${index + 1}`)
                .trim()
                .toLowerCase()
                .replace(/[^\u4e00-\u9fa5a-z0-9\s-]/g, "")
                .replace(/\s+/g, "-");

            heading.id = headingText || `section-${index + 1}`;
        }

        const link = document.createElement("a");
        link.href = `#${heading.id}`;
        link.textContent = heading.textContent?.trim() || codewfFormat("reading.chapterLabel", "章节 {0}", index + 1);
        link.className = `toc-link toc-link--${heading.tagName.toLowerCase()}`;
        link.dataset.targetId = heading.id;
        fragment.appendChild(link);
        tocLinks.push(link);
    });

    tocContainer.innerHTML = "";
    tocContainer.appendChild(fragment);

    if (!("IntersectionObserver" in window)) {
        return;
    }

    const observer = new IntersectionObserver((entries) => {
        const visibleEntry = entries
            .filter((entry) => entry.isIntersecting)
            .sort((left, right) => right.intersectionRatio - left.intersectionRatio)[0];

        if (!visibleEntry) {
            return;
        }

        const activeId = visibleEntry.target.id;
        tocLinks.forEach((link) => {
            link.classList.toggle("is-active", link.dataset.targetId === activeId);
        });
    }, {
        rootMargin: "-24% 0px -58% 0px",
        threshold: [0.15, 0.45, 0.75]
    });

    headings.forEach((heading) => observer.observe(heading));
}

function initializeReadingExperience() {
    initializeMarkdownCodeBlocks();
    initializeArticleImageViewer();
    initializeReadingProgress();
    initializeCopyUrlButtons();
}

function initializeMarkdownCodeBlocks() {
    const codeBlocks = Array.from(document.querySelectorAll(".prose pre, .article-content pre, .doc-content pre"));
    if (!codeBlocks.length) {
        return;
    }

    const fallbackCopy = (text) => {
        const textArea = document.createElement("textarea");
        textArea.value = text;
        textArea.setAttribute("readonly", "readonly");
        textArea.style.position = "absolute";
        textArea.style.left = "-9999px";
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand("copy");
        document.body.removeChild(textArea);
    };

    const getLanguage = (pre) => {
        const code = pre.querySelector("code");
        const className = code?.className || pre.className || "";
        const match = className.match(/language-([a-z0-9#+.-]+)/i);
        if (!match) {
            return "text";
        }

        const aliases = {
            csharp: "C#",
            cs: "C#",
            javascript: "JS",
            typescript: "TS",
            markup: "HTML/XML",
            bash: "Shell",
            powershell: "PowerShell",
            text: "Text"
        };

        return aliases[match[1].toLowerCase()] || match[1].toUpperCase();
    };

    codeBlocks.forEach((pre) => {
        if (pre.dataset.codeEnhanced === "true" || pre.closest(".code-block-shell")) {
            return;
        }

        const code = pre.querySelector("code");
        const rawText = code?.textContent || pre.textContent || "";
        const wrapper = document.createElement("div");
        wrapper.className = "code-block-shell";

        const header = document.createElement("div");
        header.className = "code-block-header";

        const language = document.createElement("span");
        language.className = "code-block-language";
        language.textContent = getLanguage(pre);

        const copyButton = document.createElement("button");
        copyButton.type = "button";
        copyButton.className = "code-copy-button";
        copyButton.textContent = codewfTranslate("复制");

        let resetTimer = 0;
        copyButton.addEventListener("click", async () => {
            try {
                if (navigator.clipboard?.writeText) {
                    await navigator.clipboard.writeText(rawText);
                } else {
                    fallbackCopy(rawText);
                }

                window.clearTimeout(resetTimer);
                copyButton.textContent = codewfTranslate("已复制");
                copyButton.classList.add("is-copied");
                resetTimer = window.setTimeout(() => {
                    copyButton.textContent = codewfTranslate("复制");
                    copyButton.classList.remove("is-copied");
                }, 1800);
            } catch (error) {
                copyButton.textContent = codewfTranslate("复制");
                copyButton.classList.remove("is-copied");
            }
        });

        header.append(language, copyButton);
        pre.dataset.codeEnhanced = "true";
        pre.parentNode.insertBefore(wrapper, pre);
        wrapper.append(header, pre);
    });
}

function initializeReadingProgress() {
    const readingBody = document.querySelector("[data-reading-body]");
    const progressBar = document.querySelector("[data-reading-progress-bar]");

    if (!readingBody || !progressBar) {
        return;
    }

    let ticking = false;

    const updateProgress = () => {
        const rect = readingBody.getBoundingClientRect();
        const viewportHeight = window.innerHeight || document.documentElement.clientHeight || 1;
        const totalDistance = Math.max(readingBody.scrollHeight - viewportHeight * 0.55, 1);
        const travelled = Math.min(Math.max(viewportHeight * 0.22 - rect.top, 0), totalDistance);
        const progress = Math.min(Math.max(travelled / totalDistance, 0), 1);

        progressBar.style.transform = `scaleX(${progress})`;
        ticking = false;
    };

    const requestProgressUpdate = () => {
        if (ticking) {
            return;
        }

        ticking = true;
        window.requestAnimationFrame(updateProgress);
    };

    updateProgress();
    window.addEventListener("scroll", requestProgressUpdate, { passive: true });
    window.addEventListener("resize", requestProgressUpdate);
}

function initializeArticleImageViewer() {
    const readingBody = document.querySelector("[data-reading-body]");
    if (!readingBody) {
        return;
    }

    const images = Array.from(readingBody.querySelectorAll("img"));
    if (!images.length) {
        return;
    }

    const imageUrlPattern = /\.(avif|bmp|gif|jpe?g|png|svg|webp)(\?.*)?$/i;
    let viewer = null;
    let viewerImage = null;
    let scale = 1;
    let rotation = 0;
    let activeImage = null;

    const isPreviewableImage = (image) => {
        const src = image.currentSrc || image.getAttribute("src") || "";
        if (!src) {
            return false;
        }

        const link = image.closest("a");
        if (!link) {
            return true;
        }

        const href = link.getAttribute("href") || "";
        if (!href || href.startsWith("#")) {
            return true;
        }

        return imageUrlPattern.test(href) || href === src;
    };

    const getPreviewSource = (image) => {
        const link = image.closest("a");
        const href = link?.getAttribute("href") || "";
        if (href && imageUrlPattern.test(href)) {
            return link.href;
        }

        return image.currentSrc || image.src;
    };

    const updateTransform = () => {
        if (!viewerImage) {
            return;
        }

        viewerImage.style.transform = `scale(${scale}) rotate(${rotation}deg)`;
    };

    const clampScale = (nextScale) => Math.min(Math.max(nextScale, 0.45), 3);

    const closeViewer = () => {
        if (!viewer) {
            return;
        }

        viewer.hidden = true;
        viewer.classList.remove("is-open");
        document.body.classList.remove("article-image-viewer-open");
        activeImage?.focus?.();
        activeImage = null;
    };

    const ensureViewer = () => {
        if (viewer) {
            return viewer;
        }

        viewer = document.createElement("div");
        viewer.className = "article-image-viewer";
        viewer.hidden = true;
        viewer.setAttribute("role", "dialog");
        viewer.setAttribute("aria-modal", "true");
        viewer.setAttribute("aria-label", codewfTranslate("文章图片预览"));

        const toolbar = document.createElement("div");
        toolbar.className = "article-image-viewer__toolbar";

        const makeButton = (label, iconClass, action) => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "article-image-viewer__button";
            button.setAttribute("aria-label", label);
            button.title = label;
            button.innerHTML = `<i class="${iconClass}" aria-hidden="true"></i>`;
            button.addEventListener("click", action);
            return button;
        };

        toolbar.append(
            makeButton(codewfTranslate("缩小"), "fas fa-magnifying-glass-minus", () => {
                scale = clampScale(scale - 0.2);
                updateTransform();
            }),
            makeButton(codewfTranslate("放大"), "fas fa-magnifying-glass-plus", () => {
                scale = clampScale(scale + 0.2);
                updateTransform();
            }),
            makeButton(codewfTranslate("向左旋转"), "fas fa-rotate-left", () => {
                rotation -= 90;
                updateTransform();
            }),
            makeButton(codewfTranslate("向右旋转"), "fas fa-rotate-right", () => {
                rotation += 90;
                updateTransform();
            }),
            makeButton(codewfTranslate("重置"), "fas fa-arrows-rotate", () => {
                scale = 1;
                rotation = 0;
                updateTransform();
            }),
            makeButton(codewfTranslate("关闭"), "fas fa-xmark", closeViewer)
        );

        const stage = document.createElement("div");
        stage.className = "article-image-viewer__stage";

        viewerImage = document.createElement("img");
        viewerImage.className = "article-image-viewer__image";
        viewerImage.alt = "";
        stage.appendChild(viewerImage);

        viewer.append(toolbar, stage);
        viewer.addEventListener("click", (event) => {
            if (event.target === viewer || event.target === stage) {
                closeViewer();
            }
        });

        document.body.appendChild(viewer);
        return viewer;
    };

    const openViewer = (image) => {
        ensureViewer();
        activeImage = image;
        scale = 1;
        rotation = 0;
        viewerImage.src = getPreviewSource(image);
        viewerImage.alt = image.alt || codewfTranslate("文章图片");
        updateTransform();
        viewer.hidden = false;
        viewer.classList.add("is-open");
        document.body.classList.add("article-image-viewer-open");
        viewer.querySelector("button")?.focus();
    };

    images.forEach((image) => {
        if (!isPreviewableImage(image)) {
            return;
        }

        image.classList.add("article-image-previewable");
        image.tabIndex = 0;
        image.setAttribute("role", "button");
        image.setAttribute("aria-label", image.alt ? `${codewfTranslate("查看大图")}：${image.alt}` : codewfTranslate("查看文章图片大图"));

        image.addEventListener("click", (event) => {
            event.preventDefault();
            openViewer(image);
        });

        image.addEventListener("keydown", (event) => {
            if (event.key !== "Enter" && event.key !== " ") {
                return;
            }

            event.preventDefault();
            openViewer(image);
        });
    });

    document.addEventListener("keydown", (event) => {
        if (!viewer || viewer.hidden) {
            return;
        }

        if (event.key === "Escape") {
            event.preventDefault();
            closeViewer();
            return;
        }

        if (event.key === "+" || event.key === "=") {
            event.preventDefault();
            scale = clampScale(scale + 0.2);
            updateTransform();
            return;
        }

        if (event.key === "-") {
            event.preventDefault();
            scale = clampScale(scale - 0.2);
            updateTransform();
            return;
        }

        if (event.key === "0") {
            event.preventDefault();
            scale = 1;
            rotation = 0;
            updateTransform();
            return;
        }

        if (event.key.toLowerCase() === "r" || event.key === "ArrowRight") {
            event.preventDefault();
            rotation += 90;
            updateTransform();
            return;
        }

        if (event.key === "ArrowLeft") {
            event.preventDefault();
            rotation -= 90;
            updateTransform();
        }
    });
}

function initializeCopyUrlButtons() {
    const copyButtons = Array.from(document.querySelectorAll("[data-copy-url]"));
    if (!copyButtons.length) {
        return;
    }

    const fallbackCopy = (text) => {
        const textArea = document.createElement("textarea");
        textArea.value = text;
        textArea.setAttribute("readonly", "readonly");
        textArea.style.position = "absolute";
        textArea.style.left = "-9999px";
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand("copy");
        document.body.removeChild(textArea);
    };

    copyButtons.forEach((button) => {
        const label = button.querySelector("[data-copy-url-label]");
        const defaultLabel = button.dataset.copyLabelDefault || codewfTranslate("复制链接");
        const successLabel = button.dataset.copyLabelSuccess || codewfTranslate("已复制");
        let resetTimer = 0;

        button.addEventListener("click", async () => {
            try {
                if (navigator.clipboard?.writeText) {
                    await navigator.clipboard.writeText(window.location.href);
                } else {
                    fallbackCopy(window.location.href);
                }

                if (!label) {
                    return;
                }

                window.clearTimeout(resetTimer);
                label.textContent = successLabel;
                resetTimer = window.setTimeout(() => {
                    label.textContent = defaultLabel;
                }, 2200);
            } catch (error) {
                if (label) {
                    label.textContent = defaultLabel;
                }
            }
        });
    });
}
