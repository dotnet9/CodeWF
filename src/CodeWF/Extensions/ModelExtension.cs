namespace CodeWF.Extensions;

/// <summary>
/// 数据模型扩展类。
/// </summary>
public static class ModelExtension
{
    /// <summary>
    /// 将内容类别列表转换成树形结构。
    /// </summary>
    /// <param name="lists">内容类别列表。</param>
    /// <returns>树形结构。</returns>
    public static List<CmCategory> ToTreeData(this List<CmCategory> lists)
    {
        var data = new List<CmCategory>();
        var roots = lists.Where(l => l.ParentId == "0").ToList();
        foreach (var item in roots)
        {
            data.Add(item);
            AddChild(lists, item);
        }
        return data;
    }

    private static void AddChild(List<CmCategory> lists, CmCategory model)
    {
        var items = lists.Where(l => l.ParentId == model.Id).ToList();
        if (items == null || items.Count == 0)
            return;

        foreach (var item in items)
        {
            model.AddChild(item);
            AddChild(lists, item);
        }
    }

    internal static void CalculateRank(this CmPost post)
    {
        post.RankNo = (post.ViewQty ?? 0) * 1 + (post.ReplyQty ?? 0) * 2;
    }
}