namespace CodeWF.WebAPI.Infrastructure.Abouts;

public class AboutRepository : IAboutRepository
{
    private readonly CodeWFDbContext _dbContext;

    public AboutRepository(CodeWFDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<About?> GetAsync()
    {
        return await _dbContext.Abouts!.FirstOrDefaultAsync();
    }
}