namespace IntegrationContracts.Events
{
    public interface TicketStatusChanged
    {
        Guid EventId { get; }

        Guid TicketId { get; }
        string TicketTitle { get; }
        Guid CreatorId { get; }
        string OldStatus { get; }
        string NewStatus { get; }
    }
}
