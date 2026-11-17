using IntegrationContracts.Events;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public sealed class TicketAssignedConsumer : IConsumer<TicketAssignedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly IUserDirectory _directory;

    public TicketAssignedConsumer(INotificationDispatcher dispatcher, IUserDirectory directory)
    {
        _dispatcher = dispatcher;
        _directory = directory;
    }

    public async Task Consume(ConsumeContext<TicketAssignedEvent> context)
    {
        var m = context.Message;
        var ct = context.CancellationToken;

        if (m.AssigneeId == Guid.Empty)
            throw new InvalidOperationException(
                "TicketAssignedEvent has an invalid AssigneeId.");

        var names = await _directory.GetNamesAsync(
            new[] { m.AssignedByUserId, m.AssigneeId }, ct);

        var creatorRecipients = m.CreatorId == m.AssignedByUserId
            ? Array.Empty<Guid>()
            : new[] { m.CreatorId };

        await _dispatcher.DispatchAsync(m.EventId, m.TicketId,
        [
            new NotificationDraft(
                "assignee",
                "Ticket assigned to you",
                $"{names.NameOf(m.AssignedByUserId)} assigned you: {m.TicketTitle}",
                new[] { m.AssigneeId }),

            new NotificationDraft(
                "creator",
                "Your ticket was assigned",
                $"{names.NameOf(m.AssigneeId)} will handle: {m.TicketTitle}",
                creatorRecipients)
        ],
        ct);
    }
}