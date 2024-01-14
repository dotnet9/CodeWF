namespace CodeWF.WebAPI.ViewModel.ActionLogs;

public record GetActionLogListResponse(IEnumerable<ActionLogDto>? Data, long Total, bool Success, int PageSize,
    int Current);