namespace CodeWF.FriendLink;

public record DeleteLinkCommand(Guid Id) : IRequest;

public class DeleteLinkCommandHandler(IRepository<FriendLinkEntity> repo) : IRequestHandler<DeleteLinkCommand>
{
    public Task Handle(DeleteLinkCommand request, CancellationToken ct) => repo.DeleteAsync(request.Id, ct);
}