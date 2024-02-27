using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeWF.Web.Pages.Admin;

public class LocalAccountModel(IMediator mediator) : PageModel
{
    public CreateAccountCommand ViewModel { get; set; } = new();

    public IReadOnlyList<Account> Accounts { get; set; }

    public async Task OnGet() => Accounts = await mediator.Send(new GetAccountsQuery());
}