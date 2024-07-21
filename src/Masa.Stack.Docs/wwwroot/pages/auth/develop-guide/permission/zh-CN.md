# 权限

## 菜单权限

交由 Auth 管理，对接的业务系统无需考虑，在 Auth 中添加菜单并设置权限即可。

## 元素权限(Blazor)

页面内增删改等元素权限，需要业务系统自己实现 `IPermissionValidator` 接口并注入，目前仅支持元素的显示和隐藏。

示例代码：

```csharp 
public class PermissionValidator : IPermissionValidator
{
    public bool Validate(string code, ClaimsPrincipal user)
    {
        // code:元素权限Code
        // 权限判断逻辑
        return true;
    }
}

// ioc
builder.Services.AddScoped<IPermissionValidator, PermissionValidator>();
```

使用 `PermissionView` 包裹需要权限控制的元素

```html
<PermissionView Code="@Code">
    <SButton Small Class="rounded-lg" BorderRadiusClass="rounded-lg">
        <span class="ml-2 btn">@T("Add")</span>
    </SButton>
</PermissionView>
```

## API权限

**API权限**可以通过两种方式来控制，MASA Auth 中均有提供示例代码。

* 借助中间件 更适合大多数接口控制权限，少数接口忽略,需要忽略的接口添加 `AllowAnonymousAttribute` 特性。
* `IAuthorizationMiddlewareResultHandler` 适合大多数接口忽略，少数接口控制权限，需要控制的接口添加 `AuthorizeAttribute` 或 `MasaAuthorizeAttribute` 特性。

### 中间件

定义中间件，`InvokeAsync` 方法内处理逻辑。

1. 过滤非自定义路由（不是必须的），如 Dapr 等。
2. 判断是否有 `AllowAnonymousAttribute` 特性，有则直接调用 `next`
3. 判断是否有 `MasaAuthorizeAttribute` 特性，获取接口对应 `Code` 值
4. 没有自定义 `Code`，字默认生成 `Code`，`api/user/create` 生成为 `{AppId}.api.user.create`
5. 获取用户授权得 `Code` 并判断，未授权 `Code` 返回 `StatusCodes.Status403Forbidden`

[完整代码](https://github.com/masastack/MASA.Auth/blob/main/src/Services/Masa.Auth.Service.Admin/Infrastructure/Authorization/MasaAuthorizeMiddleware.cs)

### Authorize扩展

实现接口 `IAuthorizationMiddlewareResultHandler` 并注入。

```csharp 
.AddScoped<IAuthorizationMiddlewareResultHandler, CodeAuthorizationMiddlewareResultHandler>()
.AddSingleton<IAuthorizationHandler, DefaultRuleCodeAuthorizationHandler>()
//.AddSingleton<IAuthorizationPolicyProvider, DefaultRuleCodePolicyProvider>()
.AddAuthorization(options =>
{
    var defaultPolicy = new AuthorizationPolicyBuilder()
        // Remove if you don't need the user to be authenticated
        .RequireAuthenticatedUser()
        .AddRequirements(new DefaultRuleCodeRequirement(AppId))
        .Build();
    options.DefaultPolicy = defaultPolicy;
})
```

`AddAuthorization` 方法内设置默认策略，或者通过 `DefaultRuleCodePolicyProvider` 设置策略，添加策略的 `Requirements`。
`IAuthorizationMiddlewareResultHandler` 的 `HandleAsync` 方法内处理逻辑。

[完整代码](https://github.com/masastack/MASA.Auth/blob/main/src/Services/Masa.Auth.Service.Admin/Infrastructure/Authorization/CodeAuthorizationMiddlewareResultHandler.cs)

## 数据权限

Todo
