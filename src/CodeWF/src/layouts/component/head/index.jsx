import { Dropdown, Space } from "antd";
import { DownOutlined } from "@ant-design/icons";

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

function head({ base, menuItems }) {
  return (
    <header>
      <div className="header basicLayout basicContent">
        <a className="iconBox">
          <img src={base.logo} />
          {base.name}
        </a>
        <div className="navbarNav">
          <a href="/" className="decoration">
            首页
          </a>
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

          <a href="/about" className="decoration">
            关于
          </a>
        </div>
      </div>
    </header>
  );
}

export default head;
