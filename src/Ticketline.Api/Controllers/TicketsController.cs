using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketline.Api.Dtos;
using Ticketline.Api.Services;

namespace Ticketline.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController(TicketService tickets) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TicketResponse>>> GetMine([FromQuery] TicketQuery query)
    {
        var page = await tickets.GetMineAsync(UserId(), query);
        return Ok(page);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketResponse>> Get(int id)
    {
        var ticket = await tickets.GetAsync(UserId(), id);
        return ticket == null ? NotFound(new { message = "Заявка не найдена." }) : Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> Create(CreateTicketRequest request)
    {
        var ticket = await tickets.CreateAsync(UserId(), request);
        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketResponse>> Update(int id, UpdateTicketRequest request)
    {
        try
        {
            var ticket = await tickets.UpdateAsync(UserId(), id, request);
            return ticket == null ? NotFound(new { message = "Заявка не найдена." }) : Ok(ticket);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private int UserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(value!);
    }
}
