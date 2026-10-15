using IntegrationContracts.Events;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public sealed class TicketAssignedConsumer : IConsumer<TicketAssignedEvent>
{
    private readonly INotificationDispatcher _dispatcher;

    public TicketAssignedConsumer(INotificationDispatcher dispatcher)
        => _dispatcher = dispatcher;

    public Task Consume(ConsumeContext<TicketAssignedEvent> context)
    {
        var m = context.Message;

        if (m.AssigneeId == Guid.Empty)
            throw new InvalidOperationException(
                "TicketAssignedEvent has an invalid AssigneeId.");

        var creatorRecipients = m.CreatorId == m.AssignedByUserId
            ? Array.Empty<Guid>()
            : new[] { m.CreatorId };

        return _dispatcher.DispatchAsync(m.EventId, m.TicketId,
        [
            new NotificationDraft(
                "assignee",
                "Ticket assigned",
                $"Ticket '{m.TicketTitle}' has been assigned to you.",
                new[] { m.AssigneeId }),

            new NotificationDraft(
                "creator",
                "Your ticket was assigned",
                $"A technician has been assigned to your ticket '{m.TicketTitle}'.",
                creatorRecipients)
        ],
        context.CancellationToken);
    }
}