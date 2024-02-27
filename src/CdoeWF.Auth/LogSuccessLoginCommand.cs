namespace CodeWF.Auth;

public record LogSuccessLoginCommand(Guid Id, string IpAddress) : IRequest;

public class LogSuccessLoginCommandHandler(IRepository<LocalAccountEntity> repo)
    : IRequestHandler<LogSuccessLoginCommand>
{
    public async Task Handle(LogSuccessLoginCommand request, CancellationToken ct)
    {
        (Guid id, string ipAddress) = request;

        LocalAccountEntity? entity = await repo.GetAsync(id, ct);
        if (entity is not null)
        {
            entity.LastLoginIp = ipAddress.Trim();
            entity.LastLoginTimeUtc = DateTime.UtcNow;
            await repo.UpdateAsync(entity, ct);
        }
    }
}