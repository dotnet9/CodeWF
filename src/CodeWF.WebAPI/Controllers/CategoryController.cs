using CodeWF.Core.Categories;
using CodeWF.EventBus;
using Microsoft.AspNetCore.Mvc;

namespace CodeWF.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(IEventBus eventBus) : ControllerBase
    {
        [HttpGet("{slug}")]
        [ProducesResponseType<GetCategoryBySlugResponse>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromRoute] string slug)
        {
            var response = await eventBus.QueryAsync(new GetCategoryBySlugQuery(slug));
            return Ok(response);
        }
    }
}