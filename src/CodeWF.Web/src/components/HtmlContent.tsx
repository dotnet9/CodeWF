export function HtmlContent({ html }: { html?: string }) {
  if (!html) {
    return null;
  }

  return <div className="rich-content" dangerouslySetInnerHTML={{ __html: html }} />;
}
