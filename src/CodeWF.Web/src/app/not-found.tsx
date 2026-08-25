import Link from "next/link";

const exits = [
  { href: "/zh-CN", icon: "⌂", title: "回首页", command: "cd ~" },
  { href: "/zh-CN/post", icon: "#", title: "逛文章", command: "cd ~/blog" },
  { href: "/zh-CN/tool", icon: "$", title: "开工具", command: "cd ~/tool" },
  { href: "/zh-CN/s", icon: "⌕", title: "搜一搜", command: "grep -r …" }
];

export default function NotFound() {
  return (
    <div className="not-found-shell">
      <header className="not-found-header">
        <Link className="brand" href="/zh-CN"><span className="brand-icon">码</span><span>Code<em>WF</em></span></Link>
        <nav><Link href="/zh-CN">首页</Link><Link href="/zh-CN/post">文章</Link><Link href="/zh-CN/album">专题</Link><Link href="/zh-CN/tool">工具</Link><Link href="/zh-CN/about">关于</Link></nav>
      </header>
      <main className="not-found-page">
        <div className="not-found-heading"><h1>页面未找到</h1><span>exit code 404</span></div>
        <div className="not-found-terminal">
          <div className="not-found-terminal__bar"><i /><i /><i /><span>codewf@dotnet9 — zsh</span></div>
          <div className="not-found-terminal__body">
            <div><b>$</b> curl -I <strong>https://codewf.com/the-page-you-wanted</strong></div>
            <div className="error">HTTP/1.1 404 <span>· Not Found</span></div>
            <div>zsh: no such file or directory: <em>~/the-page-you-wanted</em></div>
            <div># 可能是：链接已变更、文章已归档，或你手滑敲错了一个字符。</div>
            <div><b>$</b> <span className="not-found-cursor" /></div>
          </div>
        </div>
        <div className="not-found-code">404</div>
        <p className="not-found-hint">// 别慌，路由迷路了，内容还在</p>
        <div className="not-found-exits">{exits.map((item) => <Link className="not-found-exit" href={item.href} key={item.href}><span>{item.icon}</span><b>{item.title}</b><small>{item.command}</small></Link>)}</div>
        <p className="not-found-tip">快捷键 <kbd>/</kbd> 或 <kbd>Ctrl K</kbd> 直接唤起全站搜索</p>
      </main>
      <footer className="not-found-footer">© 2019-2026 CodeWF · dotnet9.com <span>RSS · Sitemap</span></footer>
    </div>
  );
}
