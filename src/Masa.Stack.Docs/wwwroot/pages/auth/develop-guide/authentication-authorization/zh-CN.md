## Auth 认证授权

在 `Auth` 中权限设计主要采用了 `OAuth 2` 、`OIDC` 、`JWT` 、`Identity Server 4` 协议。其中利用 `OAuth 2` 流程与外部应用授权信息，使用 `OIDC` 验证授权信息并交互获取终端用户概要信息，使用 `JWT` 封装终端用户概要信息并加密生成 `Token` ，使用 `IDentity Server 4` 管理需要获取的权限范围。

### OAuth 2

#### 基础概念
`OAuth` 是一个开放授权标准，它允许用户让第三方应用访问该用户在某服务的特定私有资源，但可以不提供账号密码信息给第三方应用，`OAuth 2` 是其 `2.0` 版本。使用 `OAuth 2` 可以给我们带来好处：防止密码泄露、提供受限访问范围、权限回收。它由四种角色参与组成，即：资源拥有者、资源服务器、第三方应用、授权服务器。总共有四种工作流程模式，即：授权码模式、隐式授权模式、用户名密码模式、客户端模式。

**OAuth 2 中的四个角色**

|角色|说明|
| :-: | :-: |
|资源拥有者|即用户本身|
|资源服务器|提供资源的业务系统|
|第三方应用|请求资源的第三方应用|
|授权服务器|管理 `Token` 的授权系统|

**OAuth 2 工作流程总共包含以下四种模式**

|模式|特点|
|-:|:-|
|授权码模式|`Token` 存放在客户端的后台，安全性最高，适合外部用户登录访问|
|隐式授权模式|`Token` 令牌保存在前端（如浏览器），不安全|
|用户名密码模式|用户名和密码需要提交给客户端，不太安全，适合对客户端应用相当信任的场景，比如公司内部用户访问公司内部系统|
|客户端模式|无需用户授权，由第三方服务与授权服务器进行 `Token` 获取|

四种模式其授权流程基本相同，即第三方应用请求资源，用户确认授权信息，授权服务器发放 `Token` 令牌，第三方应用利用 `Token` 管理资源。下面第二部分给出了详细流程图。

**基于以上流程特点，在 MASA Stack 中，我们采用授权码模式**。

#### 授权流程

以下展示了四种模式流程图

