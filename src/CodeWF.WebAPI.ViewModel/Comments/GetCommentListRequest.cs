namespace CodeWF.WebAPI.ViewModel.Comments;

public record GetCommentListRequest(string Url, Guid? ParentId, int Current, int PageSize);