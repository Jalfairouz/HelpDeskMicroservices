namespace IntegrationContracts.Events;

public sealed record ApiResponseLoggedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string CorrelationId { get; init; } = string.Empty;
    public DateTime RequestTimestamp { get; init; }       
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public int StatusCode { get; init; }
    public string? Error { get; init; }
    public long DurationMs { get; init; }
    public long? ResponseSize { get; init; }
}