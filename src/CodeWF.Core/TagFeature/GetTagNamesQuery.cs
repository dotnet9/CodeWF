namespace CodeWF.Core.TagFeature;

public record GetTagNamesQuery : IRequest<IReadOnlyList<string>>;

public class GetTagNamesQueryHandler(IRepository<TagEntity> repo)
    : IRequestHandler<GetTagNamesQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(GetTagNamesQuery request, CancellationToken ct)
    {
        return repo.SelectAsync(t => t.DisplayName, ct);
    }
}