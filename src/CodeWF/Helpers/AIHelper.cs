namespace CodeWF.Helpers;

/// <summary>
/// AI自动识别内容帮助者类。
/// </summary>
public class AIHelper
{
    /// <summary>
    /// 取得或设置AI标签数据列表。
    /// </summary>
    public static Dictionary<string, string[]> Tags { get; set; }

    /// <summary>
    /// 根据内容自动识别类别。
    /// </summary>
    /// <param name="categories">类别列表。</param>
    /// <param name="info">内容表单信息。</param>
    /// <returns>类别。</returns>
    public static string GetPostCategory(List<CmCategory> categories, PostFormInfo info)
    {
        return string.Empty;
    }

    /// <summary>
    /// 根据内容自动识别标签。
    /// </summary>
    /// <param name="info">内容表单信息。</param>
    /// <returns>标签。</returns>
    public static string GetPostTags(PostFormInfo info)
    {
        var tags = new List<string>();
        foreach (var tag in Tags)
        {
            foreach (var key in tag.Value)
            {
                if (info.Title.Contains(key) || info.Content.Contains(key))
                {
                    if (!tags.Contains(tag.Key))
                        tags.Add(tag.Key);
                    continue;
                }
            }
        }
        if (tags.Count == 0)
            tags.Add("其他");
        return string.Join(",", tags);
    }
}