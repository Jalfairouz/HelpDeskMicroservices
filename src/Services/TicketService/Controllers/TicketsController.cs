
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketService.DTOs.Requests;
using TicketService.DTOs.Responses;
using TicketService.Models;
using TicketService.Services;

namespace TicketService.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketsService _ticketsService;
    private readonly IAssignmentService _assignmentService;
    public TicketsController(ITicketsService ticketsService, IAssignmentService assignmentService )
    {
        _ticketsService = ticketsService;
        _assignmentService = assignmentService;
    }



    [HttpPost]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<ActionResult<TicketResponse>> CreateTicket( [FromBody] CreateTicketRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _ticketsService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetTicketById), new { ticketId = response.Id }, response);
    }


    [HttpPatch("{ticketId:guid}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TicketResponse>> AssignTicket(Guid ticketId, [FromBody] AssignTicketRequest request)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var ticket = await _assignmentService.AssignAsync(ticketId, request.TechnicianId!.Value, adminId);

            if (ticket is null)
            {
                return NotFound(new { message = "Ticket not found." });
            }

            return Ok(ticket);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAllTickets([FromQuery] TicketFilter filter)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tickets = await _ticketsService.GetAllTicketsAsync(userId, filter);
        return Ok(tickets);
    }

    [HttpGet("system")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetSystemTickets([FromQuery] TicketFilter filter)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tickets = await _ticketsService.GetSystemTicketsAsync(adminId, filter);
        return Ok(tickets);
    }

    [HttpGet("assigned")]
    [Authorize(Roles = "Technician")]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAssignedTickets()
    {
        var technicianId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tickets = await _ticketsService.GetAssignedTicketsAsync(technicianId);
        return Ok(tickets);
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<ActionResult<TicketDetailsResponse>> GetTicketById(Guid ticketId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        try
        {
            var ticket = await _ticketsService.GetByIdAsync(ticketId, userId, role);
            if (ticket is null) return NotFound();
            return Ok(ticket);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }





    [HttpPut("{ticketId:guid}")]
    [Authorize(Roles = "Technician,Admin")]
    public async Task<ActionResult<TicketResponse>> UpdateTicket(Guid ticketId, [FromBody] UpdateTicketRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        try
        {
            var ticket = await _ticketsService.UpdateAsync(ticketId, request, userId, role);
            if (ticket is null) return NotFound();
            return Ok(ticket);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }




    [HttpPatch("{ticketId:guid}/status")]
    [Authorize(Roles = "Technician,Admin")]
    public async Task<ActionResult<TicketResponse>> ChangeStatusTicket(Guid ticketId, [FromBody] ChangeTicketStatusRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        try
        {
            var ticket = await _ticketsService.ChangeStatusAsync(ticketId, request, userId, role);
            if (ticket is null) return NotFound();
            return Ok(ticket);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{ticketId:guid}/history")]
    public async Task<ActionResult<IEnumerable<TicketHistoryResponse>>> GetTicketHistory( Guid ticketId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized();
        }

        try
        {
            var history = await _ticketsService.GetHistoryAsync( ticketId, userId, role);

            if (history is null)
            {
                return NotFound(new { message = "Ticket not found." });
            }

            return Ok(history);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{ticketId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTicket(Guid ticketId)
    {
        var deleted = await _ticketsService.DeleteAsync(ticketId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}