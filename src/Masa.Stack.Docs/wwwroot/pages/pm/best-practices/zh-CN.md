# 最佳实践

## 实现应用从远程获取配置信息功能

使用 `MASA.PM` + `MASA.DCC` 实现应用从远程获取配置信息功能。

[MASA.DCC](/stack/dcc/introduce) 是一个分布式配置中心，它的基础数据（环境、集群、项目、应用）是从 `MASA.PM` 中获取的，从而进行应用的配置管理。

实现步骤如下：

1. 在 `PM` 中创建所需的环境、集群、项目和应用信息，可参考 [PM 使用指南](stack/pm/get-started)。
2. `DCC` 通过 `PM` 的 SDK 获取创建好的环境、集群、项目和应用信息，可参考 PM 的 [SDK 示例](stack/pm/sdk-instance)。
3. 在 `DCC` 中对相应的应用进行配置写入，可参考 [DCC 使用指南](stack/dcc/get-started)。
4. 通过 `DCC` 的 SDK 去获取写入好的配置，可参考 `DCC` 的 [SDK 示例](stack/dcc/sdk-instance)。

通过以上步骤，即可完成 PM 和 DCC 的结合使用，最终实现应用从远程获取配置的功能。

## 实现应用的权限控制

使用 `MASA.PM` + `MASA.Auth` 实现应用的权限控制。

[MASA.Auth](stack/auth/introduce) 是一个权限控制中心，其中权限模块的基础数据（应用）是从 `MASA.PM` 中获取的，之后再对应用进行权限的管理。

实现步骤如下：

1. 在 `PM` 中创建所需应用信息，可参考 [PM 使用指南](stack/pm/get-started)。
2. `Auth` 通过 `PM` 的 SDK 获取已创建的应用信息，可参考 `PM` 的 [SDK 示例](stack/pm/sdk-instance)。
3. 在 `Auth` 中对权限进行分配：将权限给角色，再给用户赋予相应的角色，可参考 [Auth 使用指南](stack/auth/use-guide/permission)。
4. 通过 `Auth` 统一认证登录系统后，会将权限信息返回给客户端。
5. 客户端对返回的权限信息进行对比，以达到页面或接口的权限控制。
