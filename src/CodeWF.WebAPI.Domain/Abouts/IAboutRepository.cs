namespace CodeWF.WebAPI.Domain.Abouts;

public interface IAboutRepository
{
    Task<About?> GetAsync();
}