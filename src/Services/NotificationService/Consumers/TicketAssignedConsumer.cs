using HelpDesk.NotificationService.Data;
using IntegrationContracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Consumers;

public sealed class TicketAssignedConsumer
    : IConsumer<TicketAssignedEvent>
{
    private readonly NotificationDbContext _dbContext;
    private readonly ILogger<TicketAssignedConsumer> _logger;

    public TicketAssignedConsumer(
        NotificationDbContext dbContext,
        ILogger<TicketAssignedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<TicketAssignedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Assignment event received. Ticket={TicketId}, Assignee={AssigneeId}",
            message.TicketId,
            message.AssigneeId);

        if (message.AssigneeId == Guid.Empty)
        {
            _logger.LogError(
                "Cannot create notification: AssigneeId is empty for ticket {TicketId}",
                message.TicketId);

            throw new InvalidOperationException(
                "TicketAssignedEvent has an invalid AssigneeId.");
        }

        var eventId = context.MessageId
            ?? throw new InvalidOperationException(
                "The assignment event must have a message ID.");

        if (await _dbContext.Notifications.AnyAsync(
                n => n.EventId == eventId,
                context.CancellationToken))
        {
            _logger.LogInformation(
                "Event {EventId} has already been processed.",
                eventId);

            return;
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                context.CancellationToken);

        var notification = new Notification
        {
            EventId = eventId,
            TicketId = message.TicketId,
            Title = "Ticket assigned",
            Message = $"Ticket '{message.TicketTitle}' has been assigned to you.",
            CreatedAt = DateTime.UtcNow
        };

        var inbox = new Inbox
        {
            Notification = notification,
            UserId = message.AssigneeId,
            IsRead = false,
            DeliveredAt = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        _dbContext.Inboxes.Add(inbox);

        await _dbContext.SaveChangesAsync(
            context.CancellationToken);

        await transaction.CommitAsync(
            context.CancellationToken);

        _logger.LogInformation(
            "Notification saved. InboxId={InboxId}, UserId={UserId}, TicketId={TicketId}",
            inbox.Id,
            inbox.UserId,
            message.TicketId);
    }
}
