namespace CodeWF.Web.ViewModels.BlogPosts;

public class BlogPostDetailsViewModel
{
    public BlogPostBriefForFront? Preview { get; set; }
    public BlogPostBriefForFront? Next { get; set; }
    public BlogPostDetails? Current { get; set; }
}
