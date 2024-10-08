namespace CodeWF.Models;

/// <summary>
/// 内容系统用户登录信息类。
/// </summary>
public class LoginInfo
{
    /// <summary>
    /// 取得或设置扫码登录码。
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 取得或设置扫码Token。
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// 取得或设置用户名。
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 取得或设置密码。
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 取得或设置验证码。
    /// </summary>
    public string Captcha { get; set; }

    /// <summary>
    /// 取得或设置是否记住用户名。
    /// </summary>
    public bool Remember { get; set; }

    /// <summary>
    /// 取得或设置是否用户名和密码登录，默认是。
    /// </summary>
    public bool IsPassword { get; set; } = true;
}

/// <summary>
/// 内容系统用户表单信息类。
/// </summary>
public class UserFormInfo
{
    /// <summary>
    /// 取得或设置用户名。
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 取得或设置昵称。
    /// </summary>
    public string NickName { get; set; }

    /// <summary>
    /// 取得或设置性别。
    /// </summary>
    public string Sex { get; set; }

    /// <summary>
    /// 取得或设置职业。
    /// </summary>
    public string Metier { get; set; }
}