namespace CodeWF.Pingback;

public record ClearPingbackCommand : IRequest;

public class ClearPingbackCommandHandler(IRepository<PingbackEntity> repo) : IRequestHandler<ClearPingbackCommand>
{
    public Task Handle(ClearPingbackCommand request, CancellationToken ct)
    {
        return repo.Clear(ct);
    }
}