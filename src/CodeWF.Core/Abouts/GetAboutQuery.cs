using CodeWF.Data;
using CodeWF.Data.Entities;
using CodeWF.Data.Specifications;
using MediatR;

namespace CodeWF.Core.Abouts;

public record GetAboutQuery : IRequest<About?>;

public class GetAboutQueryHandler(CodeWFRepository<About> repository)
    : IRequestHandler<GetAboutQuery, About?>
{
    public Task<About?> Handle(GetAboutQuery request, CancellationToken ct)
    {
        return repository.FirstOrDefaultAsync(new AboutSpec(), ct);
    }
}