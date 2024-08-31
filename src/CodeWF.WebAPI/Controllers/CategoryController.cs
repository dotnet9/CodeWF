using CodeWF.Core.Categories;
using CodeWF.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace CodeWF.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(IMediator mediator) : ControllerBase
    {
        [HttpGet("list")]
        [ProducesResponseType<List<Category>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            var list = await mediator.Send(new GetCategoriesQuery());
            return Ok(list);
        }

        [HttpGet("{slug}")]
        [ProducesResponseType<CategoryAttribute>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromRoute] string slug)
        {
            var response = await mediator.Send(new GetCategoryBySlugQuery(slug));
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(CreateCategoryCommand command)
        {
            await mediator.Send(command);
            return Created(string.Empty, command);
        }
    }
}