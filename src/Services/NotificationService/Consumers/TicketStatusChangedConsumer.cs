using IntegrationContracts.Events;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public sealed class TicketStatusChangedConsumer : IConsumer<TicketStatusChangedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly IUserDirectory _directory;

    public TicketStatusChangedConsumer(INotificationDispatcher dispatcher, IUserDirectory directory)
    {
        _dispatcher = dispatcher;
        _directory = directory;
    }

    public async Task Consume(ConsumeContext<TicketStatusChangedEvent> context)
    {
        var m = context.Message;
        var ct = context.CancellationToken;

        var recipients = new[] { (Guid?)m.CreatorId, m.AssignedTechnicianId }
            .Where(id => id.HasValue && id.Value != m.ChangedByUserId)
            .Select(id => id!.Value)
            .ToList();

        if (recipients.Count == 0)
            return;

        var names = await _directory.GetNamesAsync(new[] { m.ChangedByUserId }, ct);

        await _dispatcher.DispatchAsync(m.EventId, m.TicketId,
        [
            new NotificationDraft(
                "participants",
                $"Ticket status: {m.NewStatus}",
                $"{names.NameOf(m.ChangedByUserId)} changed the status from {m.OldStatus} to {m.NewStatus}: {m.TicketTitle}",
                recipients)
        ],
        ct);
    }
}