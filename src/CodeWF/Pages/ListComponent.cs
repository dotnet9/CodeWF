using Known.Blazor;

namespace CodeWF.Pages;

/// <summary>
/// 列表组件基类，提供分页查询。
/// </summary>
public class ListComponent<T> : BaseComponent
{
    /// <summary>
    /// 分页查询条件。
    /// </summary>
    protected PagingCriteria Criteria = new();

    /// <summary>
    /// 分页查询结果。
    /// </summary>
    protected PagingResult<T> Result = new();

    /// <summary>
    /// 取得或设置URL页码参数。
    /// </summary>
    [SupplyParameterFromQuery] public int Page { get; set; }

    /// <summary>
    /// 异步初始化组件。
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        if (Page <= 0)
            Page = 1;
        Criteria.PageIndex = Page;
    }
}