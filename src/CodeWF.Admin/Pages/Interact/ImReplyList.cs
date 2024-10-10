namespace CodeWF.Admin.Pages.Interact;

[Route("/ims/replies")]
public class ImReplyList : BaseTablePage<CmReply>
{
    private IPostService Service;

    protected override async Task OnPageInitAsync()
    {
        await base.OnPageInitAsync();
        Service = await CreateServiceAsync<IPostService>();
        Table.OnQuery = Service.QueryRepliesAsync;
    }

    //public void DeleteM() => Table.DeleteM(Service.DeletePostsAsync);
    //public void Delete(CmPost row) => Table.Delete(Service.DeletePostsAsync, row);
    public async void Export() => await Table.ExportDataAsync();
}