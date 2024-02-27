namespace CodeWF.Theme;

public record CreateThemeCommand(string Name, IDictionary<string, string> Rules) : IRequest<int>;

public class CreateThemeCommandHandler(IRepository<BlogThemeEntity> repo) : IRequestHandler<CreateThemeCommand, int>
{
    public async Task<int> Handle(CreateThemeCommand request, CancellationToken ct)
    {
        (string name, IDictionary<string, string> dictionary) = request;
        if (await repo.AnyAsync(p => p.ThemeName == name.Trim(), ct))
        {
            return 0;
        }

        string rules = JsonSerializer.Serialize(dictionary);
        BlogThemeEntity entity =
            new BlogThemeEntity { ThemeName = name.Trim(), CssRules = rules, ThemeType = ThemeType.User };

        await repo.AddAsync(entity, ct);
        return entity.Id;
    }
}