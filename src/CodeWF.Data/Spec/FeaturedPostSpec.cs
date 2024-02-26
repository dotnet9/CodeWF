namespace CodeWF.Data.Spec;

public sealed class FeaturedPostSpec : BaseSpecification<PostEntity>
{
    public FeaturedPostSpec() : base(p => p.IsFeatured)
    {
    }

    public FeaturedPostSpec(int pageSize, int pageIndex)
        : base(p =>
            p.IsFeatured
            && !p.IsDeleted
            && p.IsPublished)
    {
        int startRow = (pageIndex - 1) * pageSize;
        ApplyPaging(startRow, pageSize);
        ApplyOrderByDescending(p => p.PubDateUtc);
    }
}