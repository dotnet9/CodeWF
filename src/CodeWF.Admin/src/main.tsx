import "@ant-design/v5-patch-for-react-19";
import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./styles.css";

document.title = "CodeWF 后台";
const iconLink = document.querySelector("link[rel~='icon']") ?? document.createElement("link");
iconLink.setAttribute("rel", "icon");
iconLink.setAttribute("href", `${import.meta.env.BASE_URL}logo.ico`);
if (!iconLink.parentNode) {
  document.head.appendChild(iconLink);
}

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
