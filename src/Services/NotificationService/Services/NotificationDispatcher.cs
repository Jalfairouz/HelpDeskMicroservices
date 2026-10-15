using HelpDesk.NotificationService.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NotificationService.DTOs;
using NotificationService.Hubs;
using NotificationService.Models;

namespace NotificationService.Services;

public sealed record NotificationDraft(string Audience,string Title,string Message, IEnumerable<Guid> Recipients);

public interface INotificationDispatcher
{
    Task DispatchAsync( Guid eventId,Guid ticketId,
        IEnumerable<NotificationDraft> drafts,
        CancellationToken cancellationToken);
}

public sealed class NotificationDispatcher : INotificationDispatcher
{
    public const string ReceiveMethod = "ReceiveNotification";

    private readonly NotificationDbContext _db;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        NotificationDbContext db,
        IHubContext<NotificationHub> hub,
        ILogger<NotificationDispatcher> logger)
    {
        _db = db;
        _hub = hub;
        _logger = logger;
    }

    public async Task DispatchAsync( Guid eventId, Guid ticketId,
        IEnumerable<NotificationDraft> drafts,
        CancellationToken cancellationToken)
    {
        var created = new List<Inbox>();

        foreach (var draft in drafts)
        {
            var recipients = draft.Recipients
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (recipients.Count == 0)
                continue;

            var alreadyProcessed = await _db.Notifications.AnyAsync(
                n => n.EventId == eventId && n.Audience == draft.Audience,
                cancellationToken);

            if (alreadyProcessed)
            {
                _logger.LogInformation(
                    "Event {EventId} ({Audience}) already processed.",
                    eventId, draft.Audience);
                continue;
            }

            var notification = new Notification
            {
                EventId = eventId,
                Audience = draft.Audience,
                TicketId = ticketId,
                Title = draft.Title,
                Message = draft.Message,
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);

            foreach (var userId in recipients)
            {
                var inbox = new Inbox
                {
                    Notification = notification,
                    UserId = userId,
                    DeliveredAt = DateTime.UtcNow
                };

                _db.Inboxes.Add(inbox);
                created.Add(inbox);
            }
        }

        if (created.Count == 0)
            return;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            _logger.LogInformation("Duplicate event {EventId} ignored.", eventId);
            return;
        }
        foreach (var inbox in created)
        {
            var dto = new NotificationDto(
                inbox.Id,
                inbox.NotificationId,
                inbox.Notification.TicketId,
                inbox.Notification.Title,
                inbox.Notification.Message,
                inbox.Notification.CreatedAt,
                inbox.IsRead,
                inbox.ReadAt);

            try
            {
                await _hub.Clients
                    .User(inbox.UserId.ToString())
                    .SendAsync(ReceiveMethod, dto, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to push notification {InboxId} to user {UserId}.",
                    inbox.Id, inbox.UserId);
            }
        }
    }
}