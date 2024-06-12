import React from "react";
import ReactDOM from "react-dom/client";

import "@/assets/css/index.scss";
import "@/assets/css/index.css";
import "@/assets/css/antDesign.scss";
import "@/assets/css/responsive.scss";
import "@/assets/css/theme.scss";
import { Provider } from "react-redux";
import storeData from "@/store/index";
import { RouterProvider } from 'react-router-dom'


import Routes from '@/router/index.jsx'

console.log(Routes, 'Routes');

// const HotRoutes = hot(Routes)

ReactDOM.createRoot(document.getElementById("root")).render(

  <Provider store={storeData}>
    <React.StrictMode>
      <RouterProvider router={Routes}>
      </RouterProvider>
    </React.StrictMode>
  </Provider>
);
