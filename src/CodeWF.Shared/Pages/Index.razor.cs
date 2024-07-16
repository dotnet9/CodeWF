using System;
using System.Globalization;
using System.Threading.Tasks;
using CodeWF.Shared.Services;
using AntDesign.Extensions.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace CodeWF.Shared.Pages
{
    public partial class Index : ComponentBase, IDisposable
    {
        [Parameter]
        public string Locale { get; set; }

        private Recommend[] _recommends = { };

        private BlogPost[] _posts = { };

        private MoreProps[] _moreArticles = { };
        [Inject] private DemoService DemoService { get; set; }
        [Inject] private ILocalizationService Language { get; set; }

        [Inject] private IStringLocalizer Localizer { get; set; }

        private bool _rendered;

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override async Task OnInitializedAsync()
        {
            await FetchData();
            await base.OnInitializedAsync();

            Language.LanguageChanged += HandleLanguageChanged;
        }

        private void HandleLanguageChanged(object _, CultureInfo culture)
        {
            _rendered = true;
            _ = FetchData();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
            {
                _rendered = true;
                StateHasChanged();
                return;
            }

            //if (_rendered)
            //{
            //    _rendered = false;
            //    _ = FetchData();
            //}
        }

        private async Task FetchData()
        {
            _recommends = await DemoService.GetRecommend();
            _posts = await DemoService.GetPost();
            //_moreArticles = await DemoService.GetMore();
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Language.LanguageChanged -= HandleLanguageChanged;
        }
    }
}
