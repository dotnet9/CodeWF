namespace CodeWF.Data.Spec;

public class PostSitePageSpec() : BaseSpecification<PostEntity>(p =>
    p.IsPublished && !p.IsDeleted);