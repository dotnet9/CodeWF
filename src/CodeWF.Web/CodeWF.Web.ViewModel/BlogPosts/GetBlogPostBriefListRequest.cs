namespace CodeWF.Web.ViewModel.BlogPosts;

public record GetBlogPostBriefListRequest(string? Keywords, int Current, int PageSize);