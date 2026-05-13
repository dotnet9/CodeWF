import React from "react";
import ReactDOM from "react-dom/client";
import { ConfigProvider } from "antd";
import zhCN from "antd/locale/zh_CN";
import App from "./App";
import "./styles.css";

document.title = "CodeWF 后台";
const iconLink = document.querySelector("link[rel~='icon']") ?? document.createElement("link");
iconLink.setAttribute("rel", "icon");
iconLink.setAttribute("href", (import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5100/api").replace(/\/$/, "").replace(/\/api$/, "") + "/site/favicon/logo.ico");
if (!iconLink.parentNode) {
  document.head.appendChild(iconLink);
}

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <ConfigProvider locale={zhCN} theme={{ token: { colorPrimary: "#0f766e", borderRadius: 8 } }}>
      <App />
    </ConfigProvider>
  </React.StrictMode>
);
