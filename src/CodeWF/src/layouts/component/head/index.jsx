
import { Button, ConfigProvider, Row, Col, Dropdown, Space, Drawer, Input, Popover, QRCode } from 'antd';
import { DownOutlined, GithubFilled, RobotFilled, MergeFilled, Html5Filled, DatabaseFilled, ProductFilled, } from '@ant-design/icons';

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




function head({ base, menu }) {
    return (
        <header>
            <div className='header basicLayout basicContent'>
                <a className='iconBox'>
                    <img src={base.logo} />
                    {base.name}
                </a>
                <div className='navbarNav'>
                    <a href='/' className='decoration'>
                        首页
                    </a>
                    {
                        menu?.map((item, index) => {
                            return <Dropdown
                                key={index}
                                menu={{
                                    items
                                }}
                            >
                                <a onClick={(e) => e.preventDefault()}>
                                    <Space>
                                        分类
                                        <DownOutlined />
                                    </Space>
                                </a>
                            </Dropdown>
                        })
                    }
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

                    <a href='/about' className='decoration'>
                        关于
                    </a>
                </div>
            </div>
        </header>
    )
}

export default head;