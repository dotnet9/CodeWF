namespace CodeWF.Configuration;

public record AddDefaultConfigurationCommand(int Id, string CfgKey, string DefaultJson) : IRequest<OperationCode>;

public class AddDefaultConfigurationCommandHandler(IRepository<BlogConfigurationEntity> repository)
    : IRequestHandler<AddDefaultConfigurationCommand, OperationCode>
{
    public async Task<OperationCode> Handle(AddDefaultConfigurationCommand request, CancellationToken ct)
    {
        BlogConfigurationEntity entity = new BlogConfigurationEntity
        {
            Id = request.Id,
            CfgKey = request.CfgKey,
            CfgValue = request.DefaultJson,
            LastModifiedTimeUtc = DateTime.UtcNow
        };

        await repository.AddAsync(entity, ct);
        return OperationCode.Done;
    }
}