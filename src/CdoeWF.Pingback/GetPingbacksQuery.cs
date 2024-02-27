namespace CodeWF.Pingback;

public record GetPingbacksQuery : IRequest<IReadOnlyList<PingbackEntity>>;

public class GetPingbacksQueryHandler(IRepository<PingbackEntity> repo)
    : IRequestHandler<GetPingbacksQuery, IReadOnlyList<PingbackEntity>>
{
    public Task<IReadOnlyList<PingbackEntity>> Handle(GetPingbacksQuery request, CancellationToken ct)
    {
        return repo.ListAsync(ct);
    }
}