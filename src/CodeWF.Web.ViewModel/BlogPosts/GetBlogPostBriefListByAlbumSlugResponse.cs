namespace CodeWF.Web.ViewModel.BlogPosts;

public record GetBlogPostBriefListByAlbumSlugResponse(string? AlbumName, List<BlogPostBriefForFront>? Data,
    int Total,
    bool Success,
    int PageSize,
    int Current);