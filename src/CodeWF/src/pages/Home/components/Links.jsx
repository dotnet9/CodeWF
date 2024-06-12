import { changeAllLinksAction } from "@/store/actionCreactors";
import { useSelector, useDispatch } from "react-redux";

import { getHomeLinks } from "@/services/Home";

import { useEffect } from "react";
import { openNewLink } from "@/utils/publicMethod";

function Links() {
  const AllLinks = useSelector((state) => state.AllLinks);
  const dispatch = useDispatch();
  const requestLinks = async () => {
    let data = await getHomeLinks();
    dispatch(changeAllLinksAction(data));
  };
  useEffect(() => {
    requestLinks();
  }, []);
  return (
    <div className="linkContent basicContent">
      <div>
        <h5 onClick={requestLinks}>友情链接：</h5>
        <div className="footerList">
          {AllLinks.map((item, index) => {
            return (
              <p
                key={index}
                className="footerP"
                onClick={() => {
                  openNewLink(item.linkUrl);
                }}
              >
                {item.title}
              </p>
            );
          })}
        </div>
      </div>
    </div>
  );
}

export default Links;
// export default connect(mapStateToProps, mapDispatchToProps)(Links);
