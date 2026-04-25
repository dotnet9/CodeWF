document.addEventListener("DOMContentLoaded", () => {
    initializeHeaderOffset();
    initializeDesktopDropdowns();
    initializeReadingToc();
});

function initializeHeaderOffset() {
    const root = document.documentElement;
    const navbar = document.querySelector(".site-navbar");

    if (!root || !navbar) {
        return;
    }

    const updateOffset = () => {
        const height = Math.ceil(navbar.getBoundingClientRect().height);
        root.style.setProperty("--site-header-offset", `${Math.max(height + 18, 96)}px`);
    };

    updateOffset();
    window.addEventListener("resize", updateOffset);

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
            tocRoot.closest(".article-aside, .content-sidebar")?.setAttribute("hidden", "hidden");
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
