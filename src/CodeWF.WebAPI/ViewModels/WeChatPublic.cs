namespace CodeWF.WebAPI.ViewModels
{
    /// <summary>
    /// 公众号信息
    /// </summary>
    public class WeChatPublic
    {
        /// <summary>
        /// 公众号名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 公众号二维码
        /// </summary>
        public string? QRCode { get; set; }
    }
}
