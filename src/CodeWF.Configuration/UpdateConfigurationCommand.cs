namespace CodeWF.Configuration;

public record UpdateConfigurationCommand(string Name, string Json) : IRequest<OperationCode>;

public class UpdateConfigurationCommandHandler(IRepository<BlogConfigurationEntity> repository)
    : IRequestHandler<UpdateConfigurationCommand, OperationCode>
{
    public async Task<OperationCode> Handle(UpdateConfigurationCommand request, CancellationToken ct)
    {
        (string name, string json) = request;
        BlogConfigurationEntity? entity = await repository.GetAsync(p => p.CfgKey == name);
        if (entity == null)
        {
            return OperationCode.ObjectNotFound;
        }

        entity.CfgValue = json;
        entity.LastModifiedTimeUtc = DateTime.UtcNow;

        await repository.UpdateAsync(entity, ct);
        return OperationCode.Done;
    }
}