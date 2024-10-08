namespace CodeWF.Admin.Pages.Interact;

[Route("/ims/logs")]
public class ImLogList : BaseTablePage<CmLog>
{
    private IPostService Service;

    protected override async Task OnPageInitAsync()
    {
        await base.OnPageInitAsync();
        Service = await CreateServiceAsync<IPostService>();
        Table.OnQuery = Service.QueryLogsAsync;
    }

    public async void Export() => await ExportDataAsync();
}