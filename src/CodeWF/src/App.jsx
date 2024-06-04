import { Button, ConfigProvider, Row, Col, Dropdown, Space, Drawer, Input, Popover, QRCode } from 'antd';
import { TinyColor } from '@ctrl/tinycolor';
import { useState, useEffect } from 'react';
import { DownOutlined, GithubFilled, RobotFilled, MergeFilled, Html5Filled, DatabaseFilled, ProductFilled, } from '@ant-design/icons';
import './index.scss'
import './antDesign.scss'
import { getHomeLinks, getHomeTool } from "@/services/Home";

//img

import logo from "./assets/avatar.png";
import hero from "./assets/hero-bg.png";
import QRCode_icon from "./assets/wechatpublic.jpg";

const { Search } = Input;
const colors1 = ['#6253E1', '#04BEFE'];
const getHoverColors = (colors) =>
  colors.map((color) => new TinyColor(color).lighten(5).toString());

const getActiveColors = (colors) =>
  colors.map((color) => new TinyColor(color).darken(5).toString());

function App() {


  useEffect(() => {
    requestLinks()
    requestTool()
  }, [])

  const items = [
    {
      key: '1',
      label: (
        <span>
          .NET
        </span>
      ),
      icon: <GithubFilled />,
    },
    {
      key: '2',
      label: (
        <span>
          分享
        </span>
      ),
      icon: <MergeFilled />
    },
    {
      key: '3',
      label: (
        <span>
          更多语言
        </span>
      ),
      icon: <ProductFilled />
    },
    {
      key: '3',
      label: (
        <span>
          课程
        </span>
      ),
      icon: <DatabaseFilled />
    },
    {
      key: '3',
      label: (
        <span>
          前端
        </span>
      ),
      icon: <Html5Filled />,
    },
    {
      key: '3',
      label: (
        <span>
          数据库
        </span>
      ),
      icon: <MergeFilled />
    },
    {
      key: '3',
      label: (
        <span>
          Python
        </span>
      ),
      icon: <RobotFilled />
    },

  ];


  const [FriendshipLink, setFriendshipLink] = useState([])
  const [correlationTool, setcorrelationTool] = useState({ tools: [], blogPosts: [], base: {} })


  const requestLinks = async () => {
    let data = await getHomeLinks()
    FriendshipLink.length = 0
    setFriendshipLink([...data])
  }

  const requestTool = async () => {
    let data = await getHomeTool()
    setcorrelationTool(data)
  }

  const [open, setOpen] = useState(false);
  const showDrawer = () => {
    setOpen(true);
  };

  const onClose = () => {
    setOpen(false);
  };

  const [PopoverState, setPopoverState] = useState(false);


  const hide = () => {
    setPopoverState(false);
  };

  const handleOpenChange = (newOpen) => {
    setPopoverState(newOpen);
  };

  const openNewLink = (link) => {
    window.open(link, '_blank');
  }



  return (
    <div className="App">
      <header>
        <div className='header basicLayout basicContent'>
          <a className='iconBox'>
            <img src={correlationTool.base.logo} />
            {correlationTool.base.name}
          </a>
          <div className='navbarNav'>

            <Dropdown
              menu={{
                items,
              }}
            >
              <a onClick={(e) => e.preventDefault()}>
                <Space>
                  分类
                  <DownOutlined />
                </Space>
              </a>
            </Dropdown>

            <a onClick={showDrawer} className='decoration'>
              标签
            </a>

            <a className='decoration'>
              归档
            </a>
            <a className='decoration'>
              ABout
            </a>
          </div>
          <div className='navbarSearch'>

            <Popover
              content={<a onClick={hide}>Close</a>}
              title="Title"
              trigger="click"
              open={PopoverState}
              onOpenChange={handleOpenChange}
            >
              <Space.Compact>
                <Search placeholder="input search text" allowClear />
              </Space.Compact>
            </Popover>

          </div>
        </div>

      </header>


      <div className='Carousel'>
        <Row>
          <Col span={6} offset={4}>
            <div className='CarouselLeft'>
              <h1><span>{correlationTool.base.name}</span> </h1>
              <p>{correlationTool.base.memo}</p>
              <ConfigProvider
                theme={{
                  components: {
                    Button: {
                      colorPrimary: `linear-gradient(135deg, ${colors1.join(', ')})`,
                      colorPrimaryHover: `linear-gradient(135deg, ${getHoverColors(colors1).join(', ')})`,
                      colorPrimaryActive: `linear-gradient(135deg, ${getActiveColors(colors1).join(', ')})`,
                      lineWidth: 0,
                    },
                  },
                }}
              >
                <Button type="primary" size="large" onClick={openNewLink(correlationTool.base.toolUrl)}>
                  在线工具
                </Button>
                <Button type="primary" size="large" onClick={openNewLink(correlationTool.base.blogPostUrl)}>
                  浏览博客
                </Button>
              </ConfigProvider>
            </div>
          </Col>
          <Col span={7} offset={3}>
            <div className='CarouselRight'>
              <div className='Masking'>
                <div>
                  <img src={hero} />
                </div>
              </div>
              <div className='Content'>
                {
                  correlationTool.base.featureKeywords?.map((item,index) => {
                    return <div className='Box' key={index}>
                      <span>{item}</span>
                    </div>
                  })
                }
              </div>

            </div>
          </Col>
        </Row>

      </div>


      <div className='basicContent main'>
        <div className='mainBox'>
          <h4>在线工具</h4>
          <div className='lableBox'>
            {
              correlationTool.tools.map((item, index) => {
                return <div key={index}>
                  <img src={item.cover} />
                  <div className='title'>{item.name}</div>
                  <div className='content'>{item.memo}</div>
                </div>
              })
            }
          </div>
          <h4>推荐博文</h4>
          <div className='lableBox'>
            {
              correlationTool.blogPosts.map((item, index) => {
                return <div key={index}>
                  <img src={item.cover} />
                  <div className='title'>{item.title}</div>
                  <div className='content'>{item.memo}</div>
                </div>
              })
            }
          </div>
        </div>
      </div>



      <footer>
        <div className='footer basicLayout basicContent'>
          <div className='footerLeft'>
            <div className='iconBox'>
              <img src={logo} />
              dotnet9
            </div>

            <div className='iconInfo'>
              {
                correlationTool.base.memo
              }
            </div>

            <div className='emallBox'>
              <p>Contact blogger</p>
              <Space.Compact>
                <Input placeholder="input search text" allowClear />
                <Button type="primary" className='basicBtnColor'>Submit</Button>
              </Space.Compact>
            </div>
            <div className='copyright'> 本站由 .NET 9.0 + Vue 3.0 强力驱动！| ©Copyright {correlationTool.base.owner} 保留所有权利</div>


          </div>

          <div className='footerContent'>
            <div>
              <h5>友情链接：</h5>
              <div className='footerList'>
                {
                  FriendshipLink.map((item) => {
                    return <p key={item.rank} className='footerP'>{item.title}</p>
                  })
                }

              </div>
            </div>

          </div>

          <div>
            <p className='footerP'>
              Contact Us
            </p>
            <div className='address decoration'>
              email@dotnet9.com
            </div>

            <img style={{ marginTop: "40px" }} width={"150px"} src={QRCode_icon} />

            <div style={{ color: "#ccc", marginTop: "10px" }}>
              微信号
            </div>

          </div>



        </div>

      </footer>


      <Drawer title="Basic Drawer" onClose={onClose} open={open}>
        <p>Some contents...</p>
        <p>Some contents...</p>
        <p>Some contents...</p>
      </Drawer>
    </div >



  );
}





export default App;
