namespace CodeWF.Core.PostFeature;

public record PurgeRecycledCommand : IRequest;

public class PurgeRecycledCommandHandler(ICacheAside cache, IRepository<PostEntity> repo)
    : IRequestHandler<PurgeRecycledCommand>
{
    public async Task Handle(PurgeRecycledCommand request, CancellationToken ct)
    {
        PostSpec spec = new PostSpec(true);
        IReadOnlyList<PostEntity> posts = await repo.ListAsync(spec);
        await repo.DeleteAsync(posts, ct);

        foreach (Guid guid in posts.Select(p => p.Id))
        {
            cache.Remove(BlogCachePartition.Post.ToString(), guid.ToString());
        }
    }
}