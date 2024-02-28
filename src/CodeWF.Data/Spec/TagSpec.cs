namespace CodeWF.Data.Spec;

public sealed class TagSpec : BaseSpecification<TagEntity>
{
    public TagSpec(int top) : base(t => true)
    {
        ApplyPaging(0, top);
        ApplyOrderByDescending(p => p.Posts.Count);
    }

    public TagSpec(string normalizedName) : base(t => t.NormalizedName.ToLower() == normalizedName.ToLower())
    {
    }
}