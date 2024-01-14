namespace CodeWF.WebAPI.ViewModel.BlogPosts;

public enum CopyRightType
{
    [EnumMember(Value = "original")] Original,
    [EnumMember(Value = "reprinted")] Reprinted,
    [EnumMember(Value = "contributes")] Contributes,
}

public static class CopyRightTypeExtensions
{
    public static string GetDescription(this CopyRightType copyRightType)
    {
        return copyRightType switch
        {
            CopyRightType.Original => "原创",
            CopyRightType.Reprinted => "转载",
            _ => "投稿"
        };
    }
}