namespace CodeWF.WebAPI.ViewModel.BlogPosts;

public record GetBlogPostsByAlbumResponse(IEnumerable<BlogPostDto>? BlogPosts, long Total);