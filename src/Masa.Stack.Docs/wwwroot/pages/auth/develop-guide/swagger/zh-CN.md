# Swagger

**Swagger** 可以看作接口文档，用于浏览 `Auth` 的接口，**MASA Auth** 专门针对 **Swagger** 做了安全认证功能，只有登录后才可以在 **Swagger** 页面中进行接口操作。

## 未登录时执行接口会报401未授权错误

![Swagger 401未授权图](https://cdn.masastack.com/stack/doc/auth/develop-guide/swagger-401.png)

## 完成登录认证

点击 **Swagger** 页面中的 `Authorize` 按钮，打开认证弹窗

![Swagger Authorize认证图](https://cdn.masastack.com/stack/doc/auth/develop-guide/swagger-authorize-button.png)

![Swagger Authorize认证登录图](https://cdn.masastack.com/stack/doc/auth/develop-guide/swagger-authorize.png)

输入账号密码认证后，页面会显示如下

![Swagger Authorize认证成功图](https://cdn.masastack.com/stack/doc/auth/develop-guide/swagger-authorize-success.png)

认证完成后调用 `api/user/Select` 接口，返回 200

![Swagger 获取用户信息图](https://cdn.masastack.com/stack/doc/auth/develop-guide/swagger-200.png)