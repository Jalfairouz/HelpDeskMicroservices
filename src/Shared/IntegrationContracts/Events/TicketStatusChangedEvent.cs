namespace IntegrationContracts.Events
{
    public sealed record TicketStatusChangedEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid TicketId { get; init; }
        public string TicketTitle { get; init; } = string.Empty;
        public Guid CreatorId { get; init; }
        public string OldStatus { get; init; }
        public string NewStatus { get; init; }
    }
}
