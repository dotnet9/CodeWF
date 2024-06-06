function footer({ base }) {
  return (
    <footer>
      <div className="footer basicLayout basicContent">
        <div className="footerRight">
          <div className="footerRightBox">
            {base.weChatPublic?.map((item, index) => {
              return (
                index == 0 && (
                  <div className="QRCodeBpx" key={index}>
                    <img src={item.qrCode} />
                    <div>微信公众号</div>
                  </div>
                )
              );
            })}
            <div className="QRCodeBpx">
              <img src={base.ownerWeChat} />
              <div>站长微信</div>
            </div>
          </div>
        </div>
        <div className="footerLeft">
          <div className="iconBox">
            <img src={base.logo} />
            {base.name}
          </div>

          <div className="iconInfo">
            本站原创文章，转载无需和站长联系，但请注明来自码界工坊{" "}
            <a href="https://codewf.com" target="_blank">
              https://codewf.com
            </a>
            <br />
            本站由&nbsp;
            <a href="https://dotnet.microsoft.com/zh-cn/" target="_blank">
              .NET 9.0
            </a>
            &nbsp;+&nbsp;
            <a href="https://cn.vuejs.org/" target="_blank">
              Vue 3.0
            </a>
            &nbsp;强力驱动！ | &nbsp; Copyright ©{" "}
            <a href="https://codewf.com" target="_blank">
              https://codewf.com
            </a>{" "}
            All Rights Reserved. &nbsp;|&nbsp; 备案号：
            <a href="https://beian.miit.gov.cn/" target="_blank">
              蜀ICP备19038256号-1
            </a>
          </div>
        </div>
      </div>
    </footer>
  );
}

export default footer;
