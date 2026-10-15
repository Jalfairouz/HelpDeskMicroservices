using IntegrationContracts.Events;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public sealed class TicketCreatedConsumer : IConsumer<TicketCreatedEvent>
{
    private readonly INotificationDispatcher _dispatcher;

    public TicketCreatedConsumer(INotificationDispatcher dispatcher)
        => _dispatcher = dispatcher;

    public Task Consume(ConsumeContext<TicketCreatedEvent> context)
    {
        var m = context.Message;

        var recipients = m.AssignedTechnicianId is { } technicianId
            ? new[] { technicianId }
            : Array.Empty<Guid>();

        return _dispatcher.DispatchAsync(m.EventId, m.TicketId,
        [
            new NotificationDraft(
                "assignee",
                "New ticket assigned to you",
                $"Ticket '{m.TicketTitle}' was assigned to you.",
                recipients)
        ],
        context.CancellationToken);
    }
}