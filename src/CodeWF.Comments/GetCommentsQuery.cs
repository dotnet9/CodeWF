namespace CodeWF.Comments;

public record GetCommentsQuery(int PageSize, int PageIndex) : IRequest<IReadOnlyList<CommentDetailedItem>>;

public class GetCommentsQueryHandler(IRepository<CommentEntity> repo)
    : IRequestHandler<GetCommentsQuery, IReadOnlyList<CommentDetailedItem>>
{
    public Task<IReadOnlyList<CommentDetailedItem>> Handle(GetCommentsQuery request, CancellationToken ct)
    {
        CommentSpec spec = new CommentSpec(request.PageSize, request.PageIndex);
        Task<IReadOnlyList<CommentDetailedItem>> comments =
            repo.SelectAsync(spec, CommentDetailedItem.EntitySelector, ct);

        return comments;
    }
}