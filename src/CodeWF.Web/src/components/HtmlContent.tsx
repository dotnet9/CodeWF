"use client";

import { useCallback, useEffect, useRef, useState, type MouseEvent } from "react";
import {
  ChevronLeft,
  ChevronRight,
  ExternalLink,
  Maximize2,
  RotateCcw,
  RotateCw,
  X,
  ZoomIn,
  ZoomOut
} from "lucide-react";

type RichImage = {
  src: string;
  alt: string;
  title: string;
  caption: string;
};

type ViewerState = {
  items: RichImage[];
  index: number;
  scale: number;
  rotation: number;
};

const MIN_SCALE = 0.7;
const MAX_SCALE = 3;
const SCALE_STEP = 0.25;
const ROTATION_STEP = 90;

export function HtmlContent({ html }: { html?: string }) {
  const contentRef = useRef<HTMLDivElement | null>(null);
  const [viewer, setViewer] = useState<ViewerState | null>(null);

  const closeViewer = useCallback(() => {
    setViewer(null);
  }, []);

  const updateViewer = useCallback((updater: (current: ViewerState) => ViewerState) => {
    setViewer((current) => (current ? updater(current) : current));
  }, []);

  const openImage = useCallback((image: HTMLImageElement) => {
    const container = contentRef.current;
    if (!container) {
      return;
    }

    const imageElements = Array.from(container.querySelectorAll<HTMLImageElement>("img"));
    const items = imageElements
      .map((item) => ({
        src: item.currentSrc || item.src,
        alt: item.alt.trim(),
        title: item.title.trim(),
        caption: item.closest("figure")?.querySelector("figcaption")?.textContent?.trim() ?? ""
      }))
      .filter((item) => Boolean(item.src));
    const index = imageElements.indexOf(image);

    if (index < 0 || items.length === 0) {
      return;
    }

    setViewer({
      items,
      index,
      scale: 1,
      rotation: 0
    });
  }, []);

  const handleClick = useCallback(
    (event: MouseEvent<HTMLDivElement>) => {
      const target = event.target;
      if (!(target instanceof Element)) {
        return;
      }

      const image = target.closest("img");
      if (!(image instanceof HTMLImageElement)) {
        return;
      }

      event.preventDefault();
      event.stopPropagation();
      openImage(image);
    },
    [openImage]
  );

  useEffect(() => {
    if (!viewer) {
      return;
    }

    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        event.preventDefault();
        closeViewer();
        return;
      }

      if (event.key === "ArrowLeft" && viewer.items.length > 1) {
        event.preventDefault();
        updateViewer((current) => ({
          ...current,
          index: (current.index - 1 + current.items.length) % current.items.length,
          scale: 1,
          rotation: 0
        }));
        return;
      }

      if (event.key === "ArrowRight" && viewer.items.length > 1) {
        event.preventDefault();
        updateViewer((current) => ({
          ...current,
          index: (current.index + 1) % current.items.length,
          scale: 1,
          rotation: 0
        }));
        return;
      }

      if (event.key === "+" || event.key === "=") {
        event.preventDefault();
        updateViewer((current) => ({ ...current, scale: Math.min(MAX_SCALE, current.scale + SCALE_STEP) }));
        return;
      }

      if (event.key === "-" || event.key === "_") {
        event.preventDefault();
        updateViewer((current) => ({ ...current, scale: Math.max(MIN_SCALE, current.scale - SCALE_STEP) }));
        return;
      }

      if (event.key === "0") {
        event.preventDefault();
        updateViewer((current) => ({ ...current, scale: 1, rotation: 0 }));
      }
    };

    document.addEventListener("keydown", onKeyDown);

    return () => {
      document.body.style.overflow = previousOverflow;
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [closeViewer, updateViewer, viewer]);

  if (!html) {
    return null;
  }

  const currentItem = viewer ? viewer.items[viewer.index] : null;
  const title = currentItem?.caption || currentItem?.title || currentItem?.alt || "Image preview";
  const indexLabel = viewer && viewer.items.length > 1 ? `${viewer.index + 1} / ${viewer.items.length}` : "";

  const navigate = (direction: -1 | 1) => {
    if (!viewer) {
      return;
    }

    updateViewer((current) => ({
      ...current,
      index: (current.index + direction + current.items.length) % current.items.length,
      scale: 1,
      rotation: 0
    }));
  };

  const openSource = () => {
    if (!currentItem || typeof window === "undefined") {
      return;
    }

    window.open(currentItem.src, "_blank", "noopener,noreferrer");
  };

  return (
    <>
      <div ref={contentRef} className="rich-content" dangerouslySetInnerHTML={{ __html: html }} onClick={handleClick} />
      {viewer && currentItem ? (
        <div className="rich-image-viewer" role="dialog" aria-modal="true" aria-label={title} onClick={closeViewer}>
          <div className="rich-image-viewer__shell" onClick={(event) => event.stopPropagation()}>
            <div className="rich-image-viewer__toolbar">
              <div className="rich-image-viewer__meta">
                <strong>{title}</strong>
                <small>{indexLabel}</small>
              </div>
              <div className="rich-image-viewer__actions">
                <button
                  type="button"
                  className="rich-image-viewer__button"
                  onClick={() => updateViewer((current) => ({ ...current, scale: Math.max(MIN_SCALE, current.scale - SCALE_STEP) }))}
                  aria-label="Zoom out"
                  title="Zoom out"
                >
                  <ZoomOut size={16} aria-hidden="true" />
                </button>
                <button
                  type="button"
                  className="rich-image-viewer__button"
                  onClick={() => updateViewer((current) => ({ ...current, scale: 1, rotation: 0 }))}
                  aria-label="Reset view"
                  title="Reset view"
                >
                  <Maximize2 size={16} aria-hidden="true" />
                </button>
                <button
                  type="button"
                  className="rich-image-viewer__button"
                  onClick={() => updateViewer((current) => ({ ...current, scale: Math.min(MAX_SCALE, current.scale + SCALE_STEP) }))}
                  aria-label="Zoom in"
                  title="Zoom in"
                >
                  <ZoomIn size={16} aria-hidden="true" />
                </button>
                <button
                  type="button"
                  className="rich-image-viewer__button"
                  onClick={() => updateViewer((current) => ({ ...current, rotation: current.rotation - ROTATION_STEP }))}
                  aria-label="Rotate left"
                  title="Rotate left"
                >
                  <RotateCcw size={16} aria-hidden="true" />
                </button>
                <button
                  type="button"
                  className="rich-image-viewer__button"
                  onClick={() => updateViewer((current) => ({ ...current, rotation: current.rotation + ROTATION_STEP }))}
                  aria-label="Rotate right"
                  title="Rotate right"
                >
                  <RotateCw size={16} aria-hidden="true" />
                </button>
                <button
                  type="button"
                  className="rich-image-viewer__button"
                  onClick={openSource}
                  aria-label="Open original image"
                  title="Open original image"
                >
                  <ExternalLink size={16} aria-hidden="true" />
                </button>
                <button
                  type="button"
                  className="rich-image-viewer__button rich-image-viewer__button--close"
                  onClick={closeViewer}
                  aria-label="Close image viewer"
                  title="Close"
                >
                  <X size={16} aria-hidden="true" />
                </button>
              </div>
            </div>

            <div className="rich-image-viewer__stage">
              {viewer.items.length > 1 ? (
                <button
                  type="button"
                  className="rich-image-viewer__nav rich-image-viewer__nav--prev"
                  onClick={() => navigate(-1)}
                  aria-label="Previous image"
                  title="Previous image"
                >
                  <ChevronLeft size={20} aria-hidden="true" />
                </button>
              ) : null}

              <div className="rich-image-viewer__frame">
                <img
                  className="rich-image-viewer__image"
                  src={currentItem.src}
                  alt={currentItem.alt || currentItem.caption || currentItem.title || ""}
                  draggable={false}
                  style={{
                    transform: `scale(${viewer.scale}) rotate(${viewer.rotation}deg)`
                  }}
                />
              </div>

              {viewer.items.length > 1 ? (
                <button
                  type="button"
                  className="rich-image-viewer__nav rich-image-viewer__nav--next"
                  onClick={() => navigate(1)}
                  aria-label="Next image"
                  title="Next image"
                >
                  <ChevronRight size={20} aria-hidden="true" />
                </button>
              ) : null}
            </div>
          </div>
        </div>
      ) : null}
    </>
  );
}
