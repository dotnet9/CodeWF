import { useEffect, useRef, useState, type MouseEvent } from "react";
import {
  CloseOutlined,
  ExpandOutlined,
  LeftOutlined,
  LinkOutlined,
  RightOutlined,
  RotateLeftOutlined,
  RotateRightOutlined,
  ZoomInOutlined,
  ZoomOutOutlined
} from "@ant-design/icons";

type RichPreviewImage = {
  src: string;
  alt: string;
  title: string;
  caption: string;
};

type RichPreviewViewer = {
  items: RichPreviewImage[];
  index: number;
  scale: number;
  rotation: number;
};

const PREVIEW_MIN_SCALE = 0.7;
const PREVIEW_MAX_SCALE = 3;
const PREVIEW_SCALE_STEP = 0.25;
const PREVIEW_ROTATION_STEP = 90;

export function RichPreview({ html }: { html: string }) {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const [viewer, setViewer] = useState<RichPreviewViewer | null>(null);

  const closeViewer = () => {
    setViewer(null);
  };

  const updateViewer = (updater: (current: RichPreviewViewer) => RichPreviewViewer) => {
    setViewer((current) => (current ? updater(current) : current));
  };

  const openImage = (image: HTMLImageElement) => {
    const container = containerRef.current;
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
  };

  const handleClick = (event: MouseEvent<HTMLDivElement>) => {
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
  };

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
        updateViewer((current) => ({ ...current, scale: Math.min(PREVIEW_MAX_SCALE, current.scale + PREVIEW_SCALE_STEP) }));
        return;
      }

      if (event.key === "-" || event.key === "_") {
        event.preventDefault();
        updateViewer((current) => ({ ...current, scale: Math.max(PREVIEW_MIN_SCALE, current.scale - PREVIEW_SCALE_STEP) }));
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
  }, [viewer]);

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
      <div ref={containerRef} className="rich-preview rich-preview--interactive" dangerouslySetInnerHTML={{ __html: html }} onClick={handleClick} />
      {viewer && currentItem ? (
        <div className="rich-preview-viewer" role="dialog" aria-modal="true" aria-label={title} onClick={closeViewer}>
          <div className="rich-preview-viewer__shell" onClick={(event) => event.stopPropagation()}>
            <div className="rich-preview-viewer__toolbar">
              <div className="rich-preview-viewer__meta">
                <strong>{title}</strong>
                <small>{indexLabel}</small>
              </div>
              <div className="rich-preview-viewer__actions">
                <button
                  type="button"
                  className="rich-preview-viewer__button"
                  onClick={() => updateViewer((current) => ({ ...current, scale: Math.max(PREVIEW_MIN_SCALE, current.scale - PREVIEW_SCALE_STEP) }))}
                  aria-label="Zoom out"
                  title="Zoom out"
                >
                  <ZoomOutOutlined />
                </button>
                <button
                  type="button"
                  className="rich-preview-viewer__button"
                  onClick={() => updateViewer((current) => ({ ...current, scale: 1, rotation: 0 }))}
                  aria-label="Reset view"
                  title="Reset view"
                >
                  <ExpandOutlined />
                </button>
                <button
                  type="button"
                  className="rich-preview-viewer__button"
                  onClick={() => updateViewer((current) => ({ ...current, scale: Math.min(PREVIEW_MAX_SCALE, current.scale + PREVIEW_SCALE_STEP) }))}
                  aria-label="Zoom in"
                  title="Zoom in"
                >
                  <ZoomInOutlined />
                </button>
                <button
                  type="button"
                  className="rich-preview-viewer__button"
                  onClick={() => updateViewer((current) => ({ ...current, rotation: current.rotation - PREVIEW_ROTATION_STEP }))}
                  aria-label="Rotate left"
                  title="Rotate left"
                >
                  <RotateLeftOutlined />
                </button>
                <button
                  type="button"
                  className="rich-preview-viewer__button"
                  onClick={() => updateViewer((current) => ({ ...current, rotation: current.rotation + PREVIEW_ROTATION_STEP }))}
                  aria-label="Rotate right"
                  title="Rotate right"
                >
                  <RotateRightOutlined />
                </button>
                <button
                  type="button"
                  className="rich-preview-viewer__button"
                  onClick={openSource}
                  aria-label="Open original image"
                  title="Open original image"
                >
                  <LinkOutlined />
                </button>
                <button
                  type="button"
                  className="rich-preview-viewer__button rich-preview-viewer__button--close"
                  onClick={closeViewer}
                  aria-label="Close image viewer"
                  title="Close"
                >
                  <CloseOutlined />
                </button>
              </div>
            </div>

            <div className="rich-preview-viewer__stage">
              {viewer.items.length > 1 ? (
                <button
                  type="button"
                  className="rich-preview-viewer__nav rich-preview-viewer__nav--prev"
                  onClick={() => navigate(-1)}
                  aria-label="Previous image"
                  title="Previous image"
                >
                  <LeftOutlined />
                </button>
              ) : null}

              <div className="rich-preview-viewer__frame">
                <img
                  className="rich-preview-viewer__image"
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
                  className="rich-preview-viewer__nav rich-preview-viewer__nav--next"
                  onClick={() => navigate(1)}
                  aria-label="Next image"
                  title="Next image"
                >
                  <RightOutlined />
                </button>
              ) : null}
            </div>
          </div>
        </div>
      ) : null}
    </>
  );
}
