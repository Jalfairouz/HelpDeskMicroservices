
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
    public async Task<IActionResult> GetAll()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var inboxItems = await _dbContext.Inboxes
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.Notification.CreatedAt)
            .Select(i => new
            {
                i.Id,
                i.NotificationId,
                i.Notification.EventId,
                i.Notification.TicketId,
                i.Notification.Title,
                i.Notification.Message,
                i.Notification.CreatedAt,
                i.IsRead,
                i.ReadAt
            })
            .ToListAsync();

        return Ok(inboxItems);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var notification = await _dbContext.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n =>
                n.Id == id && n.UserId == userId);

        if (notification is null)
            return NotFound();

        return Ok(notification);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n =>n.Id == id && n.UserId == userId);

        if (notification is null)
            return NotFound();

        notification.IsRead = true;

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            notification.Id,
            notification.IsRead
        });
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var count = await _dbContext.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return Ok(new
        {
            UnreadCount = count
        });
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var notifications = await _dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            UpdatedCount = notifications.Count
        });
    }

}