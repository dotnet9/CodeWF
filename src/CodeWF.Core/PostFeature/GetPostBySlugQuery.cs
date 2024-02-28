namespace CodeWF.Core.PostFeature;

public record GetPostBySlugQuery(PostSlug Slug) : IRequest<Post>;

public class GetPostBySlugQueryHandler(IRepository<PostEntity> repo, ICacheAside cache, IConfiguration configuration)
    : IRequestHandler<GetPostBySlugQuery, Post>
{
    public async Task<Post?> Handle(GetPostBySlugQuery request, CancellationToken ct)
    {
        var date = new DateTime(request.Slug.Year, request.Slug.Month, request.Slug.Day);

        // Try to find by checksum
        int slugCheckSum = Helper.ComputeCheckSum($"{request.Slug.Slug}#{date:yyyyMMdd}");
        ISpecification<PostEntity> spec = new PostSpec(slugCheckSum);

        Guid pid = await repo.FirstOrDefaultAsync(spec, p => p.Id);
        if (pid == Guid.Empty)
        {
            // Post does not have a checksum, fall back to old method
            spec = new PostSpec(date, request.Slug.Slug);
            pid = await repo.FirstOrDefaultAsync(spec, x => x.Id);

            if (pid == Guid.Empty)
            {
                return default;
            }

            // Post is found, fill it's checksum so that next time the query can be run against checksum
            PostEntity? p = await repo.GetAsync(pid, ct);
            p.HashCheckSum = slugCheckSum;

            await repo.UpdateAsync(p, ct);
        }

        Post? psm = await cache.GetOrCreateAsync(BlogCachePartition.Post.ToString(), $"{pid}", async entry =>
        {
            entry.SlidingExpiration =
                TimeSpan.FromMinutes(int.Parse(configuration["CacheSlidingExpirationMinutes:Post"]!));

            Post? post = await repo.FirstOrDefaultAsync(spec, Post.EntitySelector);
            return post;
        });

        return psm;
    }
}