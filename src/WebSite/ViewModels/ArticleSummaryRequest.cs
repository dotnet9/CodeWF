namespace WebSite.ViewModels;

public record ArticleSummaryRequest(string Content, int Length = 200);