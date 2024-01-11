namespace CodeWF.WebAPI.ViewModel.Timelines;

public record GetTimelineListResponse(IEnumerable<TimelineDto>? Data, long Total, bool Success, int PageSize, int Current);