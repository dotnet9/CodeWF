using CodeWF.Data.Exporting.Exporters;
using MediatR;

namespace CodeWF.Data.Exporting;

public record ExportPageDataCommand : IRequest<ExportResult>;

public class ExportPageDataCommandHandler(IRepository<PageEntity> repo)
    : IRequestHandler<ExportPageDataCommand, ExportResult>
{
    public Task<ExportResult> Handle(ExportPageDataCommand request, CancellationToken ct)
    {
        ZippedJsonExporter<PageEntity> pgExp =
            new ZippedJsonExporter<PageEntity>(repo, "codewf-pages", ExportManager.DataDir);
        return pgExp.ExportData(p => new
        {
            p.Id,
            p.Title,
            p.Slug,
            p.MetaDescription,
            p.HtmlContent,
            p.CssId,
            p.HideSidebar,
            p.IsPublished,
            p.CreateTimeUtc,
            p.UpdateTimeUtc
        }, ct);
    }
}