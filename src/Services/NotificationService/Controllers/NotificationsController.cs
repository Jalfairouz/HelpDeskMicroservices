
using HelpDesk.NotificationService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpDesk.NotificationService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _dbContext;

    public NotificationsController(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }



    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var notifications = await _dbContext.Inboxes
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.Notification.CreatedAt)
            .Select(i => new
            {
                Id = i.Id, 
                i.NotificationId,
                i.Notification.EventId,
                i.Notification.TicketId,
                i.Notification.Title,
                i.Notification.Message,
                i.Notification.CreatedAt,
                i.IsRead,
                i.ReadAt
            })
            .ToListAsync(cancellationToken);

        return Ok(notifications);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var notifications = await _dbContext.Inboxes
            .AsNoTracking()
            .Where(i => i.UserId == userId && !i.IsRead)
            .OrderByDescending(i => i.Notification.CreatedAt)
            .Select(i => new
            {
                Id = i.Id,
                i.NotificationId,
                i.Notification.TicketId,
                i.Notification.Title,
                i.Notification.Message,
                i.Notification.CreatedAt,
                i.IsRead
            })
            .ToListAsync(cancellationToken);

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var count = await _dbContext.Inboxes
            .CountAsync(i => i.UserId == userId && !i.IsRead);

        return Ok(new { UnreadCount = count });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById( Guid id )
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var notification = await _dbContext.Inboxes
            .AsNoTracking()
            .Where(i => i.Id == id && i.UserId == userId)
            .Select(i => new
            {
                Id = i.Id,
                i.NotificationId,
                i.Notification.EventId,
                i.Notification.TicketId,
                i.Notification.Title,
                i.Notification.Message,
                i.Notification.CreatedAt,
                i.IsRead,
                i.ReadAt
            })
            .FirstOrDefaultAsync();

        if (notification is null)
            return NotFound();

        return Ok(notification);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead( Guid id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var inbox = await _dbContext.Inboxes
            .FirstOrDefaultAsync(
                i => i.Id == id && i.UserId == userId);

        if (inbox is null)
            return NotFound();

        if (!inbox.IsRead)
        {
            inbox.IsRead = true;
            inbox.ReadAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        return Ok(new
        {
            inbox.Id,
            inbox.IsRead,
            inbox.ReadAt
        });
    }
    private bool TryGetUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdClaim, out userId);
    }
}