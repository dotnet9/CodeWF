namespace CodeWF.WebAPI.ViewModel.BlogPosts;

public record GetBlogPostsByCategoryResponse(IEnumerable<BlogPostDto>? BlogPosts, long TotalCount);