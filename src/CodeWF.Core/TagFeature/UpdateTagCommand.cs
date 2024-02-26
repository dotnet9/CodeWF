namespace CodeWF.Core.TagFeature;

public record UpdateTagCommand(int Id, string Name) : IRequest<OperationCode>;

public class UpdateTagCommandHandler(IRepository<TagEntity> repo) : IRequestHandler<UpdateTagCommand, OperationCode>
{
    public async Task<OperationCode> Handle(UpdateTagCommand request, CancellationToken ct)
    {
        (int id, string name) = request;
        TagEntity? tag = await repo.GetAsync(id, ct);
        if (null == tag)
        {
            return OperationCode.ObjectNotFound;
        }

        tag.DisplayName = name;
        tag.NormalizedName = Tag.NormalizeName(name, Helper.TagNormalizationDictionary);
        await repo.UpdateAsync(tag, ct);

        return OperationCode.Done;
    }
}