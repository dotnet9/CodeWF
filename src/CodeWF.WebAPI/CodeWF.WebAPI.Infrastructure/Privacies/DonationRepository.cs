namespace CodeWF.WebAPI.Infrastructure.Privacies;

public class PrivacyRepository : IPrivacyRepository
{
    private readonly CodeWFDbContext _dbContext;

    public PrivacyRepository(CodeWFDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Privacy?> GetAsync()
    {
        return await _dbContext.Privacies!.FirstOrDefaultAsync();
    }
}