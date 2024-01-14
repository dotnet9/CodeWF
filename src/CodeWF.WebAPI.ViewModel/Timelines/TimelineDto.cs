namespace CodeWF.WebAPI.ViewModel.Timelines;

public record TimelineDto(Guid Id, DateTime Time, string Title, string Content);