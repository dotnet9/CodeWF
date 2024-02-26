namespace CodeWF.Core.PageFeature;

public record GetPageByIdQuery(Guid Id) : IRequest<BlogPage>;

public class GetPageByIdQueryHandler(IRepository<PageEntity> repo) : IRequestHandler<GetPageByIdQuery, BlogPage>
{
    public async Task<BlogPage> Handle(GetPageByIdQuery request, CancellationToken ct)
    {
        PageEntity? entity = await repo.GetAsync(request.Id, ct);
        if (entity == null)
        {
            return null;
        }

        BlogPage item = new BlogPage(entity);
        return item;
    }
}