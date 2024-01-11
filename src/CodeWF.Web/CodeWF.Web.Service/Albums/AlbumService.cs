namespace CodeWF.Web.Service.Albums;

internal class AlbumService : IAlbumService
{
    private readonly CodeWFDbContext _dbContext;

    public AlbumService(CodeWFDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AlbumBrief>> GetAlbumsAsync()
    {
        List<AlbumBrief> albums = await _dbContext.Albums!.Select(c => new AlbumBrief(c.SequenceNumber, c.Slug, c.Name,
                c.Description,
                _dbContext.Set<BlogPostAlbum>().Count(d => d.AlbumId == c.Id)))
            .ToListAsync();
        IOrderedEnumerable<AlbumBrief> distinctCategories = from cat in albums
            where cat.BlogCount > 0
            orderby cat.SequenceNumber ascending
            select cat;
        return distinctCategories.ToList();
    }
}