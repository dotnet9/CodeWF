namespace BlogWebSite.Client.Layout.Componets;

public partial class NavTabs
{
    public NavTabs()
    {
        tabs = [new("首页", "/"), new("项目", "/1"), new("博客", "/2"), about];
    }

    readonly TabModel[] tabs;
    readonly TabModel about =
        new(
            "更多",
            "/more/[^/]+",
            [
                new("赞助", "/more/Donation"),
                new("博客园", "https://www.cnblogs.com/Dotnet9-com", true),
                new("B站", "https://space.bilibili.com/470546606", true)
            ]
        );

    class TabModel(string name)
    {
        public string? IconName { get; set; } = null;

        public string Name { get; set; } = name;

        public string Href { get; set; } = '/' + name;

        public string Target { get; set; } = "_self";

        public bool IsMenu { get; set; } = false;

        public TabModel[] SecTabs { get; set; } = [];

        public TabModel(string name, string href, bool isBlank = false)
            : this(name)
        {
            Href = href;
            if (isBlank)
            {
                Target = "_blank";
            }
        }

        public TabModel(string name, string href, TabModel[] subs)
            : this(name, href)
        {
            SecTabs = subs;
            IsMenu = true;
        }
    }
}
