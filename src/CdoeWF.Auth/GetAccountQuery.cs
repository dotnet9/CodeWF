namespace CodeWF.Auth;

public record GetAccountQuery(Guid Id) : IRequest<Account>;

public class GetAccountQueryHandler(IRepository<LocalAccountEntity> repo) : IRequestHandler<GetAccountQuery, Account>
{
    public async Task<Account> Handle(GetAccountQuery request, CancellationToken ct)
    {
        LocalAccountEntity? entity = await repo.GetAsync(request.Id, ct);
        Account item = new Account(entity);
        return item;
    }
}