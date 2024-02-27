namespace CodeWF.FriendLink;

public record GetAllLinksQuery : IRequest<IReadOnlyList<FriendLinkEntity>>;

public class GetAllLinksQueryHandler(IRepository<FriendLinkEntity> repo)
    : IRequestHandler<GetAllLinksQuery, IReadOnlyList<FriendLinkEntity>>
{
    public Task<IReadOnlyList<FriendLinkEntity>> Handle(GetAllLinksQuery request, CancellationToken ct)
    {
        return repo.ListAsync(ct);
    }
}