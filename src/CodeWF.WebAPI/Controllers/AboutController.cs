using CodeWF.Core.Abouts;
using CodeWF.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CodeWF.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<About>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var about = await mediator.Send(new GetAboutQuery());
            return Ok(about);
        }
    }
}