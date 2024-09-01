using CodeWF.Core.CategoryFeature;
using CodeWF.Data;
using CodeWF.Data.Entities;
using CodeWF.WebAPI.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace CodeWF.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        [ProducesResponseType<CategoryEntity>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([NotEmpty] Guid id)
        {
            var cat = await mediator.Send(new GetCategoryQuery(id));
            if (null == cat) return NotFound();

            return Ok(cat);
        }

        [HttpGet("list")]
        [ProducesResponseType<List<CategoryEntity>>(StatusCodes.Status200OK)]
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

        [HttpPut("{id:guid}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
        public async Task<IActionResult> Update([NotEmpty] Guid id, UpdateCategoryCommand command)
        {
            command.Id = id;
            var oc = await mediator.Send<OperationCode>(command);
            if (oc == OperationCode.ObjectNotFound) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
        public async Task<IActionResult> Delete([NotEmpty] Guid id)
        {
            var oc = await mediator.Send(new DeleteCategoryCommand(id));
            if (oc == OperationCode.ObjectNotFound) return NotFound();

            return NoContent();
        }
    }
}