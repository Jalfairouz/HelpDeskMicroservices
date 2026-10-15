using System.Linq.Expressions;
using System.Security.Claims;
using HelpDesk.NotificationService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.DTOs;

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

    private static readonly Expression<Func<Inbox, NotificationDto>> ToDto =
        i => new NotificationDto(
            i.Id,
            i.NotificationId,
            i.Notification.TicketId,
            i.Notification.Title,
            i.Notification.Message,
            i.Notification.CreatedAt,
            i.IsRead,
            i.ReadAt);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int take = 30,
        [FromQuery] int skip = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        take = Math.Clamp(take, 1, 100);
        skip = Math.Max(skip, 0);

        var items = await _dbContext.Inboxes
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.Notification.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(ToDto)
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var items = await _dbContext.Inboxes
            .AsNoTracking()
            .Where(i => i.UserId == userId && !i.IsRead)
            .OrderByDescending(i => i.Notification.CreatedAt)
            .Take(100)
            .Select(ToDto)
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var count = await _dbContext.Inboxes
            .CountAsync(i => i.UserId == userId && !i.IsRead, cancellationToken);

        return Ok(new { UnreadCount = count });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var item = await _dbContext.Inboxes
            .AsNoTracking()
            .Where(i => i.Id == id && i.UserId == userId)
            .Select(ToDto)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var inbox = await _dbContext.Inboxes
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId, cancellationToken);

        if (inbox is null)
            return NotFound();

        if (!inbox.IsRead)
        {
            inbox.IsRead = true;
            inbox.ReadAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(new { inbox.Id, inbox.IsRead, inbox.ReadAt });
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var now = DateTime.UtcNow;

        var updated = await _dbContext.Inboxes
            .Where(i => i.UserId == userId && !i.IsRead)
            .ExecuteUpdateAsync(
                s => s.SetProperty(i => i.IsRead, true)
                      .SetProperty(i => i.ReadAt, now),
                cancellationToken);

        return Ok(new { Updated = updated });
    }

    private bool TryGetUserId(out Guid userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }
}