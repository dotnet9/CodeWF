namespace CodeWF.Core.PageFeature;

public record GetPageBySlugQuery(string Slug) : IRequest<BlogPage>;

public class GetPageBySlugQueryHandler(IRepository<PageEntity> repo) : IRequestHandler<GetPageBySlugQuery, BlogPage>
{
    public async Task<BlogPage> Handle(GetPageBySlugQuery request, CancellationToken ct)
    {
        string lower = request.Slug.ToLower();
        PageEntity? entity = await repo.GetAsync(p => p.Slug == lower);
        if (entity == null)
        {
            return null;
        }

        BlogPage item = new BlogPage(entity);
        return item;
    }
}