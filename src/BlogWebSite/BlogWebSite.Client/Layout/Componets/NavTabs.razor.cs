namespace BlogWebSite.Client.Layout.Componets;

public partial class NavTabs
{
    TabModel[] tabs =
    [
        new("首页", "/"),
        new("项目", "/1"),
        new("博客", "/2"),
        new("关于", "/3"),
    ];

    class TabModel(string name, string? href = null)
    {
        public string Name { get; set; } = name;

        public string Href { get; set; } = href ?? '/' + name;
    }
}
