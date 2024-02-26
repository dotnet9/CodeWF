namespace CodeWF.Core.TagFeature;

public record GetTagQuery(string NormalizedName) : IRequest<Tag>;

public class GetTagQueryHandler(IRepository<TagEntity> repo) : IRequestHandler<GetTagQuery, Tag>
{
    public Task<Tag> Handle(GetTagQuery request, CancellationToken ct)
    {
        return repo.FirstOrDefaultAsync(new TagSpec(request.NormalizedName), Tag.EntitySelector);
    }
}