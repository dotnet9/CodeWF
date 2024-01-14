namespace CodeWF.WebAPI.ViewModel.BlogPosts;

public record GetBlogPostListRequest(string? Keywords, int Current, int PageSize);