# 基于 Docker Compose 本地部署 MASA Stack

本文介绍如何部署 MASA Stack，为了方便大家本地体验，在环境准备环节我们会基于 [Docker Desktop](https://www.docker.com/products/docker-desktop/)，它同样适用于生产环境的 K8s


## 先决条件

* [Docker](https://www.docker.com/)
* [Dapr](https://dapr.io/)



## 环境准备

本文档默认你已经安装 [Docker Desktop](https://www.docker.com/products/docker-desktop/) 和 [WSL](https://learn.microsoft.com/en-us/windows/wsl/about)，并且基于 Windows 11+ 的 [Windows Terminal](https://learn.microsoft.com/en-us/windows/terminal/)


### 下载 Docker Compose 文件

`Clone` 仓库 [masahelm](https://github.com/masastack/helm),仓库中 `docker` 文件夹包含 `Docker Compose` 相关文件。

> 账号：admin
>
> 密码：admin123
>
> 初始化默认密码可以通过 `docker/variables.env` 下 `ADMIN_PWD` 值修改。

## 安装 MASA Stack

`docker-compose.yml` 目录先执行命令：
```shell
docker-compose up
```

> 由于 `SSO` 服务,需要本地和容器内都要能访问，在没有域名的情况下，增加了 `nginx` 代理，需要修改 `host` 映射文件增加 `127.0.0.1 sso`。

看到如下效果表示启动成功：

![Docker Compose](https://cdn.masastack.com/stack/doc/stack/docker-compose.png)

> 虽然 Docker Compose 文件内依赖 `depends_on` 限制了服务的启动顺序，但是仍不能100%保证服务启动时前置数据初始完成。如果遇到部分服务启动失败，请手动按照 `PM Service` -> `DCC Service` -> `Auth Service` -> `Other Service` -> `SSO` -> `Other Web` 的顺序重启即可。

浏览器输入网址 `https://localhost:5401/` （PM），访问跳转如下：

![Login](https://cdn.masastack.com/stack/doc/stack/docker_stack_login.png)

## 卸载 MASA Stack

`docker-compose.yml` 目录先执行命令：

```shell
   docker-compose down
```