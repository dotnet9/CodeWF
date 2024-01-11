namespace CodeWF.Web.ViewModel.BlogPosts;

public record GetBlogPostBriefListByAlbumSlugRequest(string Slug, int Current, int PageSize);