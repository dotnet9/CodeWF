namespace CodeWF.Theme;

public record DeleteThemeCommand(int Id) : IRequest<OperationCode>;

public class DeleteThemeCommandHandler(IRepository<BlogThemeEntity> repo)
    : IRequestHandler<DeleteThemeCommand, OperationCode>
{
    public async Task<OperationCode> Handle(DeleteThemeCommand request, CancellationToken ct)
    {
        BlogThemeEntity? theme = await repo.GetAsync(request.Id, ct);
        if (null == theme)
        {
            return OperationCode.ObjectNotFound;
        }

        if (theme.ThemeType == ThemeType.System)
        {
            return OperationCode.Canceled;
        }

        await repo.DeleteAsync(request.Id, ct);
        return OperationCode.Done;
    }
}