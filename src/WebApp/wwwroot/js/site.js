document.addEventListener("DOMContentLoaded", () => {
    initializeHeaderOffset();
    initializeDesktopDropdowns();
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
    initializeReadingProgress();
    initializeCopyUrlButtons();
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
