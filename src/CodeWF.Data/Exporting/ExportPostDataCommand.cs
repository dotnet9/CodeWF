using CodeWF.Data.Exporting.Exporters;
using MediatR;

namespace CodeWF.Data.Exporting;

public record ExportPostDataCommand : IRequest<ExportResult>;

public class ExportPostDataCommandHandler(IRepository<PostEntity> repo)
    : IRequestHandler<ExportPostDataCommand, ExportResult>
{
    public Task<ExportResult> Handle(ExportPostDataCommand request, CancellationToken ct)
    {
        ZippedJsonExporter<PostEntity> poExp =
            new ZippedJsonExporter<PostEntity>(repo, "codewf-posts", ExportManager.DataDir);
        Task<ExportResult> poExportData = poExp.ExportData(p => new
        {
            p.Title,
            p.Slug,
            p.ContentAbstract,
            p.PostContent,
            p.CreateTimeUtc,
            p.CommentEnabled,
            p.PubDateUtc,
            p.ContentLanguageCode,
            p.IsDeleted,
            p.IsFeedIncluded,
            p.IsPublished,
            Categories = p.PostCategory.Select(pc => pc.Category!.DisplayName),
            Tags = p.Tags.Select(pt => pt.DisplayName)
        }, ct);

        return poExportData;
    }
}