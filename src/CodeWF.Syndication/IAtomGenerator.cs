namespace CodeWF.Syndication;

public interface IAtomGenerator
{
    Task<string> WriteAtomAsync();
}