
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
    public async Task<ActionResult<TicketResponse>> Create( [FromBody] CreateTicketRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _ticketsService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { ticketId = response.Id }, response);
    }


    [HttpPatch("{ticketId:guid}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TicketResponse>> Assign(Guid ticketId, [FromBody] AssignTicketRequest request)
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
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var tickets = await _ticketsService.GetAllAsync(userId, role);
        return Ok(tickets);
    }




    [HttpGet("{ticketId:guid}")]
    public async Task<ActionResult<TicketResponse>> GetById(Guid ticketId)
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
    public async Task<ActionResult<TicketResponse>> Update(Guid ticketId, [FromBody] UpdateTicketRequest request)
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
    public async Task<ActionResult<TicketResponse>> ChangeStatus(Guid ticketId, [FromBody] ChangeTicketStatusRequest request)
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
    public async Task<ActionResult<IEnumerable<TicketHistoryResponse>>> GetHistory( Guid ticketId)
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
    public async Task<IActionResult> Delete(Guid ticketId)
    {
        var deleted = await _ticketsService.DeleteAsync(ticketId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}