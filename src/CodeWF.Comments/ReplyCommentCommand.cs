namespace CodeWF.Comments;

public record ReplyCommentCommand(Guid CommentId, string ReplyContent) : IRequest<CommentReply>;

public class ReplyCommentCommandHandler(
    IRepository<CommentEntity> commentRepo,
    IRepository<CommentReplyEntity> commentReplyRepo) : IRequestHandler<ReplyCommentCommand, CommentReply>
{
    public async Task<CommentReply> Handle(ReplyCommentCommand request, CancellationToken ct)
    {
        CommentEntity? cmt = await commentRepo.GetAsync(request.CommentId, ct);
        if (cmt is null)
        {
            throw new InvalidOperationException($"Comment {request.CommentId} is not found.");
        }

        Guid id = Guid.NewGuid();
        CommentReplyEntity model = new CommentReplyEntity
        {
            Id = id,
            ReplyContent = request.ReplyContent,
            CreateTimeUtc = DateTime.UtcNow,
            CommentId = request.CommentId
        };

        await commentReplyRepo.AddAsync(model, ct);

        CommentReply reply = new CommentReply
        {
            CommentContent = cmt.CommentContent,
            CommentId = request.CommentId,
            Email = cmt.Email,
            Id = model.Id,
            PostId = cmt.PostId,
            PubDateUtc = cmt.Post.PubDateUtc.GetValueOrDefault(),
            ReplyContent = model.ReplyContent,
            ReplyContentHtml =
                ContentProcessor.MarkdownToContent(model.ReplyContent, ContentProcessor.MarkdownConvertType.Html),
            ReplyTimeUtc = model.CreateTimeUtc,
            Slug = cmt.Post.Slug,
            Title = cmt.Post.Title
        };

        return reply;
    }
}