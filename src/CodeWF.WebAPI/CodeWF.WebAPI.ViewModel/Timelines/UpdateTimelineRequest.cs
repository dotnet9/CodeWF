namespace CodeWF.WebAPI.ViewModel.Timelines;

public record UpdateTimelineRequest(DateTime Time, string Title, string Content);