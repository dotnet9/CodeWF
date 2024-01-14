namespace CodeWF.DomainCommons.Models;

public interface IHasModificationTime
{
    DateTime? LastModificationTime { get; }
}