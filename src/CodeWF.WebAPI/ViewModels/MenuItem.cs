namespace CodeWF.WebAPI.ViewModels
{
    /// <summary>
    /// 菜单项
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 菜单链接
        /// </summary>
        public string? Url { get; set; }
        /// <summary>
        /// 子菜单
        /// </summary>
        public List<MenuItem>? Children { get; set; }
    }
}
