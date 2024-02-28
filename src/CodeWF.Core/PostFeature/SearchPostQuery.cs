namespace CodeWF.Core.PostFeature;

public record SearchPostQuery(string Keyword) : IRequest<IReadOnlyList<PostDigest>>;

public class SearchPostQueryHandler(IRepository<PostEntity> repo)
    : IRequestHandler<SearchPostQuery, IReadOnlyList<PostDigest>>
{
    public async Task<IReadOnlyList<PostDigest>> Handle(SearchPostQuery request, CancellationToken ct)
    {
        if (null == request || string.IsNullOrWhiteSpace(request.Keyword))
        {
            throw new ArgumentNullException(request?.Keyword);
        }

        IQueryable<PostEntity> postList = SearchByKeyword(request.Keyword);
        List<PostDigest> resultList = await postList.Select(PostDigest.EntitySelector).ToListAsync(ct);

        return resultList;
    }

    private IQueryable<PostEntity> SearchByKeyword(string keyword)
    {
        IQueryable<PostEntity> query = repo.AsQueryable()
            .Where(p => !p.IsDeleted && p.IsPublished).AsNoTracking();

        string str = Regex.Replace(keyword, @"\s+", " ");
        string[] rst = str.Split(' ');
        if (rst.Length > 1)
        {
            // keyword: "dot  net rocks"
            // search for post where Title containing "dot && net && rocks"
            IQueryable<PostEntity> result =
                rst.Aggregate(query, (current, s) => current.Where(p => p.Title.Contains(s)));
            return result;
        }
        else
        {
            // keyword: "dotnetrocks"
            string k = rst.First();
            IQueryable<PostEntity> result = query.Where(p => p.Title.Contains(k)
                                                             || p.ContentAbstract!.Contains(k)
                                                             || p.Tags.Select(t => t.DisplayName).Contains(k)
                                                             || p.PostCategory.Select(c => c.Category!.DisplayName)
                                                                 .Contains(k));
            return result;
        }
    }
}