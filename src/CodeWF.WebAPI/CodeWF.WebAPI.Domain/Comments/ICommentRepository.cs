namespace CodeWF.WebAPI.Domain.Comments;

public interface ICommentRepository
{
    Task<(Comment[]? Comments, long Count)> GetListAsync(GetCommentListRequest request);
    Task<int> DeleteAsync(Guid[] ids);
    Task<Comment?> FindByIdAsync(Guid id);
}