using IntegrationContracts.Events;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public sealed class CommentAddedConsumer : IConsumer<CommentAddedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly IUserDirectory _directory;

    public CommentAddedConsumer(INotificationDispatcher dispatcher, IUserDirectory directory)
    {
        _dispatcher = dispatcher;
        _directory = directory;
    }

    public async Task Consume(ConsumeContext<CommentAddedEvent> context)
    {
        var m = context.Message;
        var ct = context.CancellationToken;

        var recipients = new[] { (Guid?)m.CreatorId, m.AssignedTechnicianId }
            .Where(id => id.HasValue && id.Value != m.AuthorId)
            .Select(id => id!.Value)
            .ToList();

        if (recipients.Count == 0)
            return;

        var names = await _directory.GetNamesAsync(new[] { m.AuthorId }, ct);

        await _dispatcher.DispatchAsync(m.EventId, m.TicketId,
        [
            new NotificationDraft(
                "participants",
                "New comment",
                $"{names.NameOf(m.AuthorId)} commented on: {m.TicketTitle}",
                recipients)
        ],
        ct);
    }
}