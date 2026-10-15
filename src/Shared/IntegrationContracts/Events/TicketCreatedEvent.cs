namespace IntegrationContracts.Events;

public sealed record TicketCreatedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid TicketId { get; init; }
    public string TicketTitle { get; init; } = string.Empty;
    public Guid CreatorId { get; init; }
    public Guid? AssignedTechnicianId { get; init; }
}