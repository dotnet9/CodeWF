namespace CodeWF.Admin.Pages.Content;

[Route("/dms/articles")]
public class DmArticleList : BaseTablePage<CmPost>
{
    private IPostService Service;

    private static string Type => ContentType.Article.ToString();

    protected override async Task OnPageInitAsync()
    {
        await base.OnPageInitAsync();
        Service = await CreateServiceAsync<IPostService>();
        Table.OnQuery = QueryPostsAsync;
    }

    public void New() => Table.NewForm(Service.SavePostAsync, new CmPost { Type = Type, Status = PostStatus.Published });
    public void DeleteM() => Table.DeleteM(Service.DeletePostsAsync);
    public void Edit(CmPost row) => Table.EditForm(Service.SavePostAsync, row);
    public void Delete(CmPost row) => Table.Delete(Service.DeletePostsAsync, row);
    public async void Export() => await Table.ExportDataAsync();

    private Task<PagingResult<CmPost>> QueryPostsAsync(PagingCriteria criteria)
    {
        criteria.SetQuery(nameof(CmPost.Type), QueryType.Equal, Type);
        return Service.QueryPostsAsync(criteria);
    }
}