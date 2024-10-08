using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace WebSite.Controllers;

public class WebController : ControllerBase
{
    [Route("signin")]
    public async Task<Result> LoginAsync([FromBody] LoginInfo info)
    {
        var result = await AuthService.LoginAsync(info);
        if (!result.IsValid)
            return result;

        await LoginAsync(info.UserName);
        return Result.Success("登录成功!");
    }

    [Route("signout")]
    public async Task<Result> LogoutAsync()
    {
        await HttpContext.SignOutAsync(AppWeb.AuthType);
        return Result.Success("退出成功!");
    }

    [Route("/weixin/login")]
    public async Task<string> WeixinLoginAsync([FromQuery] string code, [FromQuery] string token)
    {
        var model = new LoginInfo { Code = code, Token = token, IsPassword = false };
        var result = await AuthService.LoginAsync(model);
        if (!result.IsValid)
            return result.Message;

        return "OK";
    }

    [Route("/weixin/check")]
    public async Task<Result> WeixinCheckAsync([FromQuery] string token)
    {
        var result = AuthService.CheckWeixinQRCodeToken(token);
        if (!result.IsValid)
            return result;

        var info = result.DataAs<UserInfo>();
        await LoginAsync(info?.UserName);
        return Result.Success("登录成功!");
    }

    private async Task LoginAsync(string userName)
    {
        var claims = new List<Claim>() { new(ClaimTypes.Name, userName) };
        var identity = new ClaimsIdentity(claims, AppWeb.AuthType);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(AppWeb.AuthType, principal);
    }
}