namespace CodeWF.Docs.Shared.Pages;

public partial class ResourceIntro
{
    [CascadingParameter(Name = "Culture")]
    private string? Culture { get; set; }

    private static string installation =
        """
        - Docker compose
        
          ```bash
          git clone https://github.com/dotnet9/helm.git
          ```
          ```bash
          cd helm/docker
          ```
          ```bash
          docker-compose up
          ```
        - [Helm](/resource/masa-stack-1.0/installation/helm)
        """;
    
    private IEnumerable<(string? Title, string? Href, string Intro)>? _products;

    protected override async Task OnInitializedAsync()
    {
        var navs = await DocService.ReadNavsAsync("resource");

        return;

        void SetHref(ProductIntro product)
        {
            var authNav = navs?.FirstOrDefault(u => u.Title == product.Title);
            product.Href = authNav?.Href ?? authNav?.Children?.FirstOrDefault()?.Href;
        }
    }

    private class ProductIntro
    {
        public ProductIntro(string title, string intro)
        {
            Title = title;
            Intro = intro;
        }

        public string Title { get; init; }

        public string Intro { get; init; }

        public string? Href { get; set; }
    }
}
