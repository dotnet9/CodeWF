namespace CodeWF.Theme;

public record GetAllThemeSegmentQuery : IRequest<IReadOnlyList<ThemeSegment>>;

public class GetAllThemeSegmentQueryHandler(IRepository<BlogThemeEntity> repo)
    : IRequestHandler<GetAllThemeSegmentQuery, IReadOnlyList<ThemeSegment>>
{
    public Task<IReadOnlyList<ThemeSegment>> Handle(GetAllThemeSegmentQuery request, CancellationToken ct)
    {
        return repo.SelectAsync(p => new ThemeSegment { Id = p.Id, Name = p.ThemeName }, ct);
    }
}