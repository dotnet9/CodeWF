
import { useState, useEffect } from "react";
import { getHomeTool } from "@/services/Home";
import { Link, Outlet } from "react-router-dom";
import Head from "@/layouts/component/head/index";
import Footer from "@/layouts/component/footer/index";
import { useSelector, useDispatch } from "react-redux";
import { changeCorrelationTool } from "@/store/actionCreactors";
function App() {
  useEffect(() => {
    requestTool();
  }, []);

  const correlationTool = useSelector((state) => state.correlationTool);
  const dispatch = useDispatch();
  const requestTool = async () => {
    let data = await getHomeTool();
    dispatch(changeCorrelationTool(data));
  };

  return (
    <div className="App">
      <Head base={correlationTool.base} menuItems={correlationTool.menu} />



      <Outlet base={correlationTool}></Outlet>

      <Footer base={correlationTool.base} />
    </div>
  );
}

export default App;
