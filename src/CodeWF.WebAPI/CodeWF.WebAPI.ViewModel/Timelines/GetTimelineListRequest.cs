namespace CodeWF.WebAPI.ViewModel.Timelines;

public record GetTimelineListRequest(string? Keywords, int Current, int PageSize);