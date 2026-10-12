
using HelpDesk.NotificationService.Data;
using HelpDesk.NotificationService.Models;
using IntegrationContracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.NotificationService.Consumers;

public sealed class TicketCreatedConsumer
    : IConsumer<TicketCreatedEvent>
{
    private readonly ILogger<TicketCreatedConsumer> _logger;
    private readonly NotificationDbContext _dbContext;

    public TicketCreatedConsumer(
        ILogger<TicketCreatedConsumer> logger,
        NotificationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Consume(
        ConsumeContext<TicketCreatedEvent> context)
    {
        var ticket = context.Message;

        var alreadyExists = await _dbContext.Notifications
            .AnyAsync(n => n.EventId == ticket.EventId);

        if (alreadyExists)
        {
            _logger.LogInformation(
                "Notification already exists for event {EventId}.",
                ticket.EventId);

            return;
        }

        var notification = new Notification
        {
            EventId = ticket.EventId,
            TicketId = ticket.TicketId,
             = ticket.CreatorId,
            Title = "New Ticket successfully created.",
            Message = $"Ticket Created: {ticket.TicketTitle}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Notification {NotificationId} saved for ticket {TicketId}.",
            notification.Id,
            notification.TicketId);
    }
}