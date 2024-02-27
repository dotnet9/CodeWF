namespace CodeWF.Auth;

public record CountAccountsQuery : IRequest<int>;

public class CountAccountsQueryHandler(IRepository<LocalAccountEntity> repo) : IRequestHandler<CountAccountsQuery, int>
{
    public Task<int> Handle(CountAccountsQuery request, CancellationToken ct)
    {
        return repo.CountAsync(ct: ct);
    }
}