using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketService.DTOs.Requests;
using TicketService.DTOs.Responses;
using TicketService.Services;

namespace TicketService.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:guid}/comments")]
[Authorize(Roles = "Employee,Technician,Admin")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ITicketsService _ticketsService;

    public CommentsController(
        ICommentService commentService,
        ITicketsService ticketsService)
    {
        _commentService = commentService;
        _ticketsService = ticketsService;
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> AddComment(
        Guid ticketId,
        [FromBody] AddCommentRequest request)
    {
        if (!TryGetCaller(out var userId, out var role))
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
                return NotFound(new
                {
                    message = "Ticket not found."
                });
            }

            var comment = await _commentService.AddAsync(
                ticketId,
                request,
                userId);

            return StatusCode(
                StatusCodes.Status201Created,
                comment);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetComments(
        Guid ticketId)
    {
        if (!TryGetCaller(out var userId, out var role))
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
                return NotFound(new
                {
                    message = "Ticket not found."
                });
            }

            var comments = await _commentService
                .GetByTicketIdAsync(ticketId);

            return Ok(comments);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private bool TryGetCaller(
        out Guid userId,
        out string role)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        role = User.FindFirstValue(
            ClaimTypes.Role) ?? string.Empty;

        var hasValidUserId = Guid.TryParse(
            userIdValue,
            out userId);

        var hasRole = !string.IsNullOrWhiteSpace(role);

        return hasValidUserId && hasRole;
    }
}