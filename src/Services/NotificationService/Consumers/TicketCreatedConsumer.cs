using IntegrationContracts.Events;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public sealed class TicketCreatedConsumer : IConsumer<TicketCreatedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly IUserDirectory _directory;

    public TicketCreatedConsumer(INotificationDispatcher dispatcher, IUserDirectory directory)
    {
        _dispatcher = dispatcher;
        _directory = directory;
    }

    public async Task Consume(ConsumeContext<TicketCreatedEvent> context)
    {
        var m = context.Message;
        var ct = context.CancellationToken;

        var names = await _directory.GetNamesAsync(new[] { m.CreatorId }, ct);
        var creator = names.NameOf(m.CreatorId);

        NotificationDraft draft;

        if (m.AssignedTechnicianId is { } technicianId)
        {
            draft = new NotificationDraft(
                "assignee",
                "New ticket assigned to you",
                $"{creator} created: {m.TicketTitle}",
                new[] { technicianId });
        }
        else
        {
            var admins = (await _directory.GetAdminIdsAsync(ct))
                .Where(id => id != m.CreatorId)
                .ToList();

            draft = new NotificationDraft(
                "admins",
                "Ticket needs assignment",
                $"{creator} created a ticket with no available technician: {m.TicketTitle}",
                admins);
        }

        await _dispatcher.DispatchAsync(m.EventId, m.TicketId, new[] { draft }, ct);
    }
}