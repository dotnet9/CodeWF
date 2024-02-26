namespace CodeWF.Data.Spec;

public sealed class PostTagSpec : BaseSpecification<PostTagEntity>
{
    public PostTagSpec(int tagId) : base(pt => pt.TagId == tagId)
    {
    }

    public PostTagSpec(int tagId, int pageSize, int pageIndex)
        : base(pt =>
            pt.TagId == tagId
            && !pt.Post!.IsDeleted
            && pt.Post.IsPublished)
    {
        int startRow = (pageIndex - 1) * pageSize;
        ApplyPaging(startRow, pageSize);
        ApplyOrderByDescending(p => p.Post!.PubDateUtc!);
    }
}