namespace CodeWF.Core.PostFeature;

public record ListFeaturedQuery(int PageSize, int PageIndex) : IRequest<IReadOnlyList<PostDigest>>;

public class ListFeaturedQueryHandler(IRepository<PostEntity> repo)
    : IRequestHandler<ListFeaturedQuery, IReadOnlyList<PostDigest>>
{
    public Task<IReadOnlyList<PostDigest>> Handle(ListFeaturedQuery request, CancellationToken ct)
    {
        (int pageSize, int pageIndex) = request;
        Helper.ValidatePagingParameters(pageSize, pageIndex);

        Task<IReadOnlyList<PostDigest>> posts =
            repo.SelectAsync(new FeaturedPostSpec(pageSize, pageIndex), PostDigest.EntitySelector, ct);
        return posts;
    }
}