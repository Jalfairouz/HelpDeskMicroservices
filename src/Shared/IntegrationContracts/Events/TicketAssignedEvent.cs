namespace IntegrationContracts.Events
{
    public interface TicketAssignedEvent
    {
        Guid EventId { get; }

        Guid TicketId { get; }
        string TicketTitle { get; }
        Guid AssigneeId { get; }
        Guid CreatorId { get; } 
    }
}
