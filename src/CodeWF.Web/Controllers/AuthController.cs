namespace CodeWF.Web.Controllers;

[Route("auth")]
public class AuthController(IOptions<AuthenticationSettings> authSettings) : ControllerBase
{
    private readonly AuthenticationSettings _authenticationSettings = authSettings.Value;

    [HttpGet("signout")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> SignOut(int nounce = 1055)
    {
        switch (_authenticationSettings.Provider)
        {
            case AuthenticationProvider.EntraID:
                string? callbackUrl = Url.Page("/Index", null, null, Request.Scheme);
                return SignOut(
                    new AuthenticationProperties { RedirectUri = callbackUrl },
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIdConnectDefaults.AuthenticationScheme);
            case AuthenticationProvider.Local:
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToPage("/Index");
            default:
                return RedirectToPage("/Index");
        }
    }

    [AllowAnonymous]
    [HttpGet("/account/accessdenied")]
    [HttpGet("accessdenied")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return Content("Access Denied");
    }
}