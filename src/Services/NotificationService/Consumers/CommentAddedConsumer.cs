using IntegrationContracts.Events;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public sealed class CommentAddedConsumer : IConsumer<CommentAddedEvent>
{
    private readonly INotificationDispatcher _dispatcher;

    public CommentAddedConsumer(INotificationDispatcher dispatcher)
        => _dispatcher = dispatcher;

    public Task Consume(ConsumeContext<CommentAddedEvent> context)
    {
        var m = context.Message;

        var recipients = new[] { (Guid?)m.CreatorId, m.AssignedTechnicianId }
            .Where(id => id.HasValue && id.Value != m.AuthorId)
            .Select(id => id!.Value);

        return _dispatcher.DispatchAsync(m.EventId, m.TicketId,
        [
            new NotificationDraft(
                "participants",
                "New comment",
                $"A new comment was added on ticket '{m.TicketTitle}'.",
                recipients)
        ],
        context.CancellationToken);
    }
}