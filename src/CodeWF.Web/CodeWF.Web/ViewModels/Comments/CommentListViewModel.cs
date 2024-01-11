namespace CodeWF.Web.ViewModels.Comments;

public record CommentListViewModel(string Url, Guid? ParentId, List<CommentDto>? Comments);