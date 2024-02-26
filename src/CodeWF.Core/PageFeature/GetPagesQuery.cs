namespace CodeWF.Core.PageFeature;

public record GetPagesQuery(int Top) : IRequest<IReadOnlyList<BlogPage>>;

public class GetPagesQueryHandler(IRepository<PageEntity> repo)
    : IRequestHandler<GetPagesQuery, IReadOnlyList<BlogPage>>
{
    public async Task<IReadOnlyList<BlogPage>> Handle(GetPagesQuery request, CancellationToken ct)
    {
        IReadOnlyList<PageEntity> pages = await repo.ListAsync(new PageSpec(request.Top));
        List<BlogPage> list = pages.Select(p => new BlogPage(p)).ToList();
        return list;
    }
}