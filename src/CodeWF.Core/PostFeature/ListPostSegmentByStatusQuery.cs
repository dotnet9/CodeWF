namespace CodeWF.Core.PostFeature;

public record ListPostSegmentByStatusQuery(PostStatus Status) : IRequest<IReadOnlyList<PostSegment>>;

public class ListPostSegmentByStatusQueryHandler(IRepository<PostEntity> repo)
    : IRequestHandler<ListPostSegmentByStatusQuery, IReadOnlyList<PostSegment>>
{
    public Task<IReadOnlyList<PostSegment>> Handle(ListPostSegmentByStatusQuery request, CancellationToken ct)
    {
        PostSpec spec = new PostSpec(request.Status);
        return repo.SelectAsync(spec, PostSegment.EntitySelector, ct);
    }
}