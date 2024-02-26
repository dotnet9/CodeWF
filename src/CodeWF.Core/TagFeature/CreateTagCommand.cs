namespace CodeWF.Core.TagFeature;

public record CreateTagCommand(string Name) : IRequest<Tag>;

public class CreateTagCommandHandler(IRepository<TagEntity> repo) : IRequestHandler<CreateTagCommand, Tag>
{
    public async Task<Tag> Handle(CreateTagCommand request, CancellationToken ct)
    {
        if (!Tag.ValidateName(request.Name))
        {
            return null;
        }

        string normalizedName = Tag.NormalizeName(request.Name, Helper.TagNormalizationDictionary);
        if (await repo.AnyAsync(t => t.NormalizedName == normalizedName, ct))
        {
            return await repo.FirstOrDefaultAsync(new TagSpec(normalizedName), Tag.EntitySelector);
        }

        TagEntity newTag = new TagEntity { DisplayName = request.Name, NormalizedName = normalizedName };

        TagEntity tag = await repo.AddAsync(newTag, ct);

        return new Tag { DisplayName = tag.DisplayName, NormalizedName = tag.NormalizedName };
    }
}