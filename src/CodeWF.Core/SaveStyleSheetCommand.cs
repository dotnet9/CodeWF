using System.Security.Cryptography;

namespace CodeWF.Core;

public record SaveStyleSheetCommand(Guid Id, string Slug, string CssContent) : IRequest<Guid>;

public class SaveStyleSheetCommandHandler(IRepository<StyleSheetEntity> repo)
    : IRequestHandler<SaveStyleSheetCommand, Guid>
{
    public async Task<Guid> Handle(SaveStyleSheetCommand request, CancellationToken cancellationToken)
    {
        string slug = request.Slug.ToLower().Trim();
        string css = request.CssContent.Trim();
        string hash = CalculateHash($"{slug}_{css}");

        StyleSheetEntity? entity = await repo.GetAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            entity = new StyleSheetEntity
            {
                Id = request.Id,
                FriendlyName = $"page_{slug}",
                CssContent = css,
                Hash = hash,
                LastModifiedTimeUtc = DateTime.UtcNow
            };

            await repo.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.FriendlyName = $"page_{slug}";
            entity.CssContent = css;
            entity.Hash = hash;
            entity.LastModifiedTimeUtc = DateTime.UtcNow;

            await repo.UpdateAsync(entity, cancellationToken);
        }

        return entity.Id;
    }

    private string CalculateHash(string content)
    {
        SHA256 sha256 = SHA256.Create();

        byte[] inputBytes = Encoding.ASCII.GetBytes(content);
        byte[] outputBytes = sha256.ComputeHash(inputBytes);

        return Convert.ToBase64String(outputBytes);
    }
}