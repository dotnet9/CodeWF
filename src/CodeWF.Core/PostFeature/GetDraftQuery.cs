namespace CodeWF.Core.PostFeature;

public record GetDraftQuery(Guid Id) : IRequest<Post>;

public class GetDraftQueryHandler(IRepository<PostEntity> repo) : IRequestHandler<GetDraftQuery, Post>
{
    public Task<Post> Handle(GetDraftQuery request, CancellationToken ct)
    {
        PostSpec spec = new PostSpec(request.Id);
        Task<Post?> post = repo.FirstOrDefaultAsync(spec, Post.EntitySelector);
        return post;
    }
}