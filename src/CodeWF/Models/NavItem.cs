namespace CodeWF.Models;

public class NavItem
{
    public string? Subheader { get; set; }

    public string? Image { get; set; }

    public string? Title { get; set; }

    public string? Subtitle { get; set; }

    public string? Href { get; set; }

    public int Size { get; set; }

    public bool IsSubheader => Subheader is not null;

    public bool Disabled { get; set; }

    public string Style
    {
        get
        {
            if (Disabled)
            {
                return "color: #A3AED0;";
            }
            else
            {
                return "color: #485585;";
            }
        }
    }

    public NavItem()
    {
    }

    public NavItem(string subheader)
    {
        Subheader = subheader;
    }

    public NavItem(string title, string image)
    {
        Title = title;
        Image = image;
    }

    public NavItem(string title, string subtitle, string image)
    {
        Title = title;
        Subtitle = subtitle;
        Image = image;
    }

    public NavItem(string title, string subtitle, string image, string href, int size) : this(title, subtitle, image)
    {
        Href = href;
        Size = size;
    }

    public NavItem(string title, string subtitle, string image, string href, int size, bool disabled) : this(title,
        subtitle, image, href, size)
    {
        Disabled = disabled;
    }
}