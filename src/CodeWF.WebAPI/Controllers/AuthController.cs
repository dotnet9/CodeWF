using CodeWF.Auth;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodeWF.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator, ILogger<AuthController> logger) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] ValidateLoginCommand command)
        {
            var ua = Request.Headers["User-Agent"].ToString();
            if (string.IsNullOrWhiteSpace(ua))
            {
                return Unauthorized();
            }

            var isValid = await mediator.Send(command);
            if (isValid)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, command.Account),
                    new(ClaimTypes.Role, "Administrator")
                };
                var ci = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var p = new ClaimsPrincipal(ci);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, p);
                //await mediator.Send(new LogSuccessLoginCommand(Helper.GetClientIP(HttpContext), ua, "TODO"));

                var successMessage = $@"Authentication success for local account ""{command.Account}""";

                logger.LogInformation(successMessage);
            }

            return Ok(isValid);
        }
    }
}