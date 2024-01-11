namespace CodeWF.DomainCommons.Models;

public interface IEntity
{
    public Guid Id { get; }
    object[] GetKeys();
}