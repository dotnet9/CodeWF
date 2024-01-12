namespace CodeWF.WebAPI.ViewModel.Links;

public record LinkSeedDto(int Sort, string SiteName, string Url,
    string? Remark = null, string? Kind = null);