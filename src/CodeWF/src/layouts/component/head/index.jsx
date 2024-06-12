import { Dropdown, Space, Switch } from "antd";
import { DownOutlined } from "@ant-design/icons";
import { useEffect, useState } from "react";
import { Link, Outlet } from "react-router-dom";


function converMenu(menuItems) {
  let formatMenuItems = menuItems.map((menuItem, index) => {
    return {
      key: menuItem.name,
      label: (
        <a href={menuItem.url} target="_blank">
          {menuItem.name}
        </a>
      ),
    };
  });
  return {
    items: formatMenuItems,
  };
}

// localStorage 获取当前的主题模式  dark ，light





function head({ base, menuItems }) {
  const [theme, setTheme] = useState(false)

  useEffect(() => {
    if (localStorage.theme === 'Dark') {
      setTheme(true)
    }
  }, [])

  useEffect((e) => {
    if (theme) {
      document.documentElement.setAttribute('theme', 'light');
    } else {
      document.documentElement.removeAttribute('theme');
    }
  }, [theme])



  const onChange = (checked) => {
    localStorage.theme = checked ? 'Dark' : "Light"
    setTheme(checked)


  };
  return (
    <header>
      <div className="header basicLayout basicContent">
        <a className="iconBox">
          <img src={base.logo} />
          {base.name}
        </a>
        <div className="navbarNav">

        <Link to="/home" className="decoration">
            首页
          </Link>
          {menuItems?.map((menuItem, index) => (
            <Dropdown
              key={index}
              menu={converMenu(menuItem.children)} //{items}//
            >
              <a onClick={(e) => e.preventDefault()}>
                <Space>
                  {menuItem.name}
                  <DownOutlined />
                </Space>
              </a>
            </Dropdown>
          ))}

          <Link to="/about" className="decoration">
            关于
          </Link>

          <div className="responsiveBox">
            Light&nbsp;&nbsp;
            <Switch checked={theme} onChange={onChange} />
            &nbsp;&nbsp;Dark
          </div>
        </div>
      </div>
    </header>
  );
}

export default head;
