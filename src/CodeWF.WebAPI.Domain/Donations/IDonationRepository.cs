namespace CodeWF.WebAPI.Domain.Donations;

public interface IDonationRepository
{
    Task<Donation?> GetAsync();
}