namespace CodeWF.Core.TagFeature;

public record DeleteTagCommand(int Id) : IRequest<OperationCode>;

public class DeleteTagCommandHandler(IRepository<TagEntity> tagRepo, IRepository<PostTagEntity> postTagRepo)
    : IRequestHandler<DeleteTagCommand, OperationCode>
{
    public async Task<OperationCode> Handle(DeleteTagCommand request, CancellationToken ct)
    {
        bool exists = await tagRepo.AnyAsync(c => c.Id == request.Id, ct);
        if (!exists)
        {
            return OperationCode.ObjectNotFound;
        }

        // 1. Delete Post-Tag Association
        IReadOnlyList<PostTagEntity> postTags = await postTagRepo.ListAsync(new PostTagSpec(request.Id));
        await postTagRepo.DeleteAsync(postTags, ct);

        // 2. Delte Tag itslef
        await tagRepo.DeleteAsync(request.Id, ct);

        return OperationCode.Done;
    }
}