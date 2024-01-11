namespace CodeWF.WebAPI.Events;

public record LikeBlogPostEvent(string Slug, int
    LikeCount) : INotification;