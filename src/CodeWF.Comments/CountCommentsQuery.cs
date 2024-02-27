namespace CodeWF.Comments;

public record CountCommentsQuery : IRequest<int>;

public class CountCommentsQueryHandler(IRepository<CommentEntity> repo) : IRequestHandler<CountCommentsQuery, int>
{
    public Task<int> Handle(CountCommentsQuery request, CancellationToken ct)
    {
        return repo.CountAsync(ct: ct);
    }
}