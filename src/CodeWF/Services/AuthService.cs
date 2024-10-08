using System.Collections.Concurrent;

namespace CodeWF.Services;

/// <summary>
/// 内容系统身份认证服务类。
/// </summary>
public class AuthService
{
    // 缓存登录用户
    private static readonly ConcurrentDictionary<string, UserInfo> Users = new();
    // 缓存微信登录状态
    private static readonly ConcurrentDictionary<string, Result> Tokens = new();

    /// <summary>
    /// 获取微信登录二维码Token。
    /// </summary>
    /// <returns>二维码Token。</returns>
    public static string GetWeixinQRCodeToken()
    {
        var token = Utils.GetGuid();
        Tokens[token] = Result.Error("等待扫码登录！");
        return token;
    }

    /// <summary>
    /// 检查微信扫码结果。
    /// </summary>
    /// <param name="token">二维码Token。</param>
    /// <returns>微信扫码结果。</returns>
    public static Result CheckWeixinQRCodeToken(string token)
    {
        if (!Tokens.ContainsKey(token))
            return Result.Error("二维码票据不存在！");

        return Tokens[token];
    }

    /// <summary>
    /// 异步获取用户信息。
    /// </summary>
    /// <param name="userName">用户登录名。</param>
    /// <returns>用户信息。</returns>
    public static async Task<UserInfo> GetUserAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return null;

        Users.TryGetValue(userName, out UserInfo info);
        if (info == null)
        {
            var database = Database.Create();
            var user = await database.QueryAsync<CmUser>(d => d.UserName == userName);
            if (user != null)
            {
                info = GetUserInfo(user);
                Users[info.UserName] = info;
            }
        }
        return info;
    }

    /// <summary>
    /// 异步用户登录。
    /// </summary>
    /// <param name="info">登录信息。</param>
    /// <returns>登录结果。</returns>
    public static async Task<Result> LoginAsync(LoginInfo info)
    {
        if (!info.IsPassword) //微信扫码注册或登录
        {
            var result = await WeixinLoginAsync(info.Code);
            Tokens[info.Token] = result;
            return result;
        }

        var database = Database.Create();
        database.User = await Platform.GetUserAsync(database, "admin");
        var user = await database.QueryAsync<CmUser>(d => d.UserName == info.UserName);
        if (user == null)
            return Result.Error("用户名不存在！");

        var password = Utils.ToMd5(info.Password);
        if (user.Password != password)
            return Result.Error("密码不正确！");

        var data = GetUserInfo(user);
        Users[info.UserName] = data;
        return Result.Success("登录成功！", data);
    }

    private static async Task<Result> WeixinLoginAsync(string code)
    {
        using var http = new HttpClient();
        var authToken = await http.GetAuthorizeTokenAsync(code);
        if (authToken == null || string.IsNullOrWhiteSpace(authToken.AccessToken))
            return Result.Error($"没有获取到微信AccessToken[{code}]！");

        var database = Database.Create();
        database.User = await Platform.GetUserAsync(database, "admin");
        var weixin = await database.QueryAsync<SysWeixin>(d => d.OpenId == authToken.OpenId);
        if (weixin == null)
        {
            weixin = await http.GetUserInfoAsync(authToken.AccessToken, authToken.OpenId);
            if (weixin == null)
                return Result.Error($"没有获取到微信用户[{authToken.OpenId}]！");
        }

        var user = await database.QueryAsync<CmUser>(d => d.OpenId == weixin.OpenId);
        if (user == null)
        {
            user = new CmUser
            {
                UserName = weixin.OpenId,
                Password = Utils.ToMd5(weixin.OpenId),
                OpenId = weixin.OpenId,
                UnionId = weixin.UnionId,
                NickName = weixin.NickName,
                Status = "启用"
            };
        }
        user.Sex = weixin.Sex;
        user.Country = weixin.Country;
        user.Province = weixin.Province;
        user.City = weixin.City;
        user.AvatarUrl = weixin.HeadImgUrl;

        var result = await database.TransactionAsync("登录", async db =>
        {
            await db.SaveAsync(weixin);
            await db.SaveAsync(user);
        });
        result.Data = GetUserInfo(user);
        return result;
    }

    private static UserInfo GetUserInfo(CmUser user)
    {
        var info = new UserInfo
        {
            Id = user.Id,
            AppId = user.AppId,
            CompNo = user.CompNo,
            UserName = user.UserName,
            Name = user.NickName,
            Gender = user.Sex,
            AvatarUrl = user.AvatarUrl,
            Role = user.Metier,
            Token = Utils.GetGuid()
        };
        return info;
    }
}