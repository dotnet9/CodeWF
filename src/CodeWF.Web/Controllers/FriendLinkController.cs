namespace CodeWF.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FriendLinkController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(AddLinkCommand command)
    {
        await mediator.Send(command);
        return Created(new Uri(command.LinkUrl), command);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<FriendLinkEntity>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([NotEmpty] Guid id)
    {
        FriendLinkEntity? link = await mediator.Send(new GetLinkQuery(id));
        if (null == link)
        {
            return NotFound();
        }

        return Ok(link);
    }

    [HttpGet("list")]
    [ProducesResponseType<List<FriendLinkEntity>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List()
    {
        IReadOnlyList<FriendLinkEntity> list = await mediator.Send(new GetAllLinksQuery());
        return Ok(list);
    }

    [HttpGet("list2")]
    [ProducesResponseType<List<FriendLinkDto>>(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> List2()
    {
        IReadOnlyList<FriendLinkEntity> list = await mediator.Send(new GetAllLinksQuery());
        return Ok(list.Select(link =>
            new FriendLinkDto() { Title = link.Title, Rank = link.Rank, LinkUrl = link.LinkUrl }));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update([NotEmpty] Guid id, UpdateLinkCommand command)
    {
        command.Id = id;
        await mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([NotEmpty] Guid id)
    {
        await mediator.Send(new DeleteLinkCommand(id));
        return NoContent();
    }
}