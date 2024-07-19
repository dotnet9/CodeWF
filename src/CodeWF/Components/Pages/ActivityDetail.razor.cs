using Masa.Blazor;
using CodeWF.Models;
using Microsoft.AspNetCore.Components;
using static CodeWF.Data.ActivityData;

namespace CodeWF.Components.Pages;

public partial class ActivityDetail : IDisposable
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private MasaBlazor MasaBlazor { get; set; } = null!;

    [Parameter]
    public string Id { get; set; } = null!;

    private static readonly List<MenuableTitleItem> sMenuableTitleItems =
    [
        new MenuableTitleItem("视频回放", "", "#activity-video"),
        new MenuableTitleItem("活动详情", "", "#activity-detail"),
        new MenuableTitleItem("资料下载", "", "#active-file")
    ];

    private Models.ActivityDetail? _data;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        MasaBlazor.MobileChanged += MasaBlazorOnMobileChanged;
    }

    private void MasaBlazorOnMobileChanged(object? sender, MobileChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _data = AllActivityDetails.GetValueOrDefault(Id);
    }

    public void Dispose()
    {
        MasaBlazor.MobileChanged -= MasaBlazorOnMobileChanged;
    }
}
