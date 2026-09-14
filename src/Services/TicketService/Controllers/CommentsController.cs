
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketService.DTOs.Requests;
using TicketService.DTOs.Responses;
using TicketService.Models;
using TicketService.Services;

namespace TicketService.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:guid}/comments")]
[Authorize]
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

        var ticket = await _ticketsService.GetByIdAsync(
            ticketId, userId, role);

        if (ticket is null)
        {
            return NotFound("Ticket not found.");
        }

        // 2️⃣ هل أنت صاحب أو تقني أو admin؟
        var canComment = role switch
        {
            RoleNames.Employee => ticket.CreatedByUserId == userId,
            RoleNames.Technician => ticket.AssignedTechnicianId == userId,
            RoleNames.Admin => true,
            _ => false
        };

        if (!canComment)
        {
            return Forbid();
        }

        // 3️⃣ أضف التعليق
        var comment = await _commentService.AddAsync(
            ticketId, request, userId);

        return StatusCode(201, comment);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetComments(
        Guid ticketId)
    {
        if (!TryGetCaller(out var userId, out var role))
        {
            return Unauthorized();
        }

        // 1️⃣ التذكرة موجودة والصلاحية؟
        var ticket = await _ticketsService.GetByIdAsync(
            ticketId, userId, role);

        if (ticket is null)
        {
            return NotFound("Ticket not found.");
        }

        // 2️⃣ اجلب التعليقات
        var comments = await _commentService
            .GetByTicketIdAsync(ticketId);

        return Ok(comments);
    }

    private bool TryGetCaller(out Guid userId, out string role)
    {
        role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out userId) &&
               !string.IsNullOrWhiteSpace(role);
    }
}