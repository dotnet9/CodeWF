namespace CodeWF.Admin.Pages.BaseData;

[Route("/bds/users")]
public class BdUserList : BaseTablePage<CmUser>
{
    private ICommonService Service;

    protected override async Task OnPageInitAsync()
    {
        await base.OnPageInitAsync();
        Service = await CreateServiceAsync<ICommonService>();
        Table.OnQuery = Service.QueryUsersAsync;
    }

    public async void Export() => await ExportDataAsync();
}