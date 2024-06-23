using CodeWF.Core.Abouts;
using CodeWF.EventBus;
using Microsoft.AspNetCore.Mvc;

namespace CodeWF.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutController(IEventBus eventBus) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<GetAboutResponse>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var about = await eventBus.QueryAsync(new GetAboutQuery());
            return Ok(about);
        }
    }
}
