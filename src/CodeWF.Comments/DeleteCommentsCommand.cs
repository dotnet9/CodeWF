namespace CodeWF.Comments;

public record DeleteCommentsCommand(Guid[] Ids) : IRequest;

public class DeleteCommentsCommandHandler(
    IRepository<CommentEntity> commentRepo,
    IRepository<CommentReplyEntity> commentReplyRepo) : IRequestHandler<DeleteCommentsCommand>
{
    public async Task Handle(DeleteCommentsCommand request, CancellationToken ct)
    {
        CommentSpec spec = new CommentSpec(request.Ids);
        IReadOnlyList<CommentEntity> comments = await commentRepo.ListAsync(spec);
        foreach (CommentEntity cmt in comments)
        {
            // 1. Delete all replies
            IReadOnlyList<CommentReplyEntity> cReplies = await commentReplyRepo.ListAsync(new CommentReplySpec(cmt.Id));
            if (cReplies.Any())
            {
                await commentReplyRepo.DeleteAsync(cReplies, ct);
            }

            // 2. Delete comment itself
            await commentRepo.DeleteAsync(cmt, ct);
        }
    }
}