* 授权码模式

  ![授权码模式](https://masa-docs.oss-cn-hangzhou.aliyuncs.com/stack/auth/oauth2/oauth2_authcode_process.png)

* 隐式授权模式

  ![隐式授权模式](https://masa-docs.oss-cn-hangzhou.aliyuncs.com/stack/auth/oauth2/oauth2_hideauth_process.png)

* 用户名密码模式

  ![用户名密码模式](https://masa-docs.oss-cn-hangzhou.aliyuncs.com/stack/auth/oauth2/oauth2_password_process.png)

* 客户端模式

  ![客户端模式](https://masa-docs.oss-cn-hangzhou.aliyuncs.com/stack/auth/oauth2/oauth2_client_process.png)


#### 在 MASA Stack 中应用 OAuth 2

在 `MASA Stack` 中我们使用授权码模式来进行流程规范，`MASA Stack` 中 `MASA.Auth` 项目是统一授权中心（授权服务器），`MASA PM`、`MASA DCC`、`MASA TSC`、`MASA MC` 等应用我们可以当成业务系统（资源服务器）。参照授权码模式流程，即用户访问第三方应用，第三方应用跳转到 `MASA Auth`，`MASA Auth` 返回第三方应用需要的权限信息，用户确认授权给第三方应用，`MASA Auth` 重定向到第三方应用并携带 `Code` 参数，第三方应用根据 `Code` 向 `MASA.Auth` 申请 `Token`，`MASA Auth` 发放 `Token` 给第三方应用，第三方应用根据 `Token` 请求资源服务器相应资源并返回给用户。

以下展示了 `MASA Stack` 与第三方之间交互方式，具体流程可以参照授权码模式流程图。

![MASA Auth 交互图](https://masa-docs.oss-cn-hangzhou.aliyuncs.com/stack/auth/oauth2/masa_auth_oauth2.png)

### OIDC

#### 基础概念

`OpenID Connect 1.0` 是 `OAuth 2.0` 协议之上的一个简单的身份层。 它允许客户端基于授权服务器执行的身份验证来验证终端用户的身份，并以一种可互操作的、类似 `REST` 的方式获取关于终端用户的基本概要信息。

![OIDC 结构图](https://masa-docs.oss-cn-hangzhou.aliyuncs.com/stack/auth/oauth2/oidc_struct.png)

#### OIDC 常用术语

* EU：End User：终端用户

* RP：Relying Party，用来代指 `OAuth2` 中的受信任的客户端，身份认证和授权信息的消费方

* OP：OpenID Provider，有能力提供EU认证的服务（比如 `OAuth2` 中的授权服务），用来为 `RP` 提供 `EU` 的身份认证信息

* ID Token：`JWT` 格式的数据，包含 `EU` 身份认证的信息

* UserInfo Endpoint：用户信息接口（受 `OAuth2` 保护），当 `RP` 使用 `Access Token` 访问时，返回授权用户的信息，此接口必须使用 `HTTPS`

#### OIDC 工作流

1. `RP` 发送一个认证请求给 `OP`

2. `OP` 对 `EU` 进行身份认证，然后提供授权

3. `OP` 把 `ID Token` 和 `Access Token` （需要的话）返回给 `RP`

4. `RP` 使用 `Access Token` 发送一个请求 `UserInfo EndPoint`

5. `UserInfo EndPoint` 返回 `EU` 的 `Claims`

![OIDC 工作流](https://masa-docs.oss-cn-hangzhou.aliyuncs.com/stack/auth/oauth2/oidc_workflow.jpg)

### JWT
#### 基础概念

`JWT`（`JSON Web token`）是一个开放的、行业标准的 [RFC 7519](https://www.rfc-editor.org/rfc/rfc7519) 方法，用于在双方之间安全地表示声明。`JWT` 由 `3` 部分组成：标头(`Header`)、有效载荷(`Payload`)和签名(`Signature`)。在传输的时候，会将 `JWT` 的 `3` 部分分别进行 `Base64` 编码后用.进行连接形成最终传输的字符串

``` csharp
JWT = Base64(Header).Base64(Payload).HMACSHA256(base64UrlEncode(header) + "." + base64UrlEncode(payload), secret);
```	

#### Header

`JWT` 头是一个描述 `JWT` 元数据的 `JSON` 对象，`alg` 属性表示签名使用的算法，默认为 `HMAC SHA256`（写为 `HS256`）；`typ` 属性表示令牌的类型，`JWT` 令牌统一写为 `JWT`。最后，使用 `Base64 URL` 算法将上述 `JSON` 对象转换为字符串保存

``` json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

#### Payload

有效载荷部分，是 `JWT` 的主体内容部分，也是一个 `JSON` 对象，包含需要传递的数据（允许自定义）。

``` json
{
  "sub": "1234567890",
  "name": "John Doe",
  "iat": 1516239022
}
```

#### Signature

签名哈希部分是对上面两部分数据签名，需要使用 `base64` 编码后的 `header` 和 `payload` 数据，通过指定的算法生成哈希，以确保数据不会被篡改

``` csharp
HMACSHA256(
  base64UrlEncode(header) + "." +
  base64UrlEncode(payload),
  your-256-bit-secret
)
```

### Identity Server 4 常用术语

#### Client

一个从 `IdentityServer` 请求令牌的软件——用于验证用户（请求身份令牌）或访问资源（请求访问令牌）。客户端必须先向 `IdentityServer` 注册，然后才能请求令牌  
* Allowed Scopes：即可以是 `Identity Resource`，也可以是 `Api Scopes` 和 `Api Resources`

#### Resource
希望使用 `IdentityServer` 保护的东西，如用户的身份数据或 `API` 。资源名称唯一

* API Scope：`API` 作用域

> 可以当做是 `Permission` 来用，示例见：[https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes/](https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes/)

* Identity Resource：关于用户的身份信息（又名声明），例如姓名或电子邮件地址

  * User Claims：身份声明，例如 `sub`，`name`，`amr`，`auth_time` 等

  * Identity Properties：身份资源本身的一些属性，例如 `session_id`，`issued`，`expired` 等

  * Identity Grants：被授予的身份信息

* API Resource：一组 `API Scope`
  
  * User Claims：需要包含在 `Access Token` 中的用户声明列表
  
  * API Resource Scope：`API` 资源包含的作用域
  
  * API Properties：`API` 本身的一些属性，例如 `name`, `display name`, `description` 等
  
  * API Grants：被授权的 `API` 列表

* Identity Token：身份令牌代表身份验证过程的结果。它至少包含用户的标识符以及有关用户如何以及何时进行身份验证的信息。它可以包含额外的身份数据

* Access Token：访问令牌允许访问 `API` 资源。客户端请求访问令牌并将其转发到 `API` 。访问令牌包含有关客户端和用户（如果存在）的信息。 `API` 使用该信息来授权访问其数据

* Grant Types：授权类型（其实还有 `Resource owner password`，不推荐使用，就不过多介绍了）

  > 参考自：[https://docs.duendesoftware.com/identityserver/v6/overview/terminology/](https://docs.duendesoftware.com/identityserver/v6/overview/terminology/)
  
  * Machine/Robot：Client Credentials
  
  * Web Applications：Authorization Code With PKCE（Proof Key for Code Exchange）
  
    > 通常我们会选择` id_token`, `token` 作为 `response type`
    
    > 还有一个选择，就是 `Implicit`。但在隐式流程中，所有令牌都通过浏览器传输，因此不允许刷新令牌等高级功能。作用范围就是仅用于用户身份验证（服务器端和 `JavaScript` 应用程序），或身份验证和访问令牌请求（`JavaScript` 应用程序）
    
  * SPA：Authorization Code With PKCE
  
  * Native/Mobile Apps：Authorization Code With PKCE
  
  * TV/Limited Input Device：Device Flow [FC 8628](https://tools.ietf.org/html/rfc8628)
  
