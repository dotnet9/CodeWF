namespace CodeWF.Theme;

public record GetSiteThemeStyleSheetQuery(int Id) : IRequest<string>;

public class GetStyleSheetQueryHandler(IRepository<BlogThemeEntity> repo)
    : IRequestHandler<GetSiteThemeStyleSheetQuery, string>
{
    public async Task<string> Handle(GetSiteThemeStyleSheetQuery request, CancellationToken ct)
    {
        BlogThemeEntity? theme = await repo.GetAsync(request.Id, ct);
        if (null == theme)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(theme.CssRules))
        {
            throw new InvalidDataException($"Theme id '{request.Id}' is having empty CSS Rules");
        }

        try
        {
            IDictionary<string, string>?
                rules = JsonSerializer.Deserialize<IDictionary<string, string>>(theme.CssRules);

            StringBuilder sb = new StringBuilder();
            sb.Append(":root {");
            if (rules != null)
            {
                foreach ((string? key, string? value) in rules)
                {
                    if (null != key && null != value)
                    {
                        sb.Append($"{key}: {value};");
                    }
                }
            }

            sb.Append('}');

            return sb.ToString();
        }
        catch (JsonException)
        {
            throw new InvalidDataException($"Theme id '{request.Id}' CssRules is not a valid json");
        }
    }
}