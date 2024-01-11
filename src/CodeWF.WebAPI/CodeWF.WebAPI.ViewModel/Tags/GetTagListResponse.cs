namespace CodeWF.WebAPI.ViewModel.Tags;

public record GetTagListResponse(IEnumerable<TagDto>? Tags, long Total);