using CodeWF.Data;
using CodeWF.Data.Entities;
using CodeWF.Data.Specifications;
using CodeWF.EventBus;
using Microsoft.Extensions.Logging;

namespace CodeWF.Core.Abouts;

public record GetAboutResponse
{
    public string? Title { get; set; }
    public string? Content { get; set; }

    public DateTime? UpdateTime { get; set; }
}

public class GetAboutQuery : Query<GetAboutResponse>
{
    public override GetAboutResponse Result { get; set; }
}

[Event]
public class GetAboutQueryHandler(CodeWFRepository<About> repository, ILogger<GetAboutQueryHandler> logger)
{
    [EventHandler]
    public async Task Handle(GetAboutQuery request)
    {
        var result = await repository.FirstOrDefaultAsync(new AboutSpec());
        request.Result = new GetAboutResponse()
        {
            Title = result?.Title,
            Content = result?.Content,
            UpdateTime = result?.UpdateTime
        };
    }
}