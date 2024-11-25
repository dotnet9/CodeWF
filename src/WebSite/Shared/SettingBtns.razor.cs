using CodeWF.Options;

using Microsoft.Extensions.Options;

namespace WebSite.Shared
{
    public partial class SettingBtns()
    {
        string? right;
        string? bottom;

        string style = string.Empty;
        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (right != null)
            {
                style += $"{nameof(right)}:{right}";
            }

            if (bottom != null)
            {
                style += $"{nameof(bottom)}:{bottom}";
            }
        }
    }
}
