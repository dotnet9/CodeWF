import {
  Button,
  ConfigProvider,
  Row,
  Col,
} from "antd";
import { TinyColor } from "@ctrl/tinycolor";
import { useState, useEffect } from "react";
import { getHomeTool } from "@/services/Home";
import hero from "./assets/hero-bg.png";

const colors1 = ["#6253E1", "#04BEFE"];
const getHoverColors = (colors) =>
  colors.map((color) => new TinyColor(color).lighten(5).toString());

const getActiveColors = (colors) =>
  colors.map((color) => new TinyColor(color).darken(5).toString());

import Head from "@/layouts/component/head/index";
import Footer from "@/layouts/component/footer/index";
import Links from "./Links";
import { openNewLink } from "@/utils/publicMethod";

function App() {
  useEffect(() => {
    requestTool();
  }, []);

  const [correlationTool, setcorrelationTool] = useState({
    tools: [],
    blogPosts: [],
    base: {},
  });
  const requestTool = async () => {
    let data = await getHomeTool();
    setcorrelationTool(data);
  };

  return (
    <div className="App">
      <Head base={correlationTool.base} menuItems={correlationTool.menu} />

      <div className="Carousel basicContent">
        <div className="CarouselLeft">
          <h1>
            <span>{correlationTool.base.name}</span>{" "}
          </h1>
          <p>{correlationTool.base.memo}</p>
          <ConfigProvider
            theme={{
              components: {
                Button: {
                  colorPrimary: `linear-gradient(135deg, ${colors1.join(
                    ", "
                  )})`,
                  colorPrimaryHover: `linear-gradient(135deg, ${getHoverColors(
                    colors1
                  ).join(", ")})`,
                  colorPrimaryActive: `linear-gradient(135deg, ${getActiveColors(
                    colors1
                  ).join(", ")})`,
                  lineWidth: 0,
                },
              },
            }}
          >
            <Button
              type="primary"
              size="large"
              onClick={() => {
                openNewLink(correlationTool.base.toolUrl);
              }}
            >
              在线工具
            </Button>
            <Button
              style={{ marginLeft: "2rem" }}
              type="primary"
              size="large"
              onClick={() => {
                openNewLink(correlationTool.base.blogPostUrl);
              }}
            >
              浏览博客
            </Button>
          </ConfigProvider>
        </div>

        <div className="CarouselRight">
          <div className="Masking">
            <div>
              <img src={hero} />
            </div>
          </div>
          <div className="Content">
            {correlationTool.base.featureKeywords?.map((item, index) => {
              return (
                <div className="Box" key={index}>
                  <span>{item}</span>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      <div className="basicContent main">
        <div className="mainBox">
          <h4>在线工具</h4>
          <div className="lableBox">
            {correlationTool.tools.map((item, index) => {
              return (
                <div
                  key={index}
                  onClick={() => {
                    openNewLink(item.url);
                  }}
                >
                  <img src={item.cover} />
                  <div className="title">{item.name}</div>
                  <div className="content">{item.memo}</div>
                </div>
              );
            })}
          </div>
          <h4>推荐博文</h4>
          <div className="lableBox">
            {correlationTool.blogPosts.map((item, index) => {
              return (
                <div
                  key={index}
                  onClick={() => {
                    openNewLink(item.url);
                  }}
                >
                  <img src={item.cover} />
                  <div className="title">{item.title}</div>
                  <div className="content">{item.memo}</div>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      <Links />

      <Footer base={correlationTool.base} />
    </div>
  );
}

export default App;
