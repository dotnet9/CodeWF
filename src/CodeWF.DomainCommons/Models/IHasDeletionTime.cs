namespace CodeWF.DomainCommons.Models;

public interface IHasDeletionTime
{
    DateTime? DeletionTime { get; }
}