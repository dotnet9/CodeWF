using Masa.Blazor.Core;
using Microsoft.AspNetCore.Components;

namespace BlogWebSite.Client.Componets.Settings
{
    public partial class ThemeSwitch : MasaComponentBase
    {
        //[Parameter]
        //public string? Class { get; set; }

        //[Parameter]
        //public string? Style { get; set; }

        [Parameter]
        public double Zoom { get; set; }

        [CascadingParameter(Name = "IsDark")]
        public bool CascadingIsDark { get; set; }

        string GetZoom() => $";transform: scale({Zoom});";

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (IsDirtyParameter(nameof(CascadingIsDark)))
            {
                if (Dark != CascadingIsDark)
                {
                    Dark = CascadingIsDark;
                    InvokeStateHasChanged();
                }
            }
        }

        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
        //    await base.OnAfterRenderAsync(firstRender);
        //    if (firstRender && load is false)
        //    {
        //        //var ls = await LocalStorage.GetItemAsync("masablazor@theme");

        //        //dark = ls is "dark";
        //        //dark = CascadingIsDark;

        //        if (Dark != MasaBlazor.Theme.Dark)
        //        {
        //            await ThemeChanged();
        //            await InvokeStateHasChangedAsync();
        //        }
        //    }
        //}

        //bool load;

        //protected override async Task OnInitializedAsync()
        //{
        //    await base.OnInitializedAsync();

        //    try
        //    {
        //        if (Dark != CascadingIsDark)
        //        {
        //            //await ThemeChanged();
        //            Dark = CascadingIsDark;
        //            load = true;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return;
        //    }
        //}

        bool Dark;

        Task ThemeChanged()
        {
            MasaBlazor.SetTheme(Dark);

            return MasaLS.SetItemAsync("masablazor@theme", Dark ? "dark" : "light");
        }
    }
}
