
namespace IntegrationContracts.Events;

public sealed record TicketAssignedEvent
{
    public Guid TicketId { get; init; }

    public string TicketTitle { get; init; } = string.Empty;

    public Guid AssigneeId { get; init; }

    public Guid CreatorId { get; init; }
}