namespace AntBlazor.Core;

/// <summary>
/// 抽象组件基类。
/// </summary>
public abstract class BaseComponent : ComponentBase
{
    /// <summary>
    /// 取得或设置组件ID。
    /// </summary>
    [Parameter] public string Id { get; set; }

    /// <summary>
    /// 取得或设置组件名称。
    /// </summary>
    [Parameter] public string Name { get; set; }

    /// <summary>
    /// 取得或设置组件是否只读。
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// 取得或设置组件是否可用，默认可用。
    /// </summary>
    [Parameter] public bool Enabled { get; set; } = true;

    /// <summary>
    /// 取得或设置组件是否可见，默认可见。
    /// </summary>
    [Parameter] public bool Visible { get; set; } = true;

    /// <summary>
    /// 取得或设置JS运行时实例。
    /// </summary>
    [Inject] public IJSRuntime JSRuntime { get; set; }

    /// <summary>
    /// 取得或设置注入的导航管理者实例。
    /// </summary>
    [Inject] public NavigationManager Navigation { get; set; }

    internal Dictionary<string, object> InputAttributes { get; } = [];
}