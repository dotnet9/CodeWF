namespace CodeWF.Comments;

public record ToggleApprovalCommand(Guid[] CommentIds) : IRequest;

public class ToggleApprovalCommandHandler(IRepository<CommentEntity> repo) : IRequestHandler<ToggleApprovalCommand>
{
    public async Task Handle(ToggleApprovalCommand request, CancellationToken ct)
    {
        CommentSpec spec = new CommentSpec(request.CommentIds);
        IReadOnlyList<CommentEntity> comments = await repo.ListAsync(spec);
        foreach (CommentEntity cmt in comments)
        {
            cmt.IsApproved = !cmt.IsApproved;
            await repo.UpdateAsync(cmt, ct);
        }
    }
}