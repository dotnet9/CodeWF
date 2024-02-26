namespace CodeWF.Data.Spec;

public sealed class PageSpec : BaseSpecification<PageEntity>
{
    public PageSpec(int top) : base(p => p.IsPublished)
    {
        ApplyOrderByDescending(p => p.CreateTimeUtc);
        ApplyPaging(0, top);
    }
}