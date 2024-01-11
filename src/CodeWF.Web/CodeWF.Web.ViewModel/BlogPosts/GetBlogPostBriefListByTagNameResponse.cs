namespace CodeWF.Web.ViewModel.BlogPosts;

public record GetBlogPostBriefListByTagNameResponse(List<BlogPostBriefForFront>? Data,
    int Total,
    bool Success,
    int PageSize,
    int Current);