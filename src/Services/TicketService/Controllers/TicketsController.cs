
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

    public TicketsController(ITicketsService ticketsService)
    {
        _ticketsService = ticketsService;
    }



    [HttpPost]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<ActionResult<TicketResponse>> Create(
        [FromBody] CreateTicketRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _ticketsService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { ticketId = response.Id }, response);
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
    [Authorize(Roles = "Employee,Technician,Admin")]
    public async Task<ActionResult<TicketResponse>> Update(
        Guid ticketId,
        [FromBody] UpdateTicketRequest request)
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
    public async Task<ActionResult<TicketResponse>> ChangeStatus(
        Guid ticketId,
        [FromBody] ChangeTicketStatusRequest request)
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



    [HttpDelete("{ticketId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid ticketId)
    {
        var deleted = await _ticketsService.DeleteAsync(ticketId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}