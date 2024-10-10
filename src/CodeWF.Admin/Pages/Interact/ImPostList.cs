namespace CodeWF.Admin.Pages.Interact;

[Route("/ims/posts")]
public class ImPostList : BaseTablePage<CmPost>
{
    private IPostService Service;

    protected override async Task OnPageInitAsync()
    {
        await base.OnPageInitAsync();
        Service = await CreateServiceAsync<IPostService>();
        Table.OnQuery = QueryPostsAsync;
    }

    public void DeleteM() => Table.DeleteM(Service.DeletePostsAsync);
    public void Edit(CmPost row) => Table.EditForm(Service.SavePostAsync, row);
    public void Delete(CmPost row) => Table.Delete(Service.DeletePostsAsync, row);
    public async void Export() => await Table.ExportDataAsync();

    private Task<PagingResult<CmPost>> QueryPostsAsync(PagingCriteria criteria)
    {
        criteria.SetQuery(nameof(CmPost.Type), QueryType.Equal, ContentType.Interact.ToString());
        return Service.QueryPostsAsync(criteria);
    }
}