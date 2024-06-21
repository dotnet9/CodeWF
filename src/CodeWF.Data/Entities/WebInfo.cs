namespace CodeWF.Data.Entities;

public class WebInfo
{
    public Guid Id { get; set; }

    public string WebName { get; set; } = null!;

    public string WebTitle { get; set; } = null!;

    public string? Notices { get; set; }

    public string Footer { get; set; } = null!;

    public string? BackgroundImage { get; set; }

    public string Avatar { get; set; } = null!;

    public string? RandomAvatar { get; set; }

    public string? RandomName { get; set; }

    public string? RandomCover { get; set; }

    public string? WaifuJson { get; set; }

    public EnabledKind Status { get; set; }
}