namespace CodeWF.WebAPI.ViewModel.BlogPosts;

public record GetBlogPostListResponse(IEnumerable<BlogPostDto>? Data, long Total, bool Success, int PageSize,
    int Current);