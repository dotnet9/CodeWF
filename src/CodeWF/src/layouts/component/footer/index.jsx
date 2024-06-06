



import logo from "./assets/avatar.png";
function footer({ base }) {
    return (
        <footer>
            <div className='footer basicLayout basicContent'>
                <div className='footerRight'>
                    <div className='footerRightBox'>
                        <div className='QRCodeBpx'>
                            <img src={base.ownerWeChat} />
                            <div>
                                微信号：codewf
                            </div>
                        </div>
                        {
                            base.weChatPublic?.map((item, index) => {
                                return index == 0 && <div className='QRCodeBpx' key={index}>
                                    <img src={item.qrCode} />
                                    <div>
                                        {item.name}(公众号)
                                    </div>
                                </div>
                            })
                        }

                    </div>



                </div>
                <div className='footerLeft'>
                    <div className='iconBox'>
                        <img src={logo} />
                        {
                            base.name
                        }
                    </div>

                    <div className='iconInfo'>
                        {
                            base.memo
                        }
                    </div>
                    <div className='copyright'> 本站由 .NET 9.0 + Vue 3.0 强力驱动！| ©Copyright {base.owner} 保留所有权利</div>
                </div>








            </div>


        </footer>
    )
}

export default footer;