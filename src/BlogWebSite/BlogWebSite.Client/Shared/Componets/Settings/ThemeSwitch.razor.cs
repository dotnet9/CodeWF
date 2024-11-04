using Masa.Blazor.Core;

using Microsoft.AspNetCore.Components;

namespace BlogWebSite.Client.Shared.Componets.Settings
{
    public partial class ThemeSwitch : MasaComponentBase
    {
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

        protected bool Dark { get; set; }

        protected Task ThemeChanged()
        {
            MasaBlazor.SetTheme(Dark);

            return MasaLS.SetItemAsync("masablazor@theme", Dark ? "dark" : "light");
        }
    }
}
