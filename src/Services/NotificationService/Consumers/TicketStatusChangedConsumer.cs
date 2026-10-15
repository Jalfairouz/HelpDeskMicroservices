using IntegrationContracts.Events;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public sealed class TicketStatusChangedConsumer : IConsumer<TicketStatusChangedEvent>
{
    private readonly INotificationDispatcher _dispatcher;

    public TicketStatusChangedConsumer(INotificationDispatcher dispatcher)
        => _dispatcher = dispatcher;

    public Task Consume(ConsumeContext<TicketStatusChangedEvent> context)
    {
        var m = context.Message;

        var recipients = new[] { (Guid?)m.CreatorId, m.AssignedTechnicianId }
            .Where(id => id.HasValue && id.Value != m.ChangedByUserId)
            .Select(id => id!.Value);

        return _dispatcher.DispatchAsync(m.EventId, m.TicketId,
        [
            new NotificationDraft(
                "participants",
                $"Ticket status: {m.NewStatus}",
                $"Ticket '{m.TicketTitle}' changed from {m.OldStatus} to {m.NewStatus}.",
                recipients)
        ],
        context.CancellationToken);
    }
}