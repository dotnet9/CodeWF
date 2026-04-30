document.addEventListener("DOMContentLoaded", () => {
    initializeHeaderOffset();
    initializeDesktopDropdowns();
    initializeSearchSuggestions();
    initializeReadingToc();
    initializeReadingExperience();
});

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
                    label: item.label ?? item.Label ?? "建议",
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

        tocContainer.innerHTML = '<p class="toc-empty">当前页面没有可生成的目录。</p>';
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
        link.textContent = heading.textContent?.trim() || `章节 ${index + 1}`;
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
        copyButton.textContent = "复制";

        let resetTimer = 0;
        copyButton.addEventListener("click", async () => {
            try {
                if (navigator.clipboard?.writeText) {
                    await navigator.clipboard.writeText(rawText);
                } else {
                    fallbackCopy(rawText);
                }

                window.clearTimeout(resetTimer);
                copyButton.textContent = "已复制";
                copyButton.classList.add("is-copied");
                resetTimer = window.setTimeout(() => {
                    copyButton.textContent = "复制";
                    copyButton.classList.remove("is-copied");
                }, 1800);
            } catch (error) {
                copyButton.textContent = "复制";
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
        const defaultLabel = button.dataset.copyLabelDefault || "复制链接";
        const successLabel = button.dataset.copyLabelSuccess || "已复制";
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
