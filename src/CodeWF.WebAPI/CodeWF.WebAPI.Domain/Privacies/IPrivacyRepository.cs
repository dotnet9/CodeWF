namespace CodeWF.WebAPI.Domain.Privacies;

public interface IPrivacyRepository
{
    Task<Privacy?> GetAsync();
}