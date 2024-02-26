namespace CodeWF.Core.PostFeature;

public record ListArchiveQuery(int Year, int? Month = null) : IRequest<IReadOnlyList<PostDigest>>;

public class ListArchiveQueryHandler(IRepository<PostEntity> repo)
    : IRequestHandler<ListArchiveQuery, IReadOnlyList<PostDigest>>
{
    public Task<IReadOnlyList<PostDigest>> Handle(ListArchiveQuery request, CancellationToken ct)
    {
        PostSpec spec = new PostSpec(request.Year, request.Month.GetValueOrDefault());
        Task<IReadOnlyList<PostDigest>> list = repo.SelectAsync(spec, PostDigest.EntitySelector, ct);
        return list;
    }
}