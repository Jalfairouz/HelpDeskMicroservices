using HelpDesk.NotificationService.Data;
using IntegrationContracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Consumers


{
    public sealed class TicketStatusChangedConsumer : IConsumer <TicketStatusChangedEvent>
    {
        private readonly ILogger<TicketStatusChangedConsumer> _logger;
        private readonly NotificationDbContext _dbContext;
        public TicketStatusChangedConsumer(ILogger<TicketStatusChangedConsumer> logger, NotificationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task Consume(ConsumeContext<TicketStatusChangedEvent> context)
        {
            var ticket = context.Message;
            var alreadyExists = await _dbContext.Notifications.AnyAsync(n => n.EventId == ticket.EventId);
            if (alreadyExists) { _logger.LogInformation("Notification already exists for event {EventId}.", ticket.EventId); return; }
            
            var notification = new Notification
            {
                EventId = ticket.EventId,
                TicketId = ticket.TicketId,
                Title = $"Ticket status changed {ticket.NewStatus}",
                Message = $"Ticket '{ticket.TicketTitle}' status changed from {ticket.OldStatus} to {ticket.NewStatus}.",
                CreatedAt = DateTime.UtcNow
            };
            var inbox = new Inbox
            {
                UserId = ticket.CreatorId,
                IsRead = false,
                DeliveredAt = DateTime.UtcNow,
                Notification = notification
            };
            await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                context.CancellationToken);
            _dbContext.Notifications.Add(notification);
            _dbContext.Inboxes.Add(inbox);

            await _dbContext.SaveChangesAsync(
                context.CancellationToken);

            await transaction.CommitAsync(
                context.CancellationToken);

            _logger.LogInformation(
                "Notification {NotificationId} saved for ticket {TicketId}.",
                notification.Id,
                notification.TicketId);
        }

    }
}
