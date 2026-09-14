using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketService.DTOs.Requests;
using TicketService.DTOs.Responses;
using TicketService.Services;

namespace TicketService.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketsService _ticketsService;

    public TicketsController(
        ITicketsService ticketsService)
    {
        _ticketsService = ticketsService;
    }

    [HttpPost]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<ActionResult<TicketResponse>> Create(
        [FromBody] CreateTicketRequest request)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var response =
            await _ticketsService.CreateAsync(
                request,
                userId);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }
    [HttpGet]
    [Authorize(Roles = "Employee,Technician,Admin")]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAll()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var role =
            User.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized();
        }

        var tickets =
            await _ticketsService.GetAllAsync(userId, role);

        return Ok(tickets);
    }


    [HttpGet("{ticketId:guid}")]
    [Authorize(Roles = "Employee,Technician,Admin")]
    public async Task<ActionResult<TicketResponse>> GetById(
        Guid ticketId)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var role =
            User.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized();
        }

        try
        {
            var ticket = await _ticketsService.GetByIdAsync(
                ticketId,
                userId,
                role);

            if (ticket is null)
            {
                return NotFound();
            }

            return Ok(ticket);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    [HttpPut("{ticketId:guid}")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<ActionResult<TicketResponse>> Update(
    Guid ticketId,
    [FromBody] UpdateTicketRequest request)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var role =
            User.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized();
        }

        try
        {
            var ticket = await _ticketsService.UpdateAsync(
                ticketId,
                request,
                userId,
                role);

            if (ticket is null)
            {
                return NotFound();
            }

            return Ok(ticket);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }
    [HttpDelete("{ticketId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid ticketId)
    {
        var deleted =
            await _ticketsService.DeleteAsync(ticketId);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}