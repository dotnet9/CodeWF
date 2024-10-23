namespace CodeWF.Pages;

public class ListComponent<T> : BaseComponent
{
    protected PagingCriteria Criteria = new();

    protected PagingResult<T> Result = new();

    [SupplyParameterFromQuery] public int Page { get; set; }

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();
        if (Page <= 0)
            Page = 1;
        Criteria.PageIndex = Page;
    }
}