namespace CodeWF.WebAPI.Infrastructure.Donations;

public class DonationRepository : IDonationRepository
{
    private readonly CodeWFDbContext _dbContext;

    public DonationRepository(CodeWFDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Donation?> GetAsync()
    {
        return await _dbContext.Donations!.FirstOrDefaultAsync();
    }
}