namespace CodeWF.Pingback;

public record DeletePingbackCommand(Guid Id) : IRequest;

public class DeletePingbackCommandHandler(IRepository<PingbackEntity> repo) : IRequestHandler<DeletePingbackCommand>
{
    public Task Handle(DeletePingbackCommand request, CancellationToken ct)
    {
        return repo.DeleteAsync(request.Id, ct);
    }
}