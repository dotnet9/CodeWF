namespace CodeWF.Core.PostFeature;

public record GetPostByIdQuery(Guid Id) : IRequest<Post>;

public class GetPostByIdQueryHandler(IRepository<PostEntity> repo) : IRequestHandler<GetPostByIdQuery, Post>
{
    public Task<Post> Handle(GetPostByIdQuery request, CancellationToken ct)
    {
        PostSpec spec = new PostSpec(request.Id);
        Task<Post?> post = repo.FirstOrDefaultAsync(spec, Post.EntitySelector);
        return post;
    }
}