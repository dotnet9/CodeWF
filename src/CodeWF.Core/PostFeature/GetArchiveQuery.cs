namespace CodeWF.Core.PostFeature;

public record struct Archive(int Year, int Month, int Count);

public record GetArchiveQuery : IRequest<IReadOnlyList<Archive>>;

public class GetArchiveQueryHandler(IRepository<PostEntity> repo)
    : IRequestHandler<GetArchiveQuery, IReadOnlyList<Archive>>
{
    private readonly Expression<Func<IGrouping<(int Year, int Month), PostEntity>, Archive>> _archiveSelector =
        p => new Archive(p.Key.Year, p.Key.Month, p.Count());

    public async Task<IReadOnlyList<Archive>> Handle(GetArchiveQuery request, CancellationToken ct)
    {
        if (!await repo.AnyAsync(p => p.IsPublished && !p.IsDeleted, ct))
        {
            return new List<Archive>();
        }

        PostSpec spec = new(PostStatus.Published);
        IReadOnlyList<Archive> list = await repo.SelectAsync(
            post => new ValueTuple<int, int>(post.PubDateUtc.Value.Year, post.PubDateUtc.Value.Month),
            _archiveSelector, spec);

        return list;
    }
}