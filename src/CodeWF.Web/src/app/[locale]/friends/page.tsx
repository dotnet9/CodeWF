import { api, resolveAssetUrl } from "@/api";
import type { LocalePageProps } from "../layout";

export default async function FriendsPage({ params }: LocalePageProps) {
  const { locale } = await params;
  const [home, friends] = await Promise.all([api.home(locale), api.friendLinks(locale)]);

  return (
    <main className="page-wrap prototype-friends-page">
      <div className="section-head">
        <div>
          <span className="eyebrow">FRIEND LINKS</span>
          <h1>友情链接</h1>
          <p>独立博客、开发者社区和长期维护的技术站点。</p>
        </div>
      </div>
      <div className="prototype-friend-grid">
        {friends.slice().sort((left, right) => left.index - right.index).map((friend) => {
          const logo = resolveAssetUrl(home.site, friend.logo);
          return (
            <a href={friend.link} target="_blank" rel="noreferrer" className="prototype-card prototype-friend-card" key={`${friend.title}-${friend.link}`}>
              {logo ? <span className="prototype-friend-card__logo"><img src={logo} alt="" loading="lazy" /></span> : <span className="prototype-friend-card__logo prototype-friend-card__logo--empty">{friend.title?.slice(0, 1)}</span>}
              <span><strong>{friend.title}</strong><small>{friend.description}</small></span>
            </a>
          );
        })}
      </div>
    </main>
  );
}
