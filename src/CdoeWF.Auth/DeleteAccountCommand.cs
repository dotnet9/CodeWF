namespace CodeWF.Auth;

public record DeleteAccountCommand(Guid Id) : IRequest;

public class DeleteAccountCommandHandler(IRepository<LocalAccountEntity> repo) : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken ct)
    {
        LocalAccountEntity? account = await repo.GetAsync(request.Id, ct);
        if (account != null)
        {
            await repo.DeleteAsync(request.Id, ct);
        }
    }
}