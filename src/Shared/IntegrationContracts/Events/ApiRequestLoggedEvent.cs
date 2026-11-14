namespace IntegrationContracts.Events;

public sealed record ApiRequestLoggedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string CorrelationId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Method { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string? Query { get; init; }
    public string? Service { get; init; }
    public string? UserId { get; init; }
    public string? Role { get; init; }
    public string? ClientIp { get; init; }
    public string? UserAgent { get; init; }
    public long? RequestSize { get; init; }
}