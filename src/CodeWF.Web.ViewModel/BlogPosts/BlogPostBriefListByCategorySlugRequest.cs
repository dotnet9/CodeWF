namespace CodeWF.Web.ViewModel.BlogPosts;

public record BlogPostBriefListByCategorySlugRequest(string Slug, int Current, int PageSize